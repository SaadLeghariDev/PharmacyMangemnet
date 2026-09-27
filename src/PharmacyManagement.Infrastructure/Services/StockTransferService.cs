using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class StockTransferService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    INumberSequenceService sequences) : IStockTransferService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<StockTransferDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await Query()
            .FirstOrDefaultAsync(t => t.Id == id && t.FromWarehouse.Branch.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<StockTransferDto> CreateAsync(CreateStockTransferRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var fromWh = await db.Warehouses.AsNoTracking()
            .Include(w => w.Branch)
            .FirstOrDefaultAsync(w => w.Id == request.FromWarehouseId && w.Branch.TenantId == tenantId && w.IsActive, ct)
            ?? throw new ValidationAppException(["From warehouse not found."]);
        var toWh = await db.Warehouses.AsNoTracking()
            .Include(w => w.Branch)
            .FirstOrDefaultAsync(w => w.Id == request.ToWarehouseId && w.Branch.TenantId == tenantId && w.IsActive, ct)
            ?? throw new ValidationAppException(["To warehouse not found."]);

        if (fromWh.Id == toWh.Id)
            throw new ValidationAppException(["From and to warehouses must differ."]);

        foreach (var line in request.Lines)
            await ValidateLineAsync(request.FromWarehouseId, request.ToWarehouseId, line, ct);

        var number = await sequences.AllocateNextAsync(
            tenantId, DocumentTypes.StockTransfer, fromWh.BranchId, null, "TR-", ct);

        var entity = new StockTransfer
        {
            TransferNumber = number,
            FromWarehouseId = request.FromWarehouseId,
            ToWarehouseId = request.ToWarehouseId,
            TransferDate = DateTime.UtcNow,
            Status = "Draft",
            RequestedBy = currentUser.UserId
        };

        foreach (var line in request.Lines)
        {
            entity.StockTransferLines.Add(new StockTransferLine
            {
                ProductId = line.ProductId,
                BatchId = line.BatchId,
                ProductUnitId = line.ProductUnitId,
                FromLocationId = line.FromLocationId,
                ToLocationId = line.ToLocationId,
                Quantity = line.Quantity,
                ReceivedQuantity = 0
            });
        }

        db.StockTransfers.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<StockTransferDto> CompleteAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var transfer = await db.StockTransfers
                .Include(t => t.StockTransferLines)
                .Include(t => t.FromWarehouse).ThenInclude(w => w.Branch)
                .FirstOrDefaultAsync(t => t.Id == id && t.FromWarehouse.Branch.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Stock transfer {id} not found.");

            if (transfer.Status is not ("Draft" or "InTransit"))
                throw new ValidationAppException([$"Transfer cannot be completed from status '{transfer.Status}'."]);

            if (transfer.StockTransferLines.Count == 0)
                throw new ValidationAppException(["Transfer has no lines."]);

            var now = DateTime.UtcNow;
            var userId = currentUser.UserId;
            var fromBranchId = transfer.FromWarehouse.BranchId;
            var toBranchId = await db.Warehouses.AsNoTracking()
                .Where(w => w.Id == transfer.ToWarehouseId)
                .Select(w => w.BranchId)
                .FirstAsync(ct);

            foreach (var line in transfer.StockTransferLines)
            {
                var unit = await db.ProductUnits.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == line.ProductUnitId && u.ProductId == line.ProductId, ct)
                    ?? throw new ValidationAppException([$"Product unit {line.ProductUnitId} not found."]);
                var baseQty = Math.Round(line.Quantity * unit.ConversionToBase, 6);

                var fromLoc = await db.InventoryBatchLocations
                    .FirstOrDefaultAsync(l => l.BatchId == line.BatchId && l.WarehouseLocationId == line.FromLocationId, ct)
                    ?? throw new ValidationAppException([$"No stock at from-location for batch {line.BatchId}."]);

                var available = fromLoc.QuantityOnHand - fromLoc.ReservedQuantity;
                if (available < baseQty)
                    throw new ValidationAppException([$"Insufficient available stock for batch {line.BatchId} (need {baseQty}, available {available})."]);

                var balanceBeforeOut = fromLoc.QuantityOnHand;
                fromLoc.QuantityOnHand -= baseQty;
                fromLoc.UpdatedAt = now;

                var toLoc = await db.InventoryBatchLocations
                    .FirstOrDefaultAsync(l => l.BatchId == line.BatchId && l.WarehouseLocationId == line.ToLocationId, ct);
                decimal balanceBeforeIn;
                if (toLoc is null)
                {
                    balanceBeforeIn = 0;
                    toLoc = new InventoryBatchLocation
                    {
                        BatchId = line.BatchId,
                        WarehouseLocationId = line.ToLocationId,
                        QuantityOnHand = baseQty,
                        ReservedQuantity = 0,
                        UpdatedAt = now,
                        RowVersion = new byte[8]
                    };
                    db.InventoryBatchLocations.Add(toLoc);
                }
                else
                {
                    balanceBeforeIn = toLoc.QuantityOnHand;
                    toLoc.QuantityOnHand += baseQty;
                    toLoc.UpdatedAt = now;
                }

                var batch = await db.InventoryBatches.AsNoTracking().FirstAsync(b => b.Id == line.BatchId, ct);

                db.InventoryMovements.Add(new InventoryMovement
                {
                    BranchId = fromBranchId,
                    WarehouseId = transfer.FromWarehouseId,
                    WarehouseLocationId = line.FromLocationId,
                    ProductId = line.ProductId,
                    BatchId = line.BatchId,
                    ProductUnitId = line.ProductUnitId,
                    MovementType = MovementTypes.TransferOut,
                    ReferenceType = "StockTransfer",
                    ReferenceId = transfer.Id,
                    Quantity = -baseQty,
                    UnitCost = batch.PurchaseCost,
                    TotalCost = Math.Round(baseQty * batch.PurchaseCost, 4),
                    BalanceBefore = balanceBeforeOut,
                    BalanceAfter = balanceBeforeOut - baseQty,
                    MovementDate = now,
                    PerformedBy = userId,
                    IdempotencyKey = $"TR:{transfer.Id}:OUT:{line.Id}",
                    CreatedAt = now
                });

                db.InventoryMovements.Add(new InventoryMovement
                {
                    BranchId = toBranchId,
                    WarehouseId = transfer.ToWarehouseId,
                    WarehouseLocationId = line.ToLocationId,
                    ProductId = line.ProductId,
                    BatchId = line.BatchId,
                    ProductUnitId = line.ProductUnitId,
                    MovementType = MovementTypes.TransferIn,
                    ReferenceType = "StockTransfer",
                    ReferenceId = transfer.Id,
                    Quantity = baseQty,
                    UnitCost = batch.PurchaseCost,
                    TotalCost = Math.Round(baseQty * batch.PurchaseCost, 4),
                    BalanceBefore = balanceBeforeIn,
                    BalanceAfter = balanceBeforeIn + baseQty,
                    MovementDate = now,
                    PerformedBy = userId,
                    IdempotencyKey = $"TR:{transfer.Id}:IN:{line.Id}",
                    CreatedAt = now
                });

                line.ReceivedQuantity = line.Quantity;
            }

            transfer.Status = "Received";
            transfer.ReceivedBy = userId;
            transfer.ApprovedBy ??= userId;

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

    private async Task ValidateLineAsync(long fromWhId, long toWhId, StockTransferLineRequest line, CancellationToken ct)
    {
        if (!await db.WarehouseLocations.AnyAsync(l => l.Id == line.FromLocationId && l.WarehouseId == fromWhId, ct))
            throw new ValidationAppException([$"From location {line.FromLocationId} not in from warehouse."]);
        if (!await db.WarehouseLocations.AnyAsync(l => l.Id == line.ToLocationId && l.WarehouseId == toWhId, ct))
            throw new ValidationAppException([$"To location {line.ToLocationId} not in to warehouse."]);
        if (!await db.InventoryBatches.AnyAsync(b => b.Id == line.BatchId && b.ProductId == line.ProductId, ct))
            throw new ValidationAppException([$"Batch {line.BatchId} does not match product {line.ProductId}."]);
        if (!await db.ProductUnits.AnyAsync(u => u.Id == line.ProductUnitId && u.ProductId == line.ProductId, ct))
            throw new ValidationAppException([$"Product unit {line.ProductUnitId} invalid."]);
    }

    private IQueryable<StockTransfer> Query() =>
        db.StockTransfers.AsNoTracking()
            .Include(t => t.StockTransferLines)
            .Include(t => t.FromWarehouse).ThenInclude(w => w.Branch);

    private static StockTransferDto Map(StockTransfer t) => new()
    {
        Id = t.Id,
        TransferNumber = t.TransferNumber,
        FromWarehouseId = t.FromWarehouseId,
        ToWarehouseId = t.ToWarehouseId,
        TransferDate = t.TransferDate,
        Status = t.Status,
        RequestedBy = t.RequestedBy,
        ApprovedBy = t.ApprovedBy,
        ReceivedBy = t.ReceivedBy,
        Lines = t.StockTransferLines.Select(l => new StockTransferLineDto
        {
            Id = l.Id,
            ProductId = l.ProductId,
            BatchId = l.BatchId,
            ProductUnitId = l.ProductUnitId,
            FromLocationId = l.FromLocationId,
            ToLocationId = l.ToLocationId,
            Quantity = l.Quantity,
            ReceivedQuantity = l.ReceivedQuantity
        }).ToList()
    };
}
