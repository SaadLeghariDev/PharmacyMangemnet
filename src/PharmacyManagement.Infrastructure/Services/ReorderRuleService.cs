using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Alerts;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class ReorderRuleService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IReorderRuleService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<ReorderRuleDto>> SearchAsync(ReorderRuleQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = BaseQuery(tenantId);

        if (query.BranchId is long branchId) q = q.Where(r => r.BranchId == branchId);
        if (query.WarehouseId is long warehouseId) q = q.Where(r => r.WarehouseId == warehouseId);
        if (query.ProductId is long productId) q = q.Where(r => r.ProductId == productId);
        if (query.IsActive is bool active) q = q.Where(r => r.IsActive == active);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(r =>
                r.Product.Sku.Contains(s) ||
                r.Product.Name.Contains(s) ||
                r.Warehouse.Code.Contains(s) ||
                r.PreferredSupplier.Name.Contains(s));
        }

        q = q.OrderBy(r => r.Product.Name).ThenBy(r => r.Warehouse.Code);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return new PagedResult<ReorderRuleDto>
        {
            Items = rows.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<ReorderRuleDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await BaseQuery(tenantId).FirstOrDefaultAsync(r => r.Id == id, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<ReorderRuleDto> CreateAsync(CreateReorderRuleRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await ValidateRefsAsync(tenantId, request.BranchId, request.WarehouseId, request.ProductId, request.PreferredSupplierId, ct);

        var duplicate = await db.ReorderRules.AnyAsync(r =>
            r.BranchId == request.BranchId &&
            r.WarehouseId == request.WarehouseId &&
            r.ProductId == request.ProductId &&
            r.IsActive, ct);
        if (duplicate)
            throw new ConflictException("An active reorder rule already exists for this product and warehouse.");

        var entity = new ReorderRule
        {
            BranchId = request.BranchId,
            WarehouseId = request.WarehouseId,
            ProductId = request.ProductId,
            MinimumStock = request.MinimumStock,
            MaximumStock = request.MaximumStock,
            ReorderPoint = request.ReorderPoint,
            ReorderQuantity = request.ReorderQuantity,
            PreferredSupplierId = request.PreferredSupplierId,
            IsActive = request.IsActive
        };
        db.ReorderRules.Add(entity);
        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<ReorderRuleDto> UpdateAsync(long id, UpdateReorderRuleRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.ReorderRules
            .Include(r => r.Branch)
            .FirstOrDefaultAsync(r => r.Id == id && r.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Reorder rule {id} not found.");

        var supplier = await db.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.PreferredSupplierId && s.TenantId == tenantId && s.IsActive, ct)
            ?? throw new ValidationAppException(["Preferred supplier not found."]);

        _ = supplier;

        entity.MinimumStock = request.MinimumStock;
        entity.MaximumStock = request.MaximumStock;
        entity.ReorderPoint = request.ReorderPoint;
        entity.ReorderQuantity = request.ReorderQuantity;
        entity.PreferredSupplierId = request.PreferredSupplierId;
        entity.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<PagedResult<LowStockCandidateDto>> GetLowStockCandidatesAsync(
        LowStockCandidateQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var rules = await db.ReorderRules.AsNoTracking()
            .Include(r => r.Branch)
            .Include(r => r.Warehouse)
            .Include(r => r.Product)
            .Include(r => r.PreferredSupplier)
            .Where(r => r.Branch.TenantId == tenantId && r.IsActive)
            .Where(r => query.BranchId == null || r.BranchId == query.BranchId)
            .Where(r => query.WarehouseId == null || r.WarehouseId == query.WarehouseId)
            .Where(r => query.ProductId == null || r.ProductId == query.ProductId)
            .ToListAsync(ct);

        if (rules.Count == 0)
        {
            return new PagedResult<LowStockCandidateDto>
            {
                Items = [],
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = 0
            };
        }

        var productIds = rules.Select(r => r.ProductId).Distinct().ToList();
        var warehouseIds = rules.Select(r => r.WarehouseId).Distinct().ToList();

        var stockRows = await db.InventoryBatchLocations.AsNoTracking()
            .Include(x => x.Batch)
            .Where(x =>
                productIds.Contains(x.Batch.ProductId) &&
                warehouseIds.Contains(x.Batch.WarehouseId) &&
                x.Batch.BatchStatus == BatchStatuses.Available &&
                !x.Batch.IsRecalled)
            .GroupBy(x => new { x.Batch.ProductId, x.Batch.WarehouseId })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.WarehouseId,
                OnHand = g.Sum(x => x.QuantityOnHand),
                Available = g.Sum(x => x.QuantityOnHand - x.ReservedQuantity)
            })
            .ToListAsync(ct);

        var stockMap = stockRows.ToDictionary(x => (x.ProductId, x.WarehouseId));

        var candidates = new List<LowStockCandidateDto>();
        foreach (var rule in rules)
        {
            stockMap.TryGetValue((rule.ProductId, rule.WarehouseId), out var stock);
            var onHand = stock?.OnHand ?? 0m;
            var available = stock?.Available ?? 0m;
            if (available > rule.ReorderPoint) continue;

            candidates.Add(new LowStockCandidateDto
            {
                ReorderRuleId = rule.Id,
                BranchId = rule.BranchId,
                BranchCode = rule.Branch.Code,
                WarehouseId = rule.WarehouseId,
                WarehouseCode = rule.Warehouse.Code,
                ProductId = rule.ProductId,
                ProductSku = rule.Product.Sku,
                ProductName = rule.Product.Name,
                MinimumStock = rule.MinimumStock,
                ReorderPoint = rule.ReorderPoint,
                ReorderQuantity = rule.ReorderQuantity,
                PreferredSupplierId = rule.PreferredSupplierId,
                PreferredSupplierName = rule.PreferredSupplier.Name,
                OnHandQuantity = onHand,
                AvailableQuantity = available,
                ShortageQuantity = Math.Max(0, rule.ReorderPoint - available)
            });
        }

        candidates = candidates
            .OrderBy(c => c.AvailableQuantity)
            .ThenBy(c => c.ProductName)
            .ToList();

        var total = candidates.Count;
        var pageItems = candidates
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return new PagedResult<LowStockCandidateDto>
        {
            Items = pageItems,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    private IQueryable<ReorderRule> BaseQuery(long tenantId) =>
        db.ReorderRules.AsNoTracking()
            .Include(r => r.Branch)
            .Include(r => r.Warehouse)
            .Include(r => r.Product)
            .Include(r => r.PreferredSupplier)
            .Where(r => r.Branch.TenantId == tenantId);

    private async Task ValidateRefsAsync(
        long tenantId, long branchId, long warehouseId, long productId, long supplierId, CancellationToken ct)
    {
        var branch = await db.Branches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == branchId && b.TenantId == tenantId && b.IsActive, ct)
            ?? throw new ValidationAppException(["Branch not found."]);

        var warehouse = await db.Warehouses.AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == warehouseId && w.BranchId == branch.Id && w.IsActive, ct)
            ?? throw new ValidationAppException(["Warehouse not found for branch."]);

        _ = warehouse;

        _ = await db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId && p.IsActive, ct)
            ?? throw new ValidationAppException(["Product not found."]);

        _ = await db.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == supplierId && s.TenantId == tenantId && s.IsActive, ct)
            ?? throw new ValidationAppException(["Preferred supplier not found."]);
    }

    private static ReorderRuleDto Map(ReorderRule r) => new()
    {
        Id = r.Id,
        BranchId = r.BranchId,
        BranchCode = r.Branch.Code,
        WarehouseId = r.WarehouseId,
        WarehouseCode = r.Warehouse.Code,
        ProductId = r.ProductId,
        ProductSku = r.Product.Sku,
        ProductName = r.Product.Name,
        MinimumStock = r.MinimumStock,
        MaximumStock = r.MaximumStock,
        ReorderPoint = r.ReorderPoint,
        ReorderQuantity = r.ReorderQuantity,
        PreferredSupplierId = r.PreferredSupplierId,
        PreferredSupplierName = r.PreferredSupplier.Name,
        IsActive = r.IsActive
    };
}
