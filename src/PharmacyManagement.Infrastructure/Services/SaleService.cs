using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class SaleService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    INumberSequenceService sequences) : ISaleService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    private long RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedAppException("User scope required.");

    public async Task<PagedResult<SaleDto>> SearchAsync(SaleQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = Query().Where(s => s.Branch.TenantId == tenantId);

        if (query.BranchId is long branchId) q = q.Where(s => s.BranchId == branchId);
        if (query.TerminalId is long terminalId) q = q.Where(s => s.TerminalId == terminalId);
        if (query.CustomerId is long customerId) q = q.Where(s => s.CustomerId == customerId);
        if (!string.IsNullOrWhiteSpace(query.Status)) q = q.Where(s => s.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.PaymentStatus)) q = q.Where(s => s.PaymentStatus == query.PaymentStatus);
        if (query.FromDate is DateTime from) q = q.Where(s => s.SaleDate >= from);
        if (query.ToDate is DateTime to) q = q.Where(s => s.SaleDate <= to);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.InvoiceNumber.Contains(s));
        }

        q = q.OrderByDescending(s => s.Id);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<SaleDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<SaleDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await Query().FirstOrDefaultAsync(s => s.Id == id && s.Branch.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<SaleReceiptDto?> GetReceiptAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var sale = await db.Sales.AsNoTracking()
            .Include(s => s.Branch)
            .Include(s => s.Counter)
            .Include(s => s.Terminal)
            .Include(s => s.User)
            .Include(s => s.Customer)
            .Include(s => s.SaleLines).ThenInclude(l => l.SaleLineBatches).ThenInclude(b => b.Batch)
            .Include(s => s.SaleLines).ThenInclude(l => l.Product)
            .Include(s => s.SalePayments).ThenInclude(p => p.PaymentMethod)
            .FirstOrDefaultAsync(s => s.Id == id && s.Branch.TenantId == tenantId, ct);
        if (sale is null) return null;

        return new SaleReceiptDto
        {
            Sale = Map(sale),
            CustomerName = sale.Customer?.Name,
            CustomerCode = sale.Customer?.CustomerCode,
            BranchName = sale.Branch.Name,
            CounterCode = sale.Counter.Code,
            TerminalCode = sale.Terminal.TerminalCode,
            CashierName = sale.User.FullName
        };
    }

    public async Task<SaleDto> CreateAsync(CreateSaleRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var userId = RequireUserId();
        var now = DateTime.UtcNow;
        long saleId;

        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existingKey = await db.IdempotencyKeys.AsNoTracking()
                    .FirstOrDefaultAsync(k =>
                        k.TerminalId == request.TerminalId &&
                        k.Key == request.IdempotencyKey.Trim(), ct);
                if (existingKey?.EntityId is long existingSaleId)
                {
                    if (tx is not null) await tx.RollbackAsync(ct);
                    return (await GetByIdAsync(existingSaleId, ct))
                        ?? throw new ConflictException("Idempotent sale exists but could not be loaded.");
                }
            }

            await ValidateHeaderAsync(tenantId, request, ct);

            var paymentMethods = await LoadPaymentMethodsAsync(request.Payments, ct);

            var invoiceNumber = await sequences.AllocateNextAsync(
                tenantId, DocumentTypes.Sale, request.BranchId, request.TerminalId, "INV-", ct);

            var sale = new Sale
            {
                BranchId = request.BranchId,
                CounterId = request.CounterId,
                TerminalId = request.TerminalId,
                UserId = userId,
                CustomerId = request.CustomerId,
                InvoiceNumber = invoiceNumber,
                SaleDate = now,
                SaleType = request.SaleType,
                Status = "Completed",
                CurrencyCode = request.CurrencyCode.ToUpperInvariant(),
                RoundOff = Math.Round(request.RoundOff, 4),
                Fbrstatus = "Pending",
                CreatedAt = now,
                UpdatedAt = now,
                RowVersion = new byte[8]
            };

            var pendingMovements = new List<(InventoryBatchLocation Location, InventoryBatch Batch, long ProductId, long BaseUnitId, decimal BaseQty)>();
            decimal subtotal = 0, discountTotal = 0, taxTotal = 0;
            var lineIndex = 0;

            foreach (var lineReq in request.Lines)
            {
                lineIndex++;
                if (!await db.Products.AnyAsync(p =>
                        p.Id == lineReq.ProductId && p.TenantId == tenantId && p.IsActive && p.IsSaleable, ct))
                    throw new ValidationAppException([$"Line {lineIndex}: product {lineReq.ProductId} not found or not saleable."]);

                var productUnit = await db.ProductUnits.AsNoTracking()
                    .FirstOrDefaultAsync(u =>
                        u.Id == lineReq.ProductUnitId && u.ProductId == lineReq.ProductId && u.IsActive, ct)
                    ?? throw new ValidationAppException([$"Line {lineIndex}: product unit {lineReq.ProductUnitId} invalid."]);

                if (!productUnit.IsSaleUnit && !productUnit.IsBaseUnit)
                    throw new ValidationAppException([$"Line {lineIndex}: product unit is not a sale unit."]);

                if (productUnit.ConversionToBase <= 0)
                    throw new ValidationAppException([$"Line {lineIndex}: invalid ConversionToBase."]);

                var baseUnit = await db.ProductUnits.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.ProductId == lineReq.ProductId && u.IsBaseUnit && u.IsActive, ct)
                    ?? throw new ValidationAppException([$"Line {lineIndex}: base unit missing."]);

                var allocations = await AllocateBatchesAsync(
                    tenantId, request.WarehouseId, lineReq, productUnit, lineIndex, ct);

                var unitPrice = lineReq.UnitPrice
                    ?? await ResolveUnitPriceAsync(lineReq.ProductId, lineReq.ProductUnitId, allocations, productUnit, ct);

                var mrp = allocations.Count > 0
                    ? Math.Round(allocations[0].Batch.Mrp * productUnit.ConversionToBase, 4)
                    : unitPrice;

                var lineGross = Math.Round(lineReq.Quantity * unitPrice, 4);
                var lineDiscount = Math.Round(lineReq.DiscountAmount, 4);
                var lineTax = Math.Round(lineReq.TaxAmount, 4);
                var lineNet = Math.Round(lineGross - lineDiscount + lineTax, 4);
                if (lineNet < 0)
                    throw new ValidationAppException([$"Line {lineIndex}: net amount cannot be negative."]);

                var baseQty = Math.Round(lineReq.Quantity * productUnit.ConversionToBase, 6);
                var allocatedBase = allocations.Sum(a => a.BaseQuantity);
                if (allocatedBase != baseQty)
                    throw new ValidationAppException([
                        $"Line {lineIndex}: allocated base qty {allocatedBase} does not match required {baseQty}."]);

                var saleLine = new SaleLine
                {
                    ProductId = lineReq.ProductId,
                    ProductUnitId = lineReq.ProductUnitId,
                    Quantity = lineReq.Quantity,
                    BaseQuantity = baseQty,
                    ConversionFactor = productUnit.ConversionToBase,
                    UnitPrice = unitPrice,
                    Mrp = mrp,
                    DiscountAmount = lineDiscount,
                    TaxAmount = lineTax,
                    NetAmount = lineNet
                };

                foreach (var alloc in allocations)
                {
                    var saleQty = productUnit.ConversionToBase == 1m
                        ? alloc.BaseQuantity
                        : Math.Round(alloc.BaseQuantity / productUnit.ConversionToBase, 6);

                    saleLine.SaleLineBatches.Add(new SaleLineBatch
                    {
                        BatchId = alloc.Batch.Id,
                        Quantity = saleQty,
                        BaseQuantity = alloc.BaseQuantity,
                        UnitCost = alloc.Batch.PurchaseCost
                    });

                    var loc = alloc.Location;
                    var available = loc.QuantityOnHand - loc.ReservedQuantity;
                    if (available < alloc.BaseQuantity)
                        throw new ValidationAppException([
                            $"Line {lineIndex}: insufficient stock for batch {alloc.Batch.BatchNumber} (need {alloc.BaseQuantity}, available {available})."]);

                    loc.QuantityOnHand -= alloc.BaseQuantity;
                    loc.UpdatedAt = now;
                    pendingMovements.Add((loc, alloc.Batch, lineReq.ProductId, baseUnit.Id, alloc.BaseQuantity));
                }

                sale.SaleLines.Add(saleLine);
                subtotal += lineGross;
                discountTotal += lineDiscount;
                taxTotal += lineTax;
            }

            sale.Subtotal = Math.Round(subtotal, 4);
            sale.DiscountAmount = Math.Round(discountTotal, 4);
            sale.TaxAmount = Math.Round(taxTotal, 4);
            sale.NetAmount = Math.Round(sale.Subtotal - sale.DiscountAmount + sale.TaxAmount + sale.RoundOff, 4);

            ApplyPayments(sale, request.Payments, paymentMethods, now);

            if (sale.DueAmount > 0 && sale.CustomerId is long creditCustomerId)
                await EnsureCreditLimitAsync(creditCustomerId, sale.DueAmount, ct);

            db.Sales.Add(sale);

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("Stock location was modified concurrently. Reload and retry.");
            }

            saleId = sale.Id;
            var movIndex = 0;
            foreach (var (loc, batch, productId, baseUnitId, qty) in pendingMovements)
            {
                var balanceAfter = loc.QuantityOnHand; // already decremented
                var balanceBefore = balanceAfter + qty;
                db.InventoryMovements.Add(new InventoryMovement
                {
                    BranchId = request.BranchId,
                    WarehouseId = batch.WarehouseId,
                    WarehouseLocationId = loc.WarehouseLocationId,
                    ProductId = productId,
                    BatchId = batch.Id,
                    ProductUnitId = baseUnitId,
                    MovementType = MovementTypes.Sale,
                    ReferenceType = "Sale",
                    ReferenceId = saleId,
                    Quantity = -qty,
                    UnitCost = batch.PurchaseCost,
                    TotalCost = Math.Round(qty * batch.PurchaseCost, 4),
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    MovementDate = now,
                    PerformedBy = userId,
                    IdempotencyKey = $"SALE:{saleId}:MOV:{++movIndex}",
                    CreatedAt = now
                });
            }

            if (sale.DueAmount > 0 && sale.CustomerId is long customerId)
                await WriteCustomerLedgerDebitAsync(customerId, sale.BranchId, saleId, sale.DueAmount, now, ct);

            await TryPostCashSaleToOpenShiftAsync(sale, paymentMethods, userId, now, ct);

            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                db.IdempotencyKeys.Add(new IdempotencyKey
                {
                    TerminalId = request.TerminalId,
                    Key = request.IdempotencyKey.Trim(),
                    EntityType = "Sale",
                    EntityId = saleId,
                    CreatedAt = now
                });
            }

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
                {
                    var existing = await db.IdempotencyKeys.AsNoTracking()
                        .FirstOrDefaultAsync(k =>
                            k.TerminalId == request.TerminalId &&
                            k.Key == request.IdempotencyKey.Trim(), ct);
                    if (existing?.EntityId is long sid)
                    {
                        if (tx is not null) await tx.RollbackAsync(ct);
                        db.ChangeTracker.Clear();
                        return (await GetByIdAsync(sid, ct))!;
                    }
                }
                throw;
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

        return (await GetByIdAsync(saleId, ct))!;
    }

    public async Task<SaleDto> RecordPaymentAsync(long saleId, RecordSalePaymentRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var sale = await db.Sales
                .Include(s => s.Branch)
                .Include(s => s.SalePayments).ThenInclude(p => p.PaymentMethod)
                .FirstOrDefaultAsync(s => s.Id == saleId && s.Branch.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Sale {saleId} not found.");

            if (sale.Status is "Voided")
                throw new ValidationAppException(["Cannot record payment on a voided sale."]);

            var method = await db.PaymentMethods
                .FirstOrDefaultAsync(m => m.Id == request.PaymentMethodId && m.IsActive, ct)
                ?? throw new ValidationAppException(["Payment method not found."]);

            var now = DateTime.UtcNow;
            var isCredit = string.Equals(method.Type, "Credit", StringComparison.OrdinalIgnoreCase);
            var dueBefore = sale.DueAmount;

            sale.SalePayments.Add(new SalePayment
            {
                PaymentMethodId = method.Id,
                Amount = Math.Round(request.Amount, 4),
                ReferenceNumber = request.ReferenceNumber,
                PaymentDate = now,
                Status = "Completed",
                TransactionType = "Payment"
            });

            RecalculatePaymentStatus(sale, new Dictionary<long, PaymentMethod> { [method.Id] = method });

            if (!isCredit && sale.CustomerId is long customerId)
            {
                var applied = Math.Min(Math.Round(request.Amount, 4), dueBefore);
                if (applied > 0)
                    await WriteCustomerLedgerCreditAsync(customerId, sale.BranchId, sale.Id, applied, now, ct);
            }

            sale.UpdatedAt = now;
            await db.SaveChangesAsync(ct);
            if (tx is not null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            throw;
        }

        return (await GetByIdAsync(saleId, ct))!;
    }

    public async Task<SaleDto> VoidAsync(long saleId, VoidSaleRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var userId = RequireUserId();
        var now = DateTime.UtcNow;

        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var sale = await db.Sales
                .Include(s => s.Branch)
                .Include(s => s.SaleLines).ThenInclude(l => l.SaleLineBatches)
                .Include(s => s.SalePayments).ThenInclude(p => p.PaymentMethod)
                .FirstOrDefaultAsync(s => s.Id == saleId && s.Branch.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Sale {saleId} not found.");

            if (sale.Status != "Completed")
                throw new ValidationAppException([$"Only Completed sales can be voided (current: {sale.Status})."]);

            if (await db.SaleReturns.AnyAsync(r => r.SaleId == sale.Id && r.Status != "Cancelled", ct))
                throw new ValidationAppException(["Cannot void a sale that has returns. Use sale return instead."]);

            var saleMovements = await db.InventoryMovements
                .Where(m =>
                    m.ReferenceType == "Sale" &&
                    m.ReferenceId == sale.Id &&
                    m.MovementType == MovementTypes.Sale)
                .OrderBy(m => m.Id)
                .ToListAsync(ct);

            if (saleMovements.Count == 0)
                throw new ValidationAppException(["Sale has no inventory movements to reverse."]);

            var movIndex = 0;
            foreach (var mov in saleMovements)
            {
                var restoreQty = Math.Abs(mov.Quantity);
                var loc = await db.InventoryBatchLocations
                    .FirstOrDefaultAsync(l =>
                        l.BatchId == mov.BatchId &&
                        l.WarehouseLocationId == mov.WarehouseLocationId, ct);

                decimal balanceBefore;
                if (loc is null)
                {
                    balanceBefore = 0;
                    loc = new InventoryBatchLocation
                    {
                        BatchId = mov.BatchId,
                        WarehouseLocationId = mov.WarehouseLocationId,
                        QuantityOnHand = restoreQty,
                        ReservedQuantity = 0,
                        UpdatedAt = now,
                        RowVersion = new byte[8]
                    };
                    db.InventoryBatchLocations.Add(loc);
                }
                else
                {
                    balanceBefore = loc.QuantityOnHand;
                    loc.QuantityOnHand += restoreQty;
                    loc.UpdatedAt = now;
                }

                db.InventoryMovements.Add(new InventoryMovement
                {
                    BranchId = mov.BranchId,
                    WarehouseId = mov.WarehouseId,
                    WarehouseLocationId = mov.WarehouseLocationId,
                    ProductId = mov.ProductId,
                    BatchId = mov.BatchId,
                    ProductUnitId = mov.ProductUnitId,
                    MovementType = MovementTypes.Void,
                    ReferenceType = "Sale",
                    ReferenceId = sale.Id,
                    Quantity = restoreQty,
                    UnitCost = mov.UnitCost,
                    TotalCost = Math.Round(restoreQty * mov.UnitCost, 4),
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceBefore + restoreQty,
                    MovementDate = now,
                    PerformedBy = userId,
                    IdempotencyKey = $"VOID:{sale.Id}:MOV:{++movIndex}",
                    CreatedAt = now
                });
            }

            foreach (var pay in sale.SalePayments.Where(p => p.Status == "Completed"))
                pay.Status = "Voided";

            var netAr = await db.CustomerLedgers
                .Where(l => l.ReferenceType == "Sale" && l.ReferenceId == sale.Id)
                .SumAsync(l => (decimal?)(l.Debit - l.Credit), ct) ?? 0;
            if (netAr != 0 && sale.CustomerId is long customerId)
            {
                var seq = await NextLedgerSequenceAsync(customerId, ct);
                db.CustomerLedgers.Add(new CustomerLedger
                {
                    CustomerId = customerId,
                    BranchId = sale.BranchId,
                    TransactionDate = now,
                    TransactionType = "Void",
                    ReferenceType = "Sale",
                    ReferenceId = sale.Id,
                    Debit = netAr < 0 ? Math.Round(-netAr, 4) : 0,
                    Credit = netAr > 0 ? Math.Round(netAr, 4) : 0,
                    SequenceNo = seq,
                    Remarks = string.IsNullOrWhiteSpace(request.Reason)
                        ? $"Void sale {sale.InvoiceNumber}"
                        : $"Void sale {sale.InvoiceNumber}: {request.Reason}"
                });
            }

            var cashPaid = sale.SalePayments
                .Where(p =>
                    p.TransactionType == "Payment" &&
                    p.PaymentMethod is not null &&
                    string.Equals(p.PaymentMethod.Type, "Cash", StringComparison.OrdinalIgnoreCase))
                .Sum(p => p.Amount);
            if (cashPaid > 0)
            {
                var openShift = await db.CashShifts
                    .FirstOrDefaultAsync(s => s.TerminalId == sale.TerminalId && s.Status == "Open", ct);
                if (openShift is not null)
                {
                    db.CashTransactions.Add(new CashTransaction
                    {
                        CashShiftId = openShift.Id,
                        TransactionType = "Refund",
                        ReferenceType = "Sale",
                        ReferenceId = sale.Id,
                        Amount = Math.Round(cashPaid, 4),
                        Remarks = string.IsNullOrWhiteSpace(request.Reason)
                            ? $"Void {sale.InvoiceNumber}"
                            : request.Reason,
                        CreatedBy = userId,
                        CreatedAt = now
                    });
                }
            }

            sale.Status = "Voided";
            sale.PaidAmount = 0;
            sale.DueAmount = 0;
            sale.ChangeAmount = 0;
            sale.PaymentStatus = "Refunded";
            sale.UpdatedAt = now;

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

        return (await GetByIdAsync(saleId, ct))!;
    }

    private async Task ValidateHeaderAsync(long tenantId, CreateSaleRequest request, CancellationToken ct)
    {
        var branch = await db.Branches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == tenantId && b.IsActive, ct)
            ?? throw new ValidationAppException(["Branch not found."]);

        if (!await db.Counters.AnyAsync(c => c.Id == request.CounterId && c.BranchId == branch.Id && c.IsActive, ct))
            throw new ValidationAppException(["Counter not found for branch."]);

        if (!await db.Posterminals.AnyAsync(t => t.Id == request.TerminalId && t.BranchId == branch.Id && t.IsActive, ct))
            throw new ValidationAppException(["POS terminal not found for branch."]);

        if (!await db.Warehouses.AnyAsync(w => w.Id == request.WarehouseId && w.BranchId == branch.Id && w.IsActive, ct))
            throw new ValidationAppException(["Warehouse not found for branch."]);

        if (request.CustomerId is long customerId)
        {
            if (!await db.Customers.AnyAsync(c => c.Id == customerId && c.TenantId == tenantId && c.IsActive, ct))
                throw new ValidationAppException(["Customer not found."]);
        }

        if (request.SaleType == "Credit" && request.CustomerId is null)
            throw new ValidationAppException(["CustomerId is required for credit sales."]);
    }

    private sealed record BatchAllocation(InventoryBatch Batch, InventoryBatchLocation Location, decimal BaseQuantity);

    private async Task<List<BatchAllocation>> AllocateBatchesAsync(
        long tenantId,
        long warehouseId,
        CreateSaleLineRequest lineReq,
        ProductUnit productUnit,
        int lineIndex,
        CancellationToken ct)
    {
        var needBase = Math.Round(lineReq.Quantity * productUnit.ConversionToBase, 6);
        var result = new List<BatchAllocation>();

        if (lineReq.ManualBatches is { Count: > 0 })
        {
            var manualSum = Math.Round(lineReq.ManualBatches.Sum(b => b.Quantity), 6);
            if (manualSum != lineReq.Quantity)
                throw new ValidationAppException([
                    $"Line {lineIndex}: manual batch quantities ({manualSum}) must equal line quantity ({lineReq.Quantity})."]);

            foreach (var mb in lineReq.ManualBatches)
            {
                var baseQty = Math.Round(mb.Quantity * productUnit.ConversionToBase, 6);
                var loc = await db.InventoryBatchLocations
                    .Include(l => l.Batch).ThenInclude(b => b.Product)
                    .FirstOrDefaultAsync(l =>
                        l.BatchId == mb.BatchId &&
                        l.WarehouseLocationId == mb.WarehouseLocationId, ct)
                    ?? throw new ValidationAppException([
                        $"Line {lineIndex}: no stock for batch {mb.BatchId} at location {mb.WarehouseLocationId}."]);

                ValidateBatchEligible(loc.Batch, tenantId, warehouseId, lineReq.ProductId, lineIndex);
                result.Add(new BatchAllocation(loc.Batch, loc, baseQty));
            }

            return result;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var candidates = await db.InventoryBatchLocations
            .Include(l => l.Batch).ThenInclude(b => b.Product)
            .Where(l =>
                l.Batch.ProductId == lineReq.ProductId &&
                l.Batch.Product.TenantId == tenantId &&
                l.Batch.WarehouseId == warehouseId &&
                l.Batch.BatchStatus == BatchStatuses.Available &&
                !l.Batch.IsRecalled &&
                l.Batch.ExpiryDate >= today &&
                l.QuantityOnHand - l.ReservedQuantity > 0)
            .OrderBy(l => l.Batch.ExpiryDate)
            .ThenBy(l => l.BatchId)
            .ThenBy(l => l.Id)
            .ToListAsync(ct);

        decimal remaining = needBase;
        foreach (var loc in candidates)
        {
            if (remaining <= 0) break;
            var available = loc.QuantityOnHand - loc.ReservedQuantity;
            if (available <= 0) continue;
            var take = Math.Min(available, remaining);
            result.Add(new BatchAllocation(loc.Batch, loc, take));
            remaining -= take;
        }

        if (remaining > 0)
            throw new ValidationAppException([
                $"Line {lineIndex}: insufficient FEFO stock (need {needBase} base, short by {remaining})."]);

        return result;
    }

    private static void ValidateBatchEligible(InventoryBatch batch, long tenantId, long warehouseId, long productId, int lineIndex)
    {
        if (batch.ProductId != productId)
            throw new ValidationAppException([$"Line {lineIndex}: batch {batch.Id} does not match product."]);
        if (batch.WarehouseId != warehouseId)
            throw new ValidationAppException([$"Line {lineIndex}: batch {batch.Id} is not in sale warehouse."]);
        if (batch.BatchStatus != BatchStatuses.Available || batch.IsRecalled)
            throw new ValidationAppException([$"Line {lineIndex}: batch {batch.BatchNumber} is not available."]);
        if (batch.ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ValidationAppException([$"Line {lineIndex}: batch {batch.BatchNumber} is expired."]);
        if (batch.Product?.TenantId is long tid && tid != tenantId)
            throw new ValidationAppException([$"Line {lineIndex}: batch product tenant mismatch."]);
    }

    private async Task<decimal> ResolveUnitPriceAsync(
        long productId,
        long productUnitId,
        List<BatchAllocation> allocations,
        ProductUnit productUnit,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var price = await db.ProductPrices.AsNoTracking()
            .Where(p =>
                p.ProductId == productId &&
                p.ProductUnitId == productUnitId &&
                p.EffectiveFrom <= now &&
                (p.EffectiveTo == null || p.EffectiveTo >= now))
            .OrderByDescending(p => p.EffectiveFrom)
            .Select(p => (decimal?)p.SalePrice)
            .FirstOrDefaultAsync(ct);

        if (price is decimal p) return Math.Round(p, 4);

        if (allocations.Count > 0)
            return Math.Round(allocations[0].Batch.SalePrice * productUnit.ConversionToBase, 4);

        throw new ValidationAppException(["Unable to resolve unit price for product."]);
    }

    private async Task<Dictionary<long, PaymentMethod>> LoadPaymentMethodsAsync(
        IReadOnlyList<CreateSalePaymentRequest> payments,
        CancellationToken ct)
    {
        if (payments.Count == 0) return new Dictionary<long, PaymentMethod>();
        var ids = payments.Select(p => p.PaymentMethodId).Distinct().ToList();
        var methods = await db.PaymentMethods.AsNoTracking()
            .Where(m => ids.Contains(m.Id) && m.IsActive)
            .ToListAsync(ct);
        if (methods.Count != ids.Count)
            throw new ValidationAppException(["One or more payment methods are invalid."]);
        return methods.ToDictionary(m => m.Id);
    }

    private static void ApplyPayments(
        Sale sale,
        IReadOnlyList<CreateSalePaymentRequest> payments,
        Dictionary<long, PaymentMethod> methods,
        DateTime now)
    {
        foreach (var pay in payments)
        {
            var method = methods[pay.PaymentMethodId];
            sale.SalePayments.Add(new SalePayment
            {
                PaymentMethodId = method.Id,
                Amount = Math.Round(pay.Amount, 4),
                ReferenceNumber = pay.ReferenceNumber,
                PaymentDate = now,
                Status = "Completed",
                TransactionType = "Payment"
            });
        }

        RecalculatePaymentStatus(sale, methods);
    }

    private static void RecalculatePaymentStatus(Sale sale, IReadOnlyDictionary<long, PaymentMethod>? methods = null)
    {
        var completed = sale.SalePayments
            .Where(p => p.Status == "Completed" && p.TransactionType == "Payment")
            .ToList();

        decimal collected = 0;
        decimal creditCover = 0;
        foreach (var p in completed)
        {
            var type = p.PaymentMethod?.Type
                       ?? (methods is not null && methods.TryGetValue(p.PaymentMethodId, out var m) ? m.Type : null);

            if (string.Equals(type, "Credit", StringComparison.OrdinalIgnoreCase))
                creditCover += p.Amount;
            else
                collected += p.Amount;
        }

        sale.PaidAmount = Math.Round(collected, 4);

        // Credit sale fully covered by CREDIT method → Unpaid + full Due (schema / Phase 1 tests).
        if (sale.SaleType == "Credit" && sale.PaidAmount == 0 && creditCover >= sale.NetAmount)
        {
            sale.DueAmount = Math.Round(sale.NetAmount, 4);
            sale.ChangeAmount = 0;
            sale.PaymentStatus = "Unpaid";
            return;
        }

        var remaining = sale.NetAmount - sale.PaidAmount;
        if (remaining < 0)
        {
            sale.ChangeAmount = Math.Round(-remaining, 4);
            sale.DueAmount = 0;
        }
        else
        {
            sale.ChangeAmount = 0;
            sale.DueAmount = Math.Round(Math.Max(0, remaining - creditCover), 4);
        }

        sale.PaymentStatus = sale.DueAmount == 0
            ? "Paid"
            : sale.PaidAmount > 0 ? "Partial" : "Unpaid";
    }

    private async Task EnsureCreditLimitAsync(long customerId, decimal additionalDue, CancellationToken ct)
    {
        var customer = await db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId, ct)
            ?? throw new ValidationAppException(["Customer not found."]);

        var debit = await db.CustomerLedgers.Where(l => l.CustomerId == customerId).SumAsync(l => (decimal?)l.Debit, ct) ?? 0;
        var credit = await db.CustomerLedgers.Where(l => l.CustomerId == customerId).SumAsync(l => (decimal?)l.Credit, ct) ?? 0;
        var balance = debit - credit;
        var projected = Math.Round(balance + additionalDue, 4);
        if (projected > customer.CreditLimit)
            throw new ValidationAppException([
                $"Credit limit exceeded. Limit {customer.CreditLimit}, balance {Math.Round(balance, 4)}, new due {additionalDue}."]);
    }

    private async Task TryPostCashSaleToOpenShiftAsync(
        Sale sale,
        IReadOnlyDictionary<long, PaymentMethod> methods,
        long userId,
        DateTime now,
        CancellationToken ct)
    {
        decimal cashPaid = 0;
        foreach (var p in sale.SalePayments.Where(p => p.Status == "Completed" && p.TransactionType == "Payment"))
        {
            if (!methods.TryGetValue(p.PaymentMethodId, out var method)) continue;
            if (string.Equals(method.Type, "Cash", StringComparison.OrdinalIgnoreCase))
                cashPaid += p.Amount;
        }

        cashPaid = Math.Round(cashPaid, 4);
        if (cashPaid <= 0) return;

        var openShift = await db.CashShifts
            .FirstOrDefaultAsync(s => s.TerminalId == sale.TerminalId && s.Status == "Open", ct);
        if (openShift is null) return;

        db.CashTransactions.Add(new CashTransaction
        {
            CashShiftId = openShift.Id,
            TransactionType = "Sale",
            ReferenceType = "Sale",
            ReferenceId = sale.Id,
            Amount = cashPaid,
            Remarks = sale.InvoiceNumber,
            CreatedBy = userId,
            CreatedAt = now
        });
    }

    private async Task WriteCustomerLedgerDebitAsync(
        long customerId, long branchId, long saleId, decimal amount, DateTime now, CancellationToken ct)
    {
        if (amount <= 0) return;
        var seq = await NextLedgerSequenceAsync(customerId, ct);
        db.CustomerLedgers.Add(new CustomerLedger
        {
            CustomerId = customerId,
            BranchId = branchId,
            TransactionDate = now,
            TransactionType = "Sale",
            ReferenceType = "Sale",
            ReferenceId = saleId,
            Debit = Math.Round(amount, 4),
            Credit = 0,
            SequenceNo = seq,
            Remarks = $"Sale {saleId} credit"
        });
    }

    private async Task WriteCustomerLedgerCreditAsync(
        long customerId, long branchId, long saleId, decimal amount, DateTime now, CancellationToken ct)
    {
        if (amount <= 0) return;
        var seq = await NextLedgerSequenceAsync(customerId, ct);
        db.CustomerLedgers.Add(new CustomerLedger
        {
            CustomerId = customerId,
            BranchId = branchId,
            TransactionDate = now,
            TransactionType = "Payment",
            ReferenceType = "Sale",
            ReferenceId = saleId,
            Debit = 0,
            Credit = Math.Round(amount, 4),
            SequenceNo = seq,
            Remarks = $"Payment on sale {saleId}"
        });
    }

    private async Task<long> NextLedgerSequenceAsync(long customerId, CancellationToken ct)
    {
        var max = await db.CustomerLedgers
            .Where(l => l.CustomerId == customerId)
            .Select(l => (long?)l.SequenceNo)
            .MaxAsync(ct);
        return (max ?? 0) + 1;
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        return msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
               || msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
               || msg.Contains("2627")
               || msg.Contains("2601");
    }

    private IQueryable<Sale> Query() =>
        db.Sales.AsNoTracking()
            .Include(s => s.SaleLines).ThenInclude(l => l.SaleLineBatches).ThenInclude(b => b.Batch)
            .Include(s => s.SaleLines).ThenInclude(l => l.Product)
            .Include(s => s.SalePayments).ThenInclude(p => p.PaymentMethod)
            .Include(s => s.Branch);

    private static SaleDto Map(Sale s) => new()
    {
        Id = s.Id,
        BranchId = s.BranchId,
        CounterId = s.CounterId,
        TerminalId = s.TerminalId,
        UserId = s.UserId,
        CustomerId = s.CustomerId,
        InvoiceNumber = s.InvoiceNumber,
        SaleDate = s.SaleDate,
        SaleType = s.SaleType,
        Status = s.Status,
        CurrencyCode = s.CurrencyCode,
        Subtotal = s.Subtotal,
        DiscountAmount = s.DiscountAmount,
        TaxAmount = s.TaxAmount,
        RoundOff = s.RoundOff,
        NetAmount = s.NetAmount,
        PaidAmount = s.PaidAmount,
        DueAmount = s.DueAmount,
        ChangeAmount = s.ChangeAmount,
        PaymentStatus = s.PaymentStatus,
        FbrStatus = s.Fbrstatus,
        CreatedAt = s.CreatedAt,
        Lines = s.SaleLines.Select(l => new SaleLineDto
        {
            Id = l.Id,
            ProductId = l.ProductId,
            Sku = l.Product?.Sku,
            ProductName = l.Product?.Name,
            ProductUnitId = l.ProductUnitId,
            Quantity = l.Quantity,
            BaseQuantity = l.BaseQuantity,
            ConversionFactor = l.ConversionFactor,
            UnitPrice = l.UnitPrice,
            Mrp = l.Mrp,
            DiscountAmount = l.DiscountAmount,
            TaxAmount = l.TaxAmount,
            NetAmount = l.NetAmount,
            Batches = l.SaleLineBatches.Select(b => new SaleLineBatchDto
            {
                Id = b.Id,
                BatchId = b.BatchId,
                BatchNumber = b.Batch?.BatchNumber,
                ExpiryDate = b.Batch?.ExpiryDate,
                Quantity = b.Quantity,
                BaseQuantity = b.BaseQuantity,
                UnitCost = b.UnitCost
            }).ToList()
        }).ToList(),
        Payments = s.SalePayments.Select(p => new SalePaymentDto
        {
            Id = p.Id,
            PaymentMethodId = p.PaymentMethodId,
            PaymentMethodCode = p.PaymentMethod?.Code,
            PaymentMethodName = p.PaymentMethod?.Name,
            Amount = p.Amount,
            ReferenceNumber = p.ReferenceNumber,
            PaymentDate = p.PaymentDate,
            Status = p.Status,
            TransactionType = p.TransactionType,
            ParentPaymentId = p.ParentPaymentId
        }).ToList()
    };
}
