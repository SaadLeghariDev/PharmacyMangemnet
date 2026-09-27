using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class StockAdjustmentService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    INumberSequenceService sequences) : IStockAdjustmentService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<StockAdjustmentDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await Query()
            .FirstOrDefaultAsync(a => a.Id == id && a.Branch.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<StockAdjustmentDto> CreateAsync(CreateStockAdjustmentRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        if (!await db.Branches.AnyAsync(b => b.Id == request.BranchId && b.TenantId == tenantId && b.IsActive, ct))
            throw new ValidationAppException(["Branch not found."]);
        if (!await db.Warehouses.AnyAsync(w => w.Id == request.WarehouseId && w.BranchId == request.BranchId && w.IsActive, ct))
            throw new ValidationAppException(["Warehouse not found for branch."]);
        if (!await db.ReasonCodes.AnyAsync(r => r.Id == request.ReasonCodeId && r.TenantId == tenantId && r.IsActive, ct))
            throw new ValidationAppException(["Reason code not found."]);

        foreach (var line in request.Lines)
            await ValidateLineAsync(request.WarehouseId, line, ct);

        var number = await sequences.AllocateNextAsync(
            tenantId, DocumentTypes.StockAdjustment, request.BranchId, null, "ADJ-", ct);

        var entity = new StockAdjustment
        {
            BranchId = request.BranchId,
            WarehouseId = request.WarehouseId,
            AdjustmentNumber = number,
            AdjustmentDate = DateTime.UtcNow,
            AdjustmentType = request.AdjustmentType,
            Status = "Draft",
            ReasonCodeId = request.ReasonCodeId,
            CreatedBy = currentUser.UserId
        };

        foreach (var line in request.Lines)
        {
            entity.StockAdjustmentLines.Add(new StockAdjustmentLine
            {
                ProductId = line.ProductId,
                BatchId = line.BatchId,
                WarehouseLocationId = line.WarehouseLocationId,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost
            });
        }

        db.StockAdjustments.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<StockAdjustmentDto> ApproveAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.StockAdjustments
            .Include(a => a.Branch)
            .FirstOrDefaultAsync(a => a.Id == id && a.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Stock adjustment {id} not found.");

        if (entity.Status is not ("Draft" or "PendingApproval"))
            throw new ValidationAppException([$"Adjustment cannot be approved from status '{entity.Status}'."]);

        entity.Status = "Approved";
        entity.ApprovedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<StockAdjustmentDto> PostAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var adj = await db.StockAdjustments
                .Include(a => a.StockAdjustmentLines)
                .Include(a => a.Branch)
                .FirstOrDefaultAsync(a => a.Id == id && a.Branch.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Stock adjustment {id} not found.");

            if (adj.Status != "Approved")
                throw new ValidationAppException([$"Only Approved adjustments can be posted (current: {adj.Status})."]);

            var now = DateTime.UtcNow;
            var userId = currentUser.UserId;

            foreach (var line in adj.StockAdjustmentLines)
            {
                var loc = await db.InventoryBatchLocations
                    .FirstOrDefaultAsync(l => l.BatchId == line.BatchId && l.WarehouseLocationId == line.WarehouseLocationId, ct)
                    ?? throw new ValidationAppException([$"No stock location for batch {line.BatchId}."]);

                var balanceBefore = loc.QuantityOnHand;
                var newQty = balanceBefore + line.Quantity;
                if (newQty < 0)
                    throw new ValidationAppException([$"Adjustment would make QoH negative for batch {line.BatchId}."]);
                if (newQty < loc.ReservedQuantity)
                    throw new ValidationAppException([$"Adjustment would violate reserved quantity for batch {line.BatchId}."]);

                loc.QuantityOnHand = newQty;
                loc.UpdatedAt = now;

                var baseUnit = await db.ProductUnits.AsNoTracking()
                    .FirstAsync(u => u.ProductId == line.ProductId && u.IsBaseUnit, ct);

                db.InventoryMovements.Add(new InventoryMovement
                {
                    BranchId = adj.BranchId,
                    WarehouseId = adj.WarehouseId,
                    WarehouseLocationId = line.WarehouseLocationId,
                    ProductId = line.ProductId,
                    BatchId = line.BatchId,
                    ProductUnitId = baseUnit.Id,
                    MovementType = MovementTypes.Adjustment,
                    ReferenceType = "StockAdjustment",
                    ReferenceId = adj.Id,
                    Quantity = line.Quantity,
                    UnitCost = line.UnitCost,
                    TotalCost = Math.Round(Math.Abs(line.Quantity) * line.UnitCost, 4),
                    BalanceBefore = balanceBefore,
                    BalanceAfter = newQty,
                    MovementDate = now,
                    PerformedBy = userId,
                    IdempotencyKey = $"ADJ:{adj.Id}:LINE:{line.Id}",
                    CreatedAt = now
                });
            }

            adj.Status = "Posted";
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

    private async Task ValidateLineAsync(long warehouseId, StockAdjustmentLineRequest line, CancellationToken ct)
    {
        if (!await db.WarehouseLocations.AnyAsync(l => l.Id == line.WarehouseLocationId && l.WarehouseId == warehouseId, ct))
            throw new ValidationAppException([$"Location {line.WarehouseLocationId} not in warehouse."]);
        if (!await db.InventoryBatches.AnyAsync(b => b.Id == line.BatchId && b.ProductId == line.ProductId, ct))
            throw new ValidationAppException([$"Batch {line.BatchId} invalid for product."]);
    }

    private IQueryable<StockAdjustment> Query() =>
        db.StockAdjustments.AsNoTracking()
            .Include(a => a.StockAdjustmentLines)
            .Include(a => a.Branch);

    private static StockAdjustmentDto Map(StockAdjustment a) => new()
    {
        Id = a.Id,
        BranchId = a.BranchId,
        WarehouseId = a.WarehouseId,
        AdjustmentNumber = a.AdjustmentNumber,
        AdjustmentDate = a.AdjustmentDate,
        AdjustmentType = a.AdjustmentType,
        Status = a.Status,
        ReasonCodeId = a.ReasonCodeId,
        CreatedBy = a.CreatedBy,
        ApprovedBy = a.ApprovedBy,
        Lines = a.StockAdjustmentLines.Select(l => new StockAdjustmentLineDto
        {
            Id = l.Id,
            ProductId = l.ProductId,
            BatchId = l.BatchId,
            WarehouseLocationId = l.WarehouseLocationId,
            Quantity = l.Quantity,
            UnitCost = l.UnitCost
        }).ToList()
    };
}
