using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class GoodsReceiptService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    INumberSequenceService sequences) : IGoodsReceiptService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<GoodsReceiptDto>> SearchAsync(GoodsReceiptQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.GoodsReceipts.AsNoTracking()
            .Include(g => g.GoodsReceiptLines).ThenInclude(l => l.GoodsReceiptLineBatches)
            .Include(g => g.Branch)
            .Where(g => g.Branch.TenantId == tenantId);

        if (query.BranchId is long branchId) q = q.Where(g => g.BranchId == branchId);
        if (query.SupplierId is long supplierId) q = q.Where(g => g.SupplierId == supplierId);
        if (!string.IsNullOrWhiteSpace(query.Status)) q = q.Where(g => g.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(g => g.Grnnumber.Contains(s) || (g.InvoiceNumber != null && g.InvoiceNumber.Contains(s)));
        }

        q = q.OrderByDescending(g => g.Id);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<GoodsReceiptDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<GoodsReceiptDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await Query().FirstOrDefaultAsync(g => g.Id == id && g.Branch.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<GoodsReceiptDto> CreateDraftAsync(CreateGoodsReceiptRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await ValidateHeaderAsync(tenantId, request, ct);

        var grnNumber = await sequences.AllocateNextAsync(
            tenantId, DocumentTypes.GoodsReceipt, request.BranchId, null, "GRN-", ct);

        decimal subtotal = 0, discount = 0, tax = 0;
        var entity = new GoodsReceipt
        {
            BranchId = request.BranchId,
            WarehouseId = request.WarehouseId,
            SupplierId = request.SupplierId,
            PurchaseOrderId = request.PurchaseOrderId,
            Grnnumber = grnNumber,
            ReceiptDate = DateTime.UtcNow,
            Status = "Draft",
            InvoiceNumber = request.InvoiceNumber,
            InvoiceDate = request.InvoiceDate,
            ReceivedBy = currentUser.UserId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var lineReq in request.Lines)
        {
            await ValidateLineAsync(tenantId, request.WarehouseId, lineReq, ct);
            var net = Math.Round(lineReq.ReceivedQuantity * lineReq.UnitCost - lineReq.DiscountAmount + lineReq.TaxAmount, 4);
            subtotal += lineReq.ReceivedQuantity * lineReq.UnitCost;
            discount += lineReq.DiscountAmount;
            tax += lineReq.TaxAmount;

            var line = new GoodsReceiptLine
            {
                ProductId = lineReq.ProductId,
                ProductUnitId = lineReq.ProductUnitId,
                OrderedQuantity = lineReq.OrderedQuantity,
                ReceivedQuantity = lineReq.ReceivedQuantity,
                FreeQuantity = lineReq.FreeQuantity,
                UnitCost = lineReq.UnitCost,
                DiscountAmount = lineReq.DiscountAmount,
                TaxAmount = lineReq.TaxAmount,
                NetCost = net
            };

            foreach (var batch in lineReq.Batches)
            {
                line.GoodsReceiptLineBatches.Add(new GoodsReceiptLineBatch
                {
                    BatchNumber = batch.BatchNumber.Trim(),
                    ManufacturingDate = batch.ManufacturingDate,
                    ExpiryDate = batch.ExpiryDate,
                    Mrp = batch.Mrp,
                    SalePrice = batch.SalePrice,
                    Quantity = batch.Quantity,
                    FreeQuantity = batch.FreeQuantity,
                    WarehouseLocationId = batch.WarehouseLocationId
                });
            }

            entity.GoodsReceiptLines.Add(line);
        }

        entity.Subtotal = Math.Round(subtotal, 4);
        entity.DiscountAmount = Math.Round(discount, 4);
        entity.TaxAmount = Math.Round(tax, 4);
        entity.NetAmount = Math.Round(subtotal - discount + tax, 4);

        db.GoodsReceipts.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<GoodsReceiptDto> PostAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var grn = await db.GoodsReceipts
                .Include(g => g.Branch)
                .Include(g => g.GoodsReceiptLines)
                    .ThenInclude(l => l.GoodsReceiptLineBatches)
                .FirstOrDefaultAsync(g => g.Id == id && g.Branch.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Goods receipt {id} not found.");

            if (grn.Status != "Draft")
                throw new ValidationAppException([$"Only Draft goods receipts can be posted (current: {grn.Status})."]);

            if (grn.GoodsReceiptLines.Count == 0)
                throw new ValidationAppException(["Goods receipt has no lines."]);

            var now = DateTime.UtcNow;
            var userId = currentUser.UserId;

            foreach (var line in grn.GoodsReceiptLines)
            {
                if (line.GoodsReceiptLineBatches.Count == 0)
                    throw new ValidationAppException([$"Line {line.Id} has no batches."]);

                var productUnit = await db.ProductUnits.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == line.ProductUnitId && u.ProductId == line.ProductId && u.IsActive, ct)
                    ?? throw new ValidationAppException([$"Product unit {line.ProductUnitId} is invalid."]);

                if (productUnit.ConversionToBase <= 0)
                    throw new ValidationAppException([$"Product unit {line.ProductUnitId} has invalid ConversionToBase."]);

                var baseUnit = await db.ProductUnits.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.ProductId == line.ProductId && u.IsBaseUnit && u.IsActive, ct)
                    ?? throw new ValidationAppException([$"Base unit missing for product {line.ProductId}."]);

                var batchQtySum = line.GoodsReceiptLineBatches.Sum(b => b.Quantity);
                if (batchQtySum <= 0)
                    throw new ValidationAppException([$"Line {line.Id} batch quantities must be > 0."]);

                // Purchase cost per base unit = unit cost / conversion.
                var purchaseCostPerBase = Math.Round(line.UnitCost / productUnit.ConversionToBase, 4);

                foreach (var gBatch in line.GoodsReceiptLineBatches)
                {
                    if (gBatch.Quantity <= 0)
                        throw new ValidationAppException(["Batch quantity must be greater than zero."]);

                    var location = await db.WarehouseLocations.AsNoTracking()
                        .FirstOrDefaultAsync(l => l.Id == gBatch.WarehouseLocationId && l.IsActive, ct)
                        ?? throw new ValidationAppException([$"Warehouse location {gBatch.WarehouseLocationId} not found."]);

                    if (location.WarehouseId != grn.WarehouseId)
                        throw new ValidationAppException([$"Location {gBatch.WarehouseLocationId} is not in GRN warehouse {grn.WarehouseId}."]);

                    // Entered qty stored on GRLB; location/movement use base units.
                    var baseQty = Math.Round(gBatch.Quantity * productUnit.ConversionToBase, 6);
                    var baseFree = Math.Round(gBatch.FreeQuantity * productUnit.ConversionToBase, 6);

                    var inventoryBatch = new InventoryBatch
                    {
                        ProductId = line.ProductId,
                        GoodsReceiptLineId = line.Id,
                        SupplierId = grn.SupplierId,
                        WarehouseId = grn.WarehouseId,
                        BatchNumber = gBatch.BatchNumber,
                        ManufacturingDate = gBatch.ManufacturingDate,
                        ExpiryDate = gBatch.ExpiryDate,
                        QuantityReceived = baseQty,
                        FreeQuantity = baseFree,
                        PurchaseCost = purchaseCostPerBase,
                        Mrp = gBatch.Mrp,
                        SalePrice = gBatch.SalePrice,
                        BatchStatus = BatchStatuses.Available,
                        IsRecalled = false,
                        CreatedAt = now,
                        UpdatedAt = now,
                        RowVersion = new byte[8]
                    };
                    db.InventoryBatches.Add(inventoryBatch);
                    await db.SaveChangesAsync(ct);

                    db.InventoryBatchLocations.Add(new InventoryBatchLocation
                    {
                        BatchId = inventoryBatch.Id,
                        WarehouseLocationId = gBatch.WarehouseLocationId,
                        QuantityOnHand = baseQty,
                        ReservedQuantity = 0,
                        UpdatedAt = now,
                        RowVersion = new byte[8]
                    });

                    db.InventoryMovements.Add(new InventoryMovement
                    {
                        BranchId = grn.BranchId,
                        WarehouseId = grn.WarehouseId,
                        WarehouseLocationId = gBatch.WarehouseLocationId,
                        ProductId = line.ProductId,
                        BatchId = inventoryBatch.Id,
                        ProductUnitId = baseUnit.Id,
                        MovementType = MovementTypes.GoodsReceipt,
                        ReferenceType = "GoodsReceipt",
                        ReferenceId = grn.Id,
                        Quantity = baseQty,
                        UnitCost = purchaseCostPerBase,
                        TotalCost = Math.Round(baseQty * purchaseCostPerBase, 4),
                        BalanceBefore = 0,
                        BalanceAfter = baseQty,
                        MovementDate = now,
                        PerformedBy = userId,
                        IdempotencyKey = $"GRN:{grn.Id}:LINE:{line.Id}:BATCH:{gBatch.Id}",
                        CreatedAt = now
                    });
                }
            }

            grn.Status = "Posted";
            grn.ReceivedBy ??= userId;
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

    private IQueryable<GoodsReceipt> Query() =>
        db.GoodsReceipts.AsNoTracking()
            .Include(g => g.GoodsReceiptLines).ThenInclude(l => l.GoodsReceiptLineBatches)
            .Include(g => g.Branch);

    private async Task ValidateHeaderAsync(long tenantId, CreateGoodsReceiptRequest request, CancellationToken ct)
    {
        if (!await db.Branches.AnyAsync(b => b.Id == request.BranchId && b.TenantId == tenantId && b.IsActive, ct))
            throw new ValidationAppException(["Branch not found for tenant."]);
        if (!await db.Warehouses.AnyAsync(w => w.Id == request.WarehouseId && w.BranchId == request.BranchId && w.IsActive, ct))
            throw new ValidationAppException(["Warehouse not found for branch."]);
        if (!await db.Suppliers.AnyAsync(s => s.Id == request.SupplierId && s.TenantId == tenantId && s.IsActive, ct))
            throw new ValidationAppException(["Supplier not found for tenant."]);

        if (request.PurchaseOrderId is long poId)
        {
            var po = await db.PurchaseOrders.AsNoTracking()
                .Include(p => p.Branch)
                .FirstOrDefaultAsync(p => p.Id == poId && p.Branch.TenantId == tenantId, ct)
                ?? throw new ValidationAppException(["Purchase order not found."]);
            if (po.Status is not ("Approved" or "Partial"))
                throw new ValidationAppException([$"Purchase order must be Approved or Partial (current: {po.Status})."]);
            if (po.SupplierId != request.SupplierId || po.WarehouseId != request.WarehouseId || po.BranchId != request.BranchId)
                throw new ValidationAppException(["Goods receipt header must match the purchase order branch/warehouse/supplier."]);
        }
    }

    private async Task ValidateLineAsync(long tenantId, long warehouseId, GoodsReceiptLineRequest line, CancellationToken ct)
    {
        if (!await db.Products.AnyAsync(p => p.Id == line.ProductId && p.TenantId == tenantId && p.IsActive, ct))
            throw new ValidationAppException([$"Product {line.ProductId} not found."]);
        if (!await db.ProductUnits.AnyAsync(u => u.Id == line.ProductUnitId && u.ProductId == line.ProductId && u.IsActive, ct))
            throw new ValidationAppException([$"Product unit {line.ProductUnitId} invalid for product {line.ProductId}."]);

        foreach (var batch in line.Batches)
        {
            if (!await db.WarehouseLocations.AnyAsync(l =>
                    l.Id == batch.WarehouseLocationId && l.WarehouseId == warehouseId && l.IsActive, ct))
                throw new ValidationAppException([$"Location {batch.WarehouseLocationId} not in warehouse {warehouseId}."]);
        }
    }

    private static GoodsReceiptDto Map(GoodsReceipt g) => new()
    {
        Id = g.Id,
        BranchId = g.BranchId,
        WarehouseId = g.WarehouseId,
        SupplierId = g.SupplierId,
        PurchaseOrderId = g.PurchaseOrderId,
        GrnNumber = g.Grnnumber,
        ReceiptDate = g.ReceiptDate,
        Status = g.Status,
        InvoiceNumber = g.InvoiceNumber,
        InvoiceDate = g.InvoiceDate,
        Subtotal = g.Subtotal,
        DiscountAmount = g.DiscountAmount,
        TaxAmount = g.TaxAmount,
        NetAmount = g.NetAmount,
        ReceivedBy = g.ReceivedBy,
        CreatedAt = g.CreatedAt,
        Lines = g.GoodsReceiptLines.Select(l => new GoodsReceiptLineDto
        {
            Id = l.Id,
            ProductId = l.ProductId,
            ProductUnitId = l.ProductUnitId,
            OrderedQuantity = l.OrderedQuantity,
            ReceivedQuantity = l.ReceivedQuantity,
            FreeQuantity = l.FreeQuantity,
            UnitCost = l.UnitCost,
            DiscountAmount = l.DiscountAmount,
            TaxAmount = l.TaxAmount,
            NetCost = l.NetCost,
            Batches = l.GoodsReceiptLineBatches.Select(b => new GoodsReceiptLineBatchDto
            {
                Id = b.Id,
                BatchNumber = b.BatchNumber,
                ManufacturingDate = b.ManufacturingDate,
                ExpiryDate = b.ExpiryDate,
                Mrp = b.Mrp,
                SalePrice = b.SalePrice,
                Quantity = b.Quantity,
                FreeQuantity = b.FreeQuantity,
                WarehouseLocationId = b.WarehouseLocationId
            }).ToList()
        }).ToList()
    };
}
