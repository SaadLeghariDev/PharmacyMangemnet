using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Procurement;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class SupplierReturnService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    INumberSequenceService sequences) : ISupplierReturnService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    private long? CurrentUserId() => currentUser.UserId;

    public async Task<PagedResult<SupplierReturnDto>> SearchAsync(SupplierReturnQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = Query().Where(r => r.Branch.TenantId == tenantId);

        if (query.SupplierId is long supplierId) q = q.Where(r => r.SupplierId == supplierId);
        if (query.BranchId is long branchId) q = q.Where(r => r.BranchId == branchId);
        if (query.WarehouseId is long warehouseId) q = q.Where(r => r.WarehouseId == warehouseId);
        if (!string.IsNullOrWhiteSpace(query.Status)) q = q.Where(r => r.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(r =>
                r.ReturnNumber.Contains(s) ||
                (r.Reason != null && r.Reason.Contains(s)) ||
                r.Supplier.Code.Contains(s) ||
                r.Supplier.Name.Contains(s));
        }

        q = q.OrderByDescending(r => r.Id);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<SupplierReturnDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<SupplierReturnDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await Query().FirstOrDefaultAsync(r => r.Id == id && r.Branch.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<SupplierReturnDto> CreateDraftAsync(CreateSupplierReturnRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await ValidateHeaderAsync(tenantId, request, ct);

        var returnNumber = await sequences.AllocateNextAsync(
            tenantId, DocumentTypes.SupplierReturn, request.BranchId, null, "SR-", ct);

        var entity = new SupplierReturn
        {
            SupplierId = request.SupplierId,
            BranchId = request.BranchId,
            WarehouseId = request.WarehouseId,
            ReturnNumber = returnNumber,
            ReturnDate = request.ReturnDate ?? DateTime.UtcNow,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            Status = "Draft",
            TotalAmount = 0
        };

        decimal total = 0;
        foreach (var lineReq in request.Lines)
        {
            await ValidateLineAsync(tenantId, request.WarehouseId, lineReq, ct);
            var qty = Math.Round(lineReq.Quantity, 6);
            var unitCost = Math.Round(lineReq.UnitCost, 4);
            total += Math.Round(qty * unitCost, 4);

            entity.SupplierReturnLines.Add(new SupplierReturnLine
            {
                ProductId = lineReq.ProductId,
                BatchId = lineReq.BatchId,
                ProductUnitId = lineReq.ProductUnitId,
                Quantity = qty,
                UnitCost = unitCost,
                GoodsReceiptLineId = lineReq.GoodsReceiptLineId
            });
        }

        entity.TotalAmount = Math.Round(total, 4);
        db.SupplierReturns.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<SupplierReturnDto> PostAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var ret = await db.SupplierReturns
                .Include(r => r.Branch)
                .Include(r => r.Supplier)
                .Include(r => r.SupplierReturnLines)
                .FirstOrDefaultAsync(r => r.Id == id && r.Branch.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Supplier return {id} not found.");

            if (ret.Status != "Draft")
                throw new ValidationAppException([$"Only Draft supplier returns can be posted (current: {ret.Status})."]);

            if (ret.SupplierReturnLines.Count == 0)
                throw new ValidationAppException(["Supplier return has no lines."]);

            if (string.IsNullOrWhiteSpace(ret.ReturnNumber))
            {
                ret.ReturnNumber = await sequences.AllocateNextAsync(
                    tenantId, DocumentTypes.SupplierReturn, ret.BranchId, null, "SR-", ct);
            }

            var now = DateTime.UtcNow;
            var userId = CurrentUserId();
            decimal total = 0;
            var movIndex = 0;

            foreach (var line in ret.SupplierReturnLines)
            {
                var productUnit = await db.ProductUnits.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == line.ProductUnitId && u.ProductId == line.ProductId && u.IsActive, ct)
                    ?? throw new ValidationAppException([$"Product unit {line.ProductUnitId} is invalid."]);

                if (productUnit.ConversionToBase <= 0)
                    throw new ValidationAppException([$"Product unit {line.ProductUnitId} has invalid ConversionToBase."]);

                var baseUnit = await db.ProductUnits.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.ProductId == line.ProductId && u.IsBaseUnit && u.IsActive, ct)
                    ?? throw new ValidationAppException([$"Base unit missing for product {line.ProductId}."]);

                var batch = await db.InventoryBatches
                    .FirstOrDefaultAsync(b => b.Id == line.BatchId && b.ProductId == line.ProductId, ct)
                    ?? throw new ValidationAppException([$"Batch {line.BatchId} not found for product {line.ProductId}."]);

                if (batch.WarehouseId != ret.WarehouseId)
                    throw new ValidationAppException([$"Batch {line.BatchId} is not in return warehouse {ret.WarehouseId}."]);

                var baseQty = Math.Round(line.Quantity * productUnit.ConversionToBase, 6);
                if (baseQty <= 0)
                    throw new ValidationAppException(["Return quantity must convert to a positive base quantity."]);

                var loc = await ResolveStockLocationAsync(line.BatchId, ret.WarehouseId, baseQty, ct);
                var available = loc.QuantityOnHand - loc.ReservedQuantity;
                if (available < baseQty)
                    throw new ValidationAppException([
                        $"Insufficient stock for batch {line.BatchId} (available {available}, required {baseQty})."]);

                var balanceBefore = loc.QuantityOnHand;
                loc.QuantityOnHand -= baseQty;
                loc.UpdatedAt = now;

                var unitCost = line.UnitCost > 0
                    ? Math.Round(line.UnitCost / productUnit.ConversionToBase, 4)
                    : batch.PurchaseCost;

                db.InventoryMovements.Add(new InventoryMovement
                {
                    BranchId = ret.BranchId,
                    WarehouseId = ret.WarehouseId,
                    WarehouseLocationId = loc.WarehouseLocationId,
                    ProductId = line.ProductId,
                    BatchId = line.BatchId,
                    ProductUnitId = baseUnit.Id,
                    MovementType = MovementTypes.SupplierReturn,
                    ReferenceType = "SupplierReturn",
                    ReferenceId = ret.Id,
                    Quantity = -baseQty,
                    UnitCost = unitCost,
                    TotalCost = Math.Round(baseQty * unitCost, 4),
                    BalanceBefore = balanceBefore,
                    BalanceAfter = loc.QuantityOnHand,
                    MovementDate = now,
                    PerformedBy = userId,
                    IdempotencyKey = $"SR:{ret.Id}:LINE:{line.Id}:{movIndex++}",
                    CreatedAt = now
                });

                total += Math.Round(line.Quantity * line.UnitCost, 4);
            }

            ret.TotalAmount = Math.Round(total, 4);
            ret.Status = "Posted";

            var seq = await NextLedgerSequenceAsync(ret.SupplierId, ret.BranchId, ct);
            db.SupplierLedgers.Add(new SupplierLedger
            {
                SupplierId = ret.SupplierId,
                BranchId = ret.BranchId,
                TransactionDate = ret.ReturnDate,
                TransactionType = "Return",
                ReferenceType = "SupplierReturn",
                ReferenceId = ret.Id,
                Debit = ret.TotalAmount,
                Credit = 0,
                SequenceNo = seq,
                Remarks = ret.Reason ?? ret.ReturnNumber
            });

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

    public async Task<SupplierReturnDto> CancelAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var ret = await db.SupplierReturns
            .Include(r => r.Branch)
            .FirstOrDefaultAsync(r => r.Id == id && r.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Supplier return {id} not found.");

        if (ret.Status != "Draft")
            throw new ValidationAppException([$"Only Draft supplier returns can be cancelled (current: {ret.Status})."]);

        ret.Status = "Cancelled";
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(id, ct))!;
    }

    private IQueryable<SupplierReturn> Query() =>
        db.SupplierReturns.AsNoTracking()
            .Include(r => r.Branch)
            .Include(r => r.Supplier)
            .Include(r => r.Warehouse)
            .Include(r => r.SupplierReturnLines);

    private async Task ValidateHeaderAsync(long tenantId, CreateSupplierReturnRequest request, CancellationToken ct)
    {
        if (!await db.Branches.AnyAsync(b => b.Id == request.BranchId && b.TenantId == tenantId && b.IsActive, ct))
            throw new ValidationAppException(["Branch not found for tenant."]);
        if (!await db.Warehouses.AnyAsync(w =>
                w.Id == request.WarehouseId && w.BranchId == request.BranchId && w.IsActive, ct))
            throw new ValidationAppException(["Warehouse not found for branch."]);
        if (!await db.Suppliers.AnyAsync(s => s.Id == request.SupplierId && s.TenantId == tenantId && s.IsActive, ct))
            throw new ValidationAppException(["Supplier not found for tenant."]);
    }

    private async Task ValidateLineAsync(
        long tenantId, long warehouseId, CreateSupplierReturnLineRequest line, CancellationToken ct)
    {
        if (!await db.Products.AnyAsync(p => p.Id == line.ProductId && p.TenantId == tenantId && p.IsActive, ct))
            throw new ValidationAppException([$"Product {line.ProductId} not found."]);
        if (!await db.ProductUnits.AnyAsync(u =>
                u.Id == line.ProductUnitId && u.ProductId == line.ProductId && u.IsActive, ct))
            throw new ValidationAppException([$"Product unit {line.ProductUnitId} invalid for product {line.ProductId}."]);

        var batch = await db.InventoryBatches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == line.BatchId && b.ProductId == line.ProductId, ct)
            ?? throw new ValidationAppException([$"Batch {line.BatchId} not found for product {line.ProductId}."]);

        if (batch.WarehouseId != warehouseId)
            throw new ValidationAppException([$"Batch {line.BatchId} is not in warehouse {warehouseId}."]);

        if (line.GoodsReceiptLineId is long grlId &&
            !await db.GoodsReceiptLines.AnyAsync(l => l.Id == grlId && l.ProductId == line.ProductId, ct))
            throw new ValidationAppException([$"Goods receipt line {grlId} not found for product."]);

        if (line.WarehouseLocationId is long locId &&
            !await db.WarehouseLocations.AnyAsync(l =>
                l.Id == locId && l.WarehouseId == warehouseId && l.IsActive, ct))
            throw new ValidationAppException([$"Warehouse location {locId} not in warehouse {warehouseId}."]);
    }

    private async Task<InventoryBatchLocation> ResolveStockLocationAsync(
        long batchId, long warehouseId, decimal requiredBaseQty, CancellationToken ct)
    {
        var locs = await db.InventoryBatchLocations
            .Include(l => l.WarehouseLocation)
            .Where(l =>
                l.BatchId == batchId &&
                l.WarehouseLocation.WarehouseId == warehouseId &&
                l.QuantityOnHand - l.ReservedQuantity > 0)
            .OrderByDescending(l => l.QuantityOnHand - l.ReservedQuantity)
            .ToListAsync(ct);

        var loc = locs.FirstOrDefault(l => l.QuantityOnHand - l.ReservedQuantity >= requiredBaseQty)
            ?? locs.FirstOrDefault()
            ?? throw new ValidationAppException([$"No stock location for batch {batchId} in warehouse {warehouseId}."]);

        return loc;
    }

    private async Task<long> NextLedgerSequenceAsync(long supplierId, long branchId, CancellationToken ct)
    {
        var max = await db.SupplierLedgers
            .Where(l => l.SupplierId == supplierId && l.BranchId == branchId)
            .Select(l => (long?)l.SequenceNo)
            .MaxAsync(ct);
        return (max ?? 0) + 1;
    }

    private static SupplierReturnDto Map(SupplierReturn r) => new()
    {
        Id = r.Id,
        SupplierId = r.SupplierId,
        SupplierCode = r.Supplier?.Code,
        SupplierName = r.Supplier?.Name,
        BranchId = r.BranchId,
        BranchName = r.Branch?.Name,
        WarehouseId = r.WarehouseId,
        WarehouseName = r.Warehouse?.Name,
        ReturnNumber = r.ReturnNumber,
        ReturnDate = r.ReturnDate,
        Reason = r.Reason,
        Status = r.Status,
        TotalAmount = r.TotalAmount,
        Lines = r.SupplierReturnLines.Select(l => new SupplierReturnLineDto
        {
            Id = l.Id,
            ProductId = l.ProductId,
            BatchId = l.BatchId,
            GoodsReceiptLineId = l.GoodsReceiptLineId,
            ProductUnitId = l.ProductUnitId,
            Quantity = l.Quantity,
            UnitCost = l.UnitCost,
            LineAmount = Math.Round(l.Quantity * l.UnitCost, 4)
        }).ToList()
    };
}
