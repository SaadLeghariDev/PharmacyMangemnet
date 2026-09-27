using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class InventoryQueryService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IInventoryQueryService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<StockBalanceDto>> GetStockAsync(StockQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = BaseStockQuery(tenantId);

        if (query.ProductId is long productId) q = q.Where(x => x.Batch.ProductId == productId);
        if (query.WarehouseId is long warehouseId) q = q.Where(x => x.Batch.WarehouseId == warehouseId);
        if (query.WarehouseLocationId is long locId) q = q.Where(x => x.WarehouseLocationId == locId);
        if (query.BatchId is long batchId) q = q.Where(x => x.BatchId == batchId);
        if (query.IncludeZero != true)
            q = q.Where(x => x.QuantityOnHand - x.ReservedQuantity > 0);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x =>
                x.Batch.BatchNumber.Contains(s) ||
                x.Batch.Product.Sku.Contains(s) ||
                x.Batch.Product.Name.Contains(s));
        }

        q = q.OrderBy(x => x.Batch.Product.Name).ThenBy(x => x.Batch.ExpiryDate);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return new PagedResult<StockBalanceDto>
        {
            Items = rows.Select(MapStock).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<IReadOnlyList<FefoCandidateDto>> GetFefoCandidatesAsync(FefoQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var q = db.InventoryBatchLocations.AsNoTracking()
            .Include(x => x.Batch).ThenInclude(b => b.Product)
            .Where(x =>
                x.Batch.Product.TenantId == tenantId &&
                x.Batch.ProductId == query.ProductId &&
                x.Batch.BatchStatus == BatchStatuses.Available &&
                !x.Batch.IsRecalled &&
                x.Batch.ExpiryDate >= today &&
                x.QuantityOnHand - x.ReservedQuantity > 0);

        if (query.WarehouseId is long warehouseId)
            q = q.Where(x => x.Batch.WarehouseId == warehouseId);
        if (query.WarehouseLocationId is long locId)
            q = q.Where(x => x.WarehouseLocationId == locId);

        q = q.OrderBy(x => x.Batch.ExpiryDate).ThenBy(x => x.BatchId);

        var rows = await q.ToListAsync(ct);
        var result = new List<FefoCandidateDto>();
        decimal remaining = query.RequiredQuantity ?? decimal.MaxValue;

        foreach (var row in rows)
        {
            var available = row.QuantityOnHand - row.ReservedQuantity;
            result.Add(new FefoCandidateDto
            {
                BatchId = row.BatchId,
                ProductId = row.Batch.ProductId,
                BatchNumber = row.Batch.BatchNumber,
                ExpiryDate = row.Batch.ExpiryDate,
                WarehouseLocationId = row.WarehouseLocationId,
                AvailableQuantity = available,
                PurchaseCost = row.Batch.PurchaseCost,
                SalePrice = row.Batch.SalePrice
            });

            if (query.RequiredQuantity.HasValue)
            {
                remaining -= available;
                if (remaining <= 0) break;
            }
        }

        return result;
    }

    public async Task<PagedResult<StockBalanceDto>> GetNearExpiryAsync(NearExpiryQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = today.AddDays(Math.Max(query.DaysAhead, 0));

        var q = BaseStockQuery(tenantId)
            .Where(x =>
                x.Batch.ExpiryDate >= today &&
                x.Batch.ExpiryDate <= cutoff &&
                x.Batch.BatchStatus == BatchStatuses.Available &&
                !x.Batch.IsRecalled &&
                x.QuantityOnHand - x.ReservedQuantity > 0);

        if (query.WarehouseId is long warehouseId) q = q.Where(x => x.Batch.WarehouseId == warehouseId);
        if (query.ProductId is long productId) q = q.Where(x => x.Batch.ProductId == productId);

        q = q.OrderBy(x => x.Batch.ExpiryDate);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return new PagedResult<StockBalanceDto>
        {
            Items = rows.Select(MapStock).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    private IQueryable<Persistence.Entities.InventoryBatchLocation> BaseStockQuery(long tenantId) =>
        db.InventoryBatchLocations.AsNoTracking()
            .Include(x => x.Batch).ThenInclude(b => b.Product)
            .Include(x => x.WarehouseLocation)
            .Where(x => x.Batch.Product.TenantId == tenantId);

    private static StockBalanceDto MapStock(Persistence.Entities.InventoryBatchLocation x) => new()
    {
        ProductId = x.Batch.ProductId,
        Sku = x.Batch.Product.Sku,
        ProductName = x.Batch.Product.Name,
        BatchId = x.BatchId,
        BatchNumber = x.Batch.BatchNumber,
        ExpiryDate = x.Batch.ExpiryDate,
        BatchStatus = x.Batch.BatchStatus,
        IsRecalled = x.Batch.IsRecalled,
        WarehouseId = x.Batch.WarehouseId,
        WarehouseLocationId = x.WarehouseLocationId,
        LocationCode = x.WarehouseLocation.Code,
        QuantityOnHand = x.QuantityOnHand,
        ReservedQuantity = x.ReservedQuantity,
        AvailableQuantity = x.QuantityOnHand - x.ReservedQuantity
    };
}
