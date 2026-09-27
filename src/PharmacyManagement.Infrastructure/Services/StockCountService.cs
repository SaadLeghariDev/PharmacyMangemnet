using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class StockCountService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    INumberSequenceService sequences) : IStockCountService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<StockCountDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await Query()
            .FirstOrDefaultAsync(c => c.Id == id && c.Branch.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<StockCountDto> CreateAsync(CreateStockCountRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        if (!await db.Branches.AnyAsync(b => b.Id == request.BranchId && b.TenantId == tenantId && b.IsActive, ct))
            throw new ValidationAppException(["Branch not found."]);
        if (!await db.Warehouses.AnyAsync(w => w.Id == request.WarehouseId && w.BranchId == request.BranchId && w.IsActive, ct))
            throw new ValidationAppException(["Warehouse not found for branch."]);

        var number = await sequences.AllocateNextAsync(
            tenantId, DocumentTypes.StockCount, request.BranchId, null, "CNT-", ct);

        var entity = new StockCount
        {
            BranchId = request.BranchId,
            WarehouseId = request.WarehouseId,
            CountNumber = number,
            CountDate = DateTime.UtcNow,
            Status = "Draft",
            CountType = request.CountType,
            CreatedBy = currentUser.UserId
        };

        foreach (var line in request.Lines)
        {
            if (!await db.WarehouseLocations.AnyAsync(l =>
                    l.Id == line.WarehouseLocationId && l.WarehouseId == request.WarehouseId, ct))
                throw new ValidationAppException([$"Location {line.WarehouseLocationId} not in warehouse."]);
            if (!await db.ReasonCodes.AnyAsync(r => r.Id == line.ReasonCodeId && r.TenantId == tenantId, ct))
                throw new ValidationAppException([$"Reason code {line.ReasonCodeId} not found."]);

            var loc = await db.InventoryBatchLocations.AsNoTracking()
                .FirstOrDefaultAsync(l => l.BatchId == line.BatchId && l.WarehouseLocationId == line.WarehouseLocationId, ct);
            var systemQty = loc?.QuantityOnHand ?? 0m;

            entity.StockCountLines.Add(new StockCountLine
            {
                ProductId = line.ProductId,
                BatchId = line.BatchId,
                WarehouseLocationId = line.WarehouseLocationId,
                SystemQuantity = systemQty,
                CountedQuantity = line.CountedQuantity,
                VarianceQuantity = line.CountedQuantity - systemQty,
                ReasonCodeId = line.ReasonCodeId
            });
        }

        db.StockCounts.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<StockCountDto> CompleteAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var count = await db.StockCounts
                .Include(c => c.StockCountLines)
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.Id == id && c.Branch.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Stock count {id} not found.");

            if (count.Status is not ("Draft" or "InProgress"))
                throw new ValidationAppException([$"Count cannot be completed from status '{count.Status}'."]);

            var now = DateTime.UtcNow;
            var userId = currentUser.UserId;

            foreach (var line in count.StockCountLines)
            {
                // Refresh system qty and variance at complete time.
                var loc = await db.InventoryBatchLocations
                    .FirstOrDefaultAsync(l => l.BatchId == line.BatchId && l.WarehouseLocationId == line.WarehouseLocationId, ct);

                if (loc is null)
                {
                    if (line.CountedQuantity < 0)
                        throw new ValidationAppException(["Counted quantity cannot be negative."]);
                    if (line.CountedQuantity == 0)
                    {
                        line.SystemQuantity = 0;
                        line.VarianceQuantity = 0;
                        continue;
                    }

                    loc = new InventoryBatchLocation
                    {
                        BatchId = line.BatchId,
                        WarehouseLocationId = line.WarehouseLocationId,
                        QuantityOnHand = 0,
                        ReservedQuantity = 0,
                        UpdatedAt = now,
                        RowVersion = new byte[8]
                    };
                    db.InventoryBatchLocations.Add(loc);
                }

                var systemQty = loc.QuantityOnHand;
                var variance = line.CountedQuantity - systemQty;
                line.SystemQuantity = systemQty;
                line.VarianceQuantity = variance;

                if (variance == 0) continue;

                var newQty = line.CountedQuantity;
                if (newQty < loc.ReservedQuantity)
                    throw new ValidationAppException([$"Count would violate reserved qty for batch {line.BatchId}."]);

                var balanceBefore = loc.QuantityOnHand;
                loc.QuantityOnHand = newQty;
                loc.UpdatedAt = now;

                var batch = await db.InventoryBatches.AsNoTracking().FirstAsync(b => b.Id == line.BatchId, ct);
                var baseUnit = await db.ProductUnits.AsNoTracking()
                    .FirstAsync(u => u.ProductId == line.ProductId && u.IsBaseUnit, ct);

                db.InventoryMovements.Add(new InventoryMovement
                {
                    BranchId = count.BranchId,
                    WarehouseId = count.WarehouseId,
                    WarehouseLocationId = line.WarehouseLocationId,
                    ProductId = line.ProductId,
                    BatchId = line.BatchId,
                    ProductUnitId = baseUnit.Id,
                    MovementType = MovementTypes.StockCount,
                    ReferenceType = "StockCount",
                    ReferenceId = count.Id,
                    Quantity = variance,
                    UnitCost = batch.PurchaseCost,
                    TotalCost = Math.Round(Math.Abs(variance) * batch.PurchaseCost, 4),
                    BalanceBefore = balanceBefore,
                    BalanceAfter = newQty,
                    MovementDate = now,
                    PerformedBy = userId,
                    IdempotencyKey = $"CNT:{count.Id}:LINE:{line.Id}",
                    CreatedAt = now
                });
            }

            count.Status = "Completed";
            count.ApprovedBy = userId;

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

    private IQueryable<StockCount> Query() =>
        db.StockCounts.AsNoTracking()
            .Include(c => c.StockCountLines)
            .Include(c => c.Branch);

    private static StockCountDto Map(StockCount c) => new()
    {
        Id = c.Id,
        BranchId = c.BranchId,
        WarehouseId = c.WarehouseId,
        CountNumber = c.CountNumber,
        CountDate = c.CountDate,
        Status = c.Status,
        CountType = c.CountType,
        CreatedBy = c.CreatedBy,
        ApprovedBy = c.ApprovedBy,
        Lines = c.StockCountLines.Select(l => new StockCountLineDto
        {
            Id = l.Id,
            ProductId = l.ProductId,
            BatchId = l.BatchId,
            WarehouseLocationId = l.WarehouseLocationId,
            SystemQuantity = l.SystemQuantity,
            CountedQuantity = l.CountedQuantity,
            VarianceQuantity = l.VarianceQuantity,
            ReasonCodeId = l.ReasonCodeId
        }).ToList()
    };
}
