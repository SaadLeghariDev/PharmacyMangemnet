using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Pricing;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class ProductPriceService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IProductPriceService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<ProductPriceDto>> SearchAsync(ProductPriceQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var now = DateTime.UtcNow;
        var q = db.ProductPrices.AsNoTracking()
            .Include(p => p.PriceList)
            .Include(p => p.Product)
            .Include(p => p.ProductUnit).ThenInclude(u => u.Unit)
            .Where(p => p.PriceList.TenantId == tenantId);

        if (query.ProductId is long productId) q = q.Where(p => p.ProductId == productId);
        if (query.PriceListId is long priceListId) q = q.Where(p => p.PriceListId == priceListId);
        if (query.ProductUnitId is long unitId) q = q.Where(p => p.ProductUnitId == unitId);
        if (query.ActiveOnly == true)
            q = q.Where(p => p.EffectiveFrom <= now && (p.EffectiveTo == null || p.EffectiveTo >= now));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(p =>
                p.Product.Name.Contains(s) ||
                p.Product.Sku.Contains(s) ||
                p.PriceList.Name.Contains(s));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "saleprice" => query.SortDesc ? q.OrderByDescending(p => p.SalePrice) : q.OrderBy(p => p.SalePrice),
            "effectivefrom" => query.SortDesc ? q.OrderByDescending(p => p.EffectiveFrom) : q.OrderBy(p => p.EffectiveFrom),
            _ => q.OrderByDescending(p => p.EffectiveFrom).ThenByDescending(p => p.Id)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<ProductPriceDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<ProductPriceDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.ProductPrices.AsNoTracking()
            .Include(p => p.PriceList)
            .Include(p => p.Product)
            .Include(p => p.ProductUnit).ThenInclude(u => u.Unit)
            .FirstOrDefaultAsync(p => p.Id == id && p.PriceList.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<ProductPriceDto> CreateAsync(CreateProductPriceRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();

        var priceList = await db.PriceLists.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PriceListId && p.TenantId == tenantId && p.IsActive, ct)
            ?? throw new ValidationAppException(["Price list not found."]);

        if (!await db.Products.AnyAsync(p => p.Id == request.ProductId && p.TenantId == tenantId && p.IsActive, ct))
            throw new ValidationAppException(["Product not found."]);

        if (!await db.ProductUnits.AnyAsync(u =>
                u.Id == request.ProductUnitId && u.ProductId == request.ProductId && u.IsActive, ct))
            throw new ValidationAppException(["Product unit not found for product."]);

        var entity = new ProductPrice
        {
            PriceListId = priceList.Id,
            ProductId = request.ProductId,
            ProductUnitId = request.ProductUnitId,
            PurchasePrice = Math.Round(request.PurchasePrice, 4),
            SalePrice = Math.Round(request.SalePrice, 4),
            Mrp = Math.Round(request.Mrp, 4),
            DiscountPercent = Math.Round(request.DiscountPercent, 4),
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo
        };
        db.ProductPrices.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<ProductPriceDto> UpdateAsync(long id, UpdateProductPriceRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.ProductPrices
            .Include(p => p.PriceList)
            .FirstOrDefaultAsync(p => p.Id == id && p.PriceList.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Product price {id} not found.");

        entity.PurchasePrice = Math.Round(request.PurchasePrice, 4);
        entity.SalePrice = Math.Round(request.SalePrice, 4);
        entity.Mrp = Math.Round(request.Mrp, 4);
        entity.DiscountPercent = Math.Round(request.DiscountPercent, 4);
        entity.EffectiveFrom = request.EffectiveFrom;
        entity.EffectiveTo = request.EffectiveTo;
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(id, ct))!;
    }

    private static ProductPriceDto Map(ProductPrice p) => new()
    {
        Id = p.Id,
        PriceListId = p.PriceListId,
        PriceListName = p.PriceList?.Name,
        PriceListIsDefault = p.PriceList?.IsDefault ?? false,
        ProductId = p.ProductId,
        ProductSku = p.Product?.Sku,
        ProductName = p.Product?.Name,
        ProductUnitId = p.ProductUnitId,
        UnitName = p.ProductUnit?.Unit?.ShortCode ?? p.ProductUnit?.Unit?.Name,
        PurchasePrice = p.PurchasePrice,
        SalePrice = p.SalePrice,
        Mrp = p.Mrp,
        DiscountPercent = p.DiscountPercent,
        EffectiveFrom = p.EffectiveFrom,
        EffectiveTo = p.EffectiveTo
    };
}
