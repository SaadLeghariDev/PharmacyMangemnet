using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class SaleReturnService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    INumberSequenceService sequences) : ISaleReturnService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    private long? CurrentUserId() => currentUser.UserId;

    public async Task<PagedResult<SaleReturnDto>> SearchAsync(SaleReturnQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = Query().Where(r => r.Branch.TenantId == tenantId);
        if (query.BranchId is long branchId) q = q.Where(r => r.BranchId == branchId);
        if (query.SaleId is long saleId) q = q.Where(r => r.SaleId == saleId);
        if (!string.IsNullOrWhiteSpace(query.Status)) q = q.Where(r => r.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(r => r.ReturnNumber.Contains(s));
        }

        q = q.OrderByDescending(r => r.Id);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<SaleReturnDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<SaleReturnDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await Query().FirstOrDefaultAsync(r => r.Id == id && r.Branch.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<SaleReturnDto> CreateAsync(CreateSaleReturnRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var sale = await db.Sales
            .Include(s => s.Branch)
            .Include(s => s.SaleLines).ThenInclude(l => l.SaleLineBatches)
            .Include(s => s.SalePayments)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId && s.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Sale {request.SaleId} not found.");

        if (sale.Status is "Voided" or "Draft")
            throw new ValidationAppException([$"Sale status '{sale.Status}' cannot be returned."]);

        var returnNumber = await sequences.AllocateNextAsync(
            tenantId, DocumentTypes.SaleReturn, sale.BranchId, null, "RET-", ct);

        var entity = new SaleReturn
        {
            SaleId = sale.Id,
            BranchId = sale.BranchId,
            ReturnNumber = returnNumber,
            ReturnDate = DateTime.UtcNow,
            Reason = request.Reason,
            Status = "Draft",
            RefundAmount = 0,
            RefundPaymentMethodId = request.RefundPaymentMethodId,
            CreatedBy = CurrentUserId()
        };

        decimal refundTotal = 0;
        foreach (var lineReq in request.Lines)
        {
            var saleLine = sale.SaleLines.FirstOrDefault(l => l.Id == lineReq.SaleLineId)
                ?? throw new ValidationAppException([$"Sale line {lineReq.SaleLineId} not on sale {sale.Id}."]);

            if (lineReq.ProductUnitId != saleLine.ProductUnitId)
                throw new ValidationAppException([$"Return line unit must match sale line unit {saleLine.ProductUnitId}."]);

            var soldOnBatch = saleLine.SaleLineBatches
                .Where(b => b.BatchId == lineReq.BatchId)
                .Sum(b => b.Quantity);
            if (soldOnBatch <= 0)
                throw new ValidationAppException([$"Batch {lineReq.BatchId} was not sold on sale line {saleLine.Id}."]);

            var alreadyReturned = await db.SaleReturnLines
                .Where(rl =>
                    rl.SaleLineId == saleLine.Id &&
                    rl.BatchId == lineReq.BatchId &&
                    rl.SaleReturn.Status != "Cancelled")
                .SumAsync(rl => (decimal?)rl.Quantity, ct) ?? 0;

            if (alreadyReturned + lineReq.Quantity > soldOnBatch)
                throw new ValidationAppException([
                    $"Return qty exceeds sold qty for line {saleLine.Id} batch {lineReq.BatchId} (sold {soldOnBatch}, returned {alreadyReturned}, request {lineReq.Quantity})."]);

            var refundPrice = lineReq.RefundPrice
                ?? Math.Round(saleLine.NetAmount / saleLine.Quantity, 4);

            var returnToStock = IsGoodCondition(lineReq.Condition);
            var destination = await ResolveDestinationAsync(
                sale, lineReq.BatchId, lineReq.DestinationLocationId, returnToStock, ct);

            entity.SaleReturnLines.Add(new SaleReturnLine
            {
                SaleLineId = saleLine.Id,
                ProductId = saleLine.ProductId,
                BatchId = lineReq.BatchId,
                ProductUnitId = lineReq.ProductUnitId,
                Quantity = lineReq.Quantity,
                RefundPrice = refundPrice,
                Condition = lineReq.Condition,
                ReturnToStock = returnToStock,
                DestinationLocationId = destination
            });

            refundTotal += Math.Round(lineReq.Quantity * refundPrice, 4);
        }

        entity.RefundAmount = Math.Round(refundTotal, 4);
        db.SaleReturns.Add(entity);
        await db.SaveChangesAsync(ct);

        if (request.PostImmediately)
            return await PostAsync(entity.Id, ct);

        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<SaleReturnDto> PostAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var ret = await db.SaleReturns
                .Include(r => r.Branch)
                .Include(r => r.SaleReturnLines)
                .Include(r => r.Sale).ThenInclude(s => s.SaleLines).ThenInclude(l => l.SaleLineBatches)
                .Include(r => r.Sale).ThenInclude(s => s.SalePayments)
                .FirstOrDefaultAsync(r => r.Id == id && r.Branch.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Sale return {id} not found.");

            if (ret.Status is not ("Draft" or "Approved"))
                throw new ValidationAppException([$"Only Draft/Approved returns can be posted (current: {ret.Status})."]);

            if (ret.SaleReturnLines.Count == 0)
                throw new ValidationAppException(["Return has no lines."]);

            var now = DateTime.UtcNow;
            var userId = CurrentUserId();
            var movIndex = 0;

            foreach (var line in ret.SaleReturnLines)
            {
                var productUnit = await db.ProductUnits.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == line.ProductUnitId && u.ProductId == line.ProductId, ct)
                    ?? throw new ValidationAppException([$"Product unit {line.ProductUnitId} not found."]);

                var baseUnit = await db.ProductUnits.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.ProductId == line.ProductId && u.IsBaseUnit && u.IsActive, ct)
                    ?? throw new ValidationAppException([$"Base unit missing for product {line.ProductId}."]);

                var baseQty = Math.Round(line.Quantity * productUnit.ConversionToBase, 6);
                var batch = await db.InventoryBatches.FirstAsync(b => b.Id == line.BatchId, ct);

                var destLocId = line.DestinationLocationId
                    ?? throw new ValidationAppException([$"Return line {line.Id} missing destination location."]);

                var destLocMeta = await db.WarehouseLocations.AsNoTracking()
                    .FirstOrDefaultAsync(l => l.Id == destLocId && l.IsActive, ct)
                    ?? throw new ValidationAppException([$"Destination location {destLocId} not found."]);

                var loc = await db.InventoryBatchLocations
                    .FirstOrDefaultAsync(l => l.BatchId == line.BatchId && l.WarehouseLocationId == destLocId, ct);

                decimal balanceBefore;
                if (loc is null)
                {
                    balanceBefore = 0;
                    loc = new InventoryBatchLocation
                    {
                        BatchId = line.BatchId,
                        WarehouseLocationId = destLocId,
                        QuantityOnHand = baseQty,
                        ReservedQuantity = 0,
                        UpdatedAt = now,
                        RowVersion = new byte[8]
                    };
                    db.InventoryBatchLocations.Add(loc);
                }
                else
                {
                    balanceBefore = loc.QuantityOnHand;
                    loc.QuantityOnHand += baseQty;
                    loc.UpdatedAt = now;
                }

                // Non-saleable returns go to quarantine location; optionally mark batch quarantine when all restock is non-good.
                if (!line.ReturnToStock && batch.BatchStatus == BatchStatuses.Available)
                {
                    // Keep batch Available for mixed locations; quarantine is location-typed.
                    // If destination is quarantine type, leave batch status unchanged (location segregates).
                    _ = destLocMeta.LocationType;
                }

                db.InventoryMovements.Add(new InventoryMovement
                {
                    BranchId = ret.BranchId,
                    WarehouseId = destLocMeta.WarehouseId,
                    WarehouseLocationId = destLocId,
                    ProductId = line.ProductId,
                    BatchId = line.BatchId,
                    ProductUnitId = baseUnit.Id,
                    MovementType = MovementTypes.Return,
                    ReferenceType = "SaleReturn",
                    ReferenceId = ret.Id,
                    Quantity = baseQty,
                    UnitCost = batch.PurchaseCost,
                    TotalCost = Math.Round(baseQty * batch.PurchaseCost, 4),
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceBefore + baseQty,
                    MovementDate = now,
                    PerformedBy = userId,
                    IdempotencyKey = $"RET:{ret.Id}:MOV:{++movIndex}",
                    CreatedAt = now
                });
            }

            // Refund payment on original sale.
            if (ret.RefundAmount > 0 && ret.RefundPaymentMethodId is long pmId)
            {
                if (!await db.PaymentMethods.AnyAsync(m => m.Id == pmId && m.IsActive, ct))
                    throw new ValidationAppException(["Refund payment method not found."]);

                db.SalePayments.Add(new SalePayment
                {
                    SaleId = ret.SaleId,
                    PaymentMethodId = pmId,
                    Amount = ret.RefundAmount,
                    PaymentDate = now,
                    Status = "Completed",
                    TransactionType = "Refund"
                });

                if (ret.Sale.PaymentStatus != "Refunded")
                    ret.Sale.PaymentStatus = "Refunded";
            }

            // Update original sale status based on returned qty vs sold.
            await UpdateSaleReturnStatusAsync(ret.Sale, ct);

            ret.Status = "Posted";
            ret.ApprovedBy ??= userId;

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("Stock location was modified concurrently. Reload and retry.");
            }

            if (tx is not null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            throw;
        }

        return (await GetByIdAsync(id, ct))!;
    }

    private async Task UpdateSaleReturnStatusAsync(Sale sale, CancellationToken ct)
    {
        var soldQty = sale.SaleLines.Sum(l => l.Quantity);
        var returnedQty = await db.SaleReturnLines
            .Where(rl => rl.SaleLine.SaleId == sale.Id && rl.SaleReturn.Status != "Cancelled")
            .SumAsync(rl => (decimal?)rl.Quantity, ct) ?? 0;

        if (returnedQty <= 0) return;

        sale.Status = returnedQty >= soldQty ? "Returned" : "PartiallyReturned";
        sale.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<long> ResolveDestinationAsync(
        Sale sale,
        long batchId,
        long? requestedLocationId,
        bool returnToStock,
        CancellationToken ct)
    {
        if (requestedLocationId is long locId)
        {
            if (!await db.WarehouseLocations.AnyAsync(l => l.Id == locId && l.IsActive, ct))
                throw new ValidationAppException([$"Destination location {locId} not found."]);
            return locId;
        }

        var batch = await db.InventoryBatches.AsNoTracking().FirstAsync(b => b.Id == batchId, ct);

        if (returnToStock)
        {
            // Prefer a location that previously held this batch in the warehouse; else first selling location.
            var prior = await db.InventoryBatchLocations.AsNoTracking()
                .Where(l => l.BatchId == batchId)
                .OrderByDescending(l => l.QuantityOnHand)
                .Select(l => (long?)l.WarehouseLocationId)
                .FirstOrDefaultAsync(ct);
            if (prior is long p) return p;

            var selling = await db.WarehouseLocations.AsNoTracking()
                .Where(l => l.WarehouseId == batch.WarehouseId && l.IsActive)
                .OrderBy(l => l.Id)
                .Select(l => (long?)l.Id)
                .FirstOrDefaultAsync(ct);
            if (selling is null)
                throw new ValidationAppException(["No destination location available for restock."]);
            return selling.Value;
        }

        var quarantine = await db.WarehouseLocations.AsNoTracking()
            .Where(l =>
                l.WarehouseId == batch.WarehouseId &&
                l.IsActive &&
                l.LocationType == "Quarantine")
            .OrderBy(l => l.Id)
            .Select(l => (long?)l.Id)
            .FirstOrDefaultAsync(ct);
        if (quarantine is null)
            throw new ValidationAppException(["No quarantine location configured for warehouse."]);

        return quarantine.Value;
    }

    private static bool IsGoodCondition(string condition) =>
        string.Equals(condition, "Good", StringComparison.OrdinalIgnoreCase);

    private IQueryable<SaleReturn> Query() =>
        db.SaleReturns.AsNoTracking()
            .Include(r => r.SaleReturnLines)
            .Include(r => r.Branch);

    private static SaleReturnDto Map(SaleReturn r) => new()
    {
        Id = r.Id,
        SaleId = r.SaleId,
        BranchId = r.BranchId,
        ReturnNumber = r.ReturnNumber,
        ReturnDate = r.ReturnDate,
        Reason = r.Reason,
        Status = r.Status,
        RefundAmount = r.RefundAmount,
        RefundPaymentMethodId = r.RefundPaymentMethodId,
        CreatedBy = r.CreatedBy,
        ApprovedBy = r.ApprovedBy,
        Lines = r.SaleReturnLines.Select(l => new SaleReturnLineDto
        {
            Id = l.Id,
            SaleLineId = l.SaleLineId,
            ProductId = l.ProductId,
            BatchId = l.BatchId,
            ProductUnitId = l.ProductUnitId,
            Quantity = l.Quantity,
            RefundPrice = l.RefundPrice,
            Condition = l.Condition,
            ReturnToStock = l.ReturnToStock,
            DestinationLocationId = l.DestinationLocationId
        }).ToList()
    };
}
