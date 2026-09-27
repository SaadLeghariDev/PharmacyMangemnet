using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Prescriptions;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class PrescriptionService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    INumberSequenceService sequences) : IPrescriptionService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    private long? CurrentUserId() => currentUser.UserId;

    public async Task<PagedResult<PrescriptionDto>> SearchAsync(PrescriptionQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.Prescriptions.AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Doctor)
            .Where(p => p.Customer.TenantId == tenantId);

        if (query.CustomerId is long customerId) q = q.Where(p => p.CustomerId == customerId);
        if (query.DoctorId is long doctorId) q = q.Where(p => p.DoctorId == doctorId);
        if (!string.IsNullOrWhiteSpace(query.Status)) q = q.Where(p => p.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(p => p.PrescriptionNumber.Contains(s) || p.Customer.Name.Contains(s));
        }

        q = q.OrderByDescending(p => p.Id);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return new PagedResult<PrescriptionDto>
        {
            Items = items.Select(MapSummary).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<PrescriptionDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await QueryDetail()
            .FirstOrDefaultAsync(p => p.Id == id && p.Customer.TenantId == tenantId, ct);
        return entity is null ? null : MapDetail(entity);
    }

    public async Task<PrescriptionDto> CreateAsync(CreatePrescriptionRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var userId = CurrentUserId();
        var now = DateTime.UtcNow;

        var customer = await db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && c.TenantId == tenantId && c.IsActive, ct)
            ?? throw new ValidationAppException(["Customer not found."]);

        if (!await db.Doctors.AnyAsync(d => d.Id == request.DoctorId && d.IsActive, ct))
            throw new ValidationAppException(["Doctor not found."]);

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.TenantId == tenantId && p.IsActive)
            .Select(p => p.Id)
            .ToListAsync(ct);
        if (products.Count != productIds.Count)
            throw new ValidationAppException(["One or more products are invalid."]);

        var number = await sequences.AllocateNextAsync(
            tenantId, DocumentTypes.Prescription, null, null, "RX-", ct);

        var entity = new Prescription
        {
            CustomerId = customer.Id,
            DoctorId = request.DoctorId,
            PrescriptionNumber = number,
            PrescriptionDate = request.PrescriptionDate ?? DateOnly.FromDateTime(now),
            DiagnosisNotes = request.DiagnosisNotes,
            Status = "Active",
            CreatedBy = userId,
            CreatedAt = now
        };

        foreach (var item in request.Items)
        {
            entity.PrescriptionItems.Add(new PrescriptionItem
            {
                ProductId = item.ProductId,
                DosageAmount = item.DosageAmount,
                DosageUnit = item.DosageUnit,
                FrequencyCode = item.FrequencyCode,
                Route = item.Route,
                DurationValue = item.DurationValue,
                DurationUnit = item.DurationUnit,
                Quantity = Math.Round(item.Quantity, 6),
                Instructions = item.Instructions,
                RefillAllowed = item.RefillAllowed,
                RefillCount = item.RefillCount
            });
        }

        db.Prescriptions.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<PrescriptionDto> CancelAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Prescriptions
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id && p.Customer.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Prescription {id} not found.");

        if (entity.Status is "Dispensed" or "Cancelled")
            throw new ValidationAppException([$"Cannot cancel prescription in status {entity.Status}."]);

        if (await db.DispensingRecords.AnyAsync(d => d.PrescriptionId == entity.Id, ct))
            throw new ValidationAppException(["Cannot cancel a prescription that has dispensing records."]);

        entity.Status = "Cancelled";
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<PrescriptionDto> DispenseAsync(long id, DispensePrescriptionRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var userId = CurrentUserId();
        var now = DateTime.UtcNow;

        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var rx = await db.Prescriptions
                .Include(p => p.Customer)
                .Include(p => p.Doctor)
                .Include(p => p.PrescriptionItems)
                .FirstOrDefaultAsync(p => p.Id == id && p.Customer.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Prescription {id} not found.");

            if (rx.Status is "Cancelled" or "Expired")
                throw new ValidationAppException([$"Cannot dispense prescription in status {rx.Status}."]);

            var sale = await db.Sales
                .Include(s => s.Branch)
                .Include(s => s.SaleLines).ThenInclude(l => l.SaleLineBatches).ThenInclude(b => b.Batch)
                .Include(s => s.SaleLines).ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(s => s.Id == request.SaleId && s.Branch.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Sale {request.SaleId} not found.");

            if (sale.Status != "Completed")
                throw new ValidationAppException(["Only Completed sales can be dispensed against."]);

            if (sale.CustomerId != rx.CustomerId)
                throw new ValidationAppException(["Sale customer does not match prescription customer."]);

            if (await db.DispensingRecords.AnyAsync(d => d.SaleId == sale.Id && d.PrescriptionId == rx.Id, ct))
                throw new ConflictException("This sale is already dispensed against this prescription.");

            var priorDispensed = await db.DispensingItems
                .Where(di => di.PrescriptionItem.PrescriptionId == rx.Id)
                .GroupBy(di => di.PrescriptionItemId)
                .Select(g => new { PrescriptionItemId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.PrescriptionItemId, x => x.Qty, ct);

            var linkedLines = sale.SaleLines
                .Where(l => l.PrescriptionItemId is not null)
                .ToList();
            if (linkedLines.Count == 0)
                throw new ValidationAppException([
                    "Sale has no lines linked to prescription items (set PrescriptionItemId on sale lines)."]);

            var record = new DispensingRecord
            {
                PrescriptionId = rx.Id,
                SaleId = sale.Id,
                CustomerId = rx.CustomerId,
                DispensedBy = userId,
                DispensedAt = now
            };

            foreach (var line in linkedLines)
            {
                var itemId = line.PrescriptionItemId!.Value;
                var rxItem = rx.PrescriptionItems.FirstOrDefault(i => i.Id == itemId)
                    ?? throw new ValidationAppException([$"Sale line links unknown prescription item {itemId}."]);

                if (line.ProductId != rxItem.ProductId)
                    throw new ValidationAppException([
                        $"Sale line product {line.ProductId} does not match prescription item product {rxItem.ProductId}."]);

                var already = priorDispensed.GetValueOrDefault(itemId);
                var remaining = Math.Round(rxItem.Quantity - already, 6);
                var lineQty = Math.Round(line.BaseQuantity, 6);
                if (lineQty > remaining)
                {
                    // Allow refill when RefillAllowed and allotted refill count not exhausted
                    var refillUsed = await db.Refills.CountAsync(r => r.PrescriptionItemId == itemId, ct);
                    if (!(rxItem.RefillAllowed && refillUsed < rxItem.RefillCount && already >= rxItem.Quantity))
                        throw new ValidationAppException([
                            $"Dispense qty {lineQty} exceeds remaining {remaining} for prescription item {itemId}."]);
                }

                if (line.SaleLineBatches.Count == 0)
                    throw new ValidationAppException([$"Sale line {line.Id} has no batch allocations."]);

                foreach (var batch in line.SaleLineBatches)
                {
                    record.DispensingItems.Add(new DispensingItem
                    {
                        PrescriptionItemId = itemId,
                        ProductId = line.ProductId,
                        BatchId = batch.BatchId,
                        Quantity = Math.Round(batch.BaseQuantity, 6)
                    });
                }

                // Refill bookkeeping when dispensing beyond original allotted qty after prior full dispense
                if (already >= rxItem.Quantity && rxItem.RefillAllowed)
                {
                    db.Refills.Add(new Refill
                    {
                        PrescriptionItemId = itemId,
                        RefillDate = now,
                        Quantity = lineQty,
                        SaleId = sale.Id,
                        DispensedBy = userId,
                        Notes = $"Refill via sale {sale.InvoiceNumber}"
                    });
                }

                priorDispensed[itemId] = already + lineQty;
            }

            db.DispensingRecords.Add(record);

            foreach (var di in record.DispensingItems)
            {
                var product = await db.Products.AsNoTracking()
                    .FirstAsync(p => p.Id == di.ProductId, ct);
                if (!product.IsControlled) continue;

                var register = await db.ControlledDrugRegisters
                    .FirstOrDefaultAsync(r =>
                        r.BranchId == sale.BranchId &&
                        r.ProductId == di.ProductId &&
                        r.IsActive, ct)
                    ?? throw new ValidationAppException([
                        $"No active controlled-drug register for product {di.ProductId} at branch {sale.BranchId}."]);

                if (register.CurrentBalance < di.Quantity)
                    throw new ValidationAppException([
                        $"Controlled register {register.RegisterNumber} balance {register.CurrentBalance} insufficient for {di.Quantity}."]);

                var before = register.CurrentBalance;
                var after = Math.Round(before - di.Quantity, 6);
                register.CurrentBalance = after;

                db.ControlledDrugTransactions.Add(new ControlledDrugTransaction
                {
                    RegisterId = register.Id,
                    TransactionType = "Dispense",
                    ReferenceType = "Sale",
                    ReferenceId = sale.Id,
                    Quantity = di.Quantity,
                    BalanceBefore = before,
                    BalanceAfter = after,
                    PrescriptionId = rx.Id,
                    DoctorId = rx.DoctorId,
                    PerformedBy = userId,
                    WitnessedBy = request.WitnessedBy,
                    TransactionDate = now,
                    Remarks = $"Dispense {rx.PrescriptionNumber} / {sale.InvoiceNumber}"
                });
            }

            // Persist dispense rows first so status recompute can join Sale/PrescriptionItem FKs
            await db.SaveChangesAsync(ct);
            await RecalculateRxStatusAsync(db, rx, ct);
            await db.SaveChangesAsync(ct);
            if (tx is not null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            throw;
        }

        return (await GetByIdAsync(id, ct))!;
    }

    /// <summary>Shared status recompute for dispense and sale-void paths.</summary>
    public static async Task RecalculateRxStatusAsync(
        PharmacyManagementDbContext db, Prescription rx, CancellationToken ct)
    {
        if (rx.Status is "Cancelled" or "Expired") return;

        var items = rx.PrescriptionItems.Count > 0
            ? rx.PrescriptionItems.ToList()
            : await db.PrescriptionItems.Where(i => i.PrescriptionId == rx.Id).ToListAsync(ct);

        var itemIds = items.Select(i => i.Id).ToList();
        var voidedSaleIds = await db.Sales.AsNoTracking()
            .Where(s => s.Status == "Voided")
            .Select(s => s.Id)
            .ToListAsync(ct);

        var dispensed = await db.DispensingItems
            .Where(di => itemIds.Contains(di.PrescriptionItemId))
            .Where(di => !voidedSaleIds.Contains(di.DispensingRecord.SaleId))
            .GroupBy(di => di.PrescriptionItemId)
            .Select(g => new { PrescriptionItemId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.PrescriptionItemId, x => x.Qty, ct);

        var any = false;
        var all = true;
        foreach (var item in items)
        {
            var qty = dispensed.GetValueOrDefault(item.Id);
            if (qty > 0) any = true;
            if (qty < item.Quantity) all = false;
        }

        rx.Status = !any ? "Active" : all ? "Dispensed" : "PartiallyDispensed";
    }

    private IQueryable<Prescription> QueryDetail() =>
        db.Prescriptions.AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Doctor)
            .Include(p => p.PrescriptionItems).ThenInclude(i => i.Product)
            .Include(p => p.DispensingRecords).ThenInclude(d => d.Sale)
            .Include(p => p.DispensingRecords).ThenInclude(d => d.DispensingItems).ThenInclude(i => i.Batch);

    private static PrescriptionDto MapSummary(Prescription p) => new()
    {
        Id = p.Id,
        CustomerId = p.CustomerId,
        CustomerName = p.Customer?.Name,
        DoctorId = p.DoctorId,
        DoctorName = p.Doctor?.Name,
        PrescriptionNumber = p.PrescriptionNumber,
        PrescriptionDate = p.PrescriptionDate,
        DiagnosisNotes = p.DiagnosisNotes,
        Status = p.Status,
        CreatedBy = p.CreatedBy,
        CreatedAt = p.CreatedAt,
        Items = Array.Empty<PrescriptionItemDto>(),
        DispensingRecords = Array.Empty<DispensingRecordDto>()
    };

    private static PrescriptionDto MapDetail(Prescription p)
    {
        var dispensedByItem = p.DispensingRecords
            .Where(d => d.Sale is null || d.Sale.Status != "Voided")
            .SelectMany(d => d.DispensingItems)
            .GroupBy(i => i.PrescriptionItemId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        return new PrescriptionDto
        {
            Id = p.Id,
            CustomerId = p.CustomerId,
            CustomerName = p.Customer?.Name,
            DoctorId = p.DoctorId,
            DoctorName = p.Doctor?.Name,
            PrescriptionNumber = p.PrescriptionNumber,
            PrescriptionDate = p.PrescriptionDate,
            DiagnosisNotes = p.DiagnosisNotes,
            Status = p.Status,
            CreatedBy = p.CreatedBy,
            CreatedAt = p.CreatedAt,
            Items = p.PrescriptionItems.Select(i =>
            {
                var dispensed = dispensedByItem.GetValueOrDefault(i.Id);
                return new PrescriptionItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Sku = i.Product?.Sku,
                    ProductName = i.Product?.Name,
                    DosageAmount = i.DosageAmount,
                    DosageUnit = i.DosageUnit,
                    FrequencyCode = i.FrequencyCode,
                    Route = i.Route,
                    DurationValue = i.DurationValue,
                    DurationUnit = i.DurationUnit,
                    Quantity = i.Quantity,
                    Instructions = i.Instructions,
                    RefillAllowed = i.RefillAllowed,
                    RefillCount = i.RefillCount,
                    DispensedQuantity = dispensed,
                    RemainingQuantity = Math.Max(0, Math.Round(i.Quantity - dispensed, 6))
                };
            }).ToList(),
            DispensingRecords = p.DispensingRecords.Select(d => new DispensingRecordDto
            {
                Id = d.Id,
                PrescriptionId = d.PrescriptionId,
                SaleId = d.SaleId,
                CustomerId = d.CustomerId,
                DispensedBy = d.DispensedBy,
                DispensedAt = d.DispensedAt,
                Items = d.DispensingItems.Select(i => new DispensingItemDto
                {
                    Id = i.Id,
                    PrescriptionItemId = i.PrescriptionItemId,
                    ProductId = i.ProductId,
                    BatchId = i.BatchId,
                    BatchNumber = i.Batch?.BatchNumber,
                    Quantity = i.Quantity
                }).ToList()
            }).ToList()
        };
    }
}
