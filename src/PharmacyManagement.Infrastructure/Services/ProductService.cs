using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Products;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class ProductService(PharmacyManagementDbContext db, ICurrentUserService currentUser) : IProductService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<ProductDto>> SearchAsync(ProductQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .Include(p => p.Brand)
            .Include(p => p.TherapeuticClass)
            .Include(p => p.ProductUnits)
            .Where(p => p.TenantId == tenantId);

        if (query.CategoryId is long catId)
            q = q.Where(p => p.CategoryId == catId);
        if (query.IsActive is bool active)
            q = q.Where(p => p.IsActive == active);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(p =>
                p.Name.Contains(s) ||
                p.Sku.Contains(s) ||
                (p.GenericName != null && p.GenericName.Contains(s)) ||
                (p.ProductCode != null && p.ProductCode.Contains(s)));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "sku" => query.SortDesc ? q.OrderByDescending(p => p.Sku) : q.OrderBy(p => p.Sku),
            "name" => query.SortDesc ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name),
            "createdat" => query.SortDesc ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt),
            _ => q.OrderBy(p => p.Name)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<ProductDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<ProductDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var p = await QueryProducts().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
        return p is null ? null : Map(p);
    }

    public async Task<ProductDto?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var p = await QueryProducts().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Sku == sku, ct);
        return p is null ? null : Map(p);
    }

    public async Task<BarcodeLookupDto?> GetByBarcodeAsync(string barcode, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var row = await db.Barcodes.AsNoTracking()
            .Include(b => b.Product)
            .Where(b => b.BarcodeValue == barcode && b.IsActive && b.Product.TenantId == tenantId)
            .FirstOrDefaultAsync(ct);
        if (row is null) return null;
        return new BarcodeLookupDto
        {
            BarcodeId = row.Id,
            BarcodeValue = row.BarcodeValue,
            ProductId = row.ProductId,
            Sku = row.Product.Sku,
            ProductName = row.Product.Name,
            ProductUnitId = row.ProductUnitId,
            IsPrimary = row.IsPrimary
        };
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await EnsureFkAsync(request.CategoryId, request.ManufacturerId, request.BrandId, request.TherapeuticClassId, ct);

        if (await db.Products.AnyAsync(p => p.TenantId == tenantId && p.Sku == request.Sku, ct))
            throw new ConflictException($"SKU '{request.Sku}' already exists for tenant.");

        var entity = new Product
        {
            TenantId = tenantId,
            CategoryId = request.CategoryId,
            ManufacturerId = request.ManufacturerId,
            BrandId = request.BrandId,
            TherapeuticClassId = request.TherapeuticClassId,
            Sku = request.Sku.Trim(),
            ProductCode = request.ProductCode,
            Name = request.Name.Trim(),
            GenericName = request.GenericName,
            Form = request.Form,
            Strength = request.Strength,
            StrengthUnit = request.StrengthUnit,
            PackDescription = request.PackDescription,
            PrescriptionRequired = request.PrescriptionRequired,
            IsControlled = request.IsControlled,
            IsTemperatureSensitive = request.IsTemperatureSensitive,
            IsRefrigerated = request.IsRefrigerated,
            IsReturnable = request.IsReturnable,
            IsSaleable = request.IsSaleable,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            // SQL Server generates ROWVERSION; placeholder satisfies InMemory provider in tests.
            RowVersion = new byte[8]
        };
        db.Products.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<ProductDto> UpdateAsync(long id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Products.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Product {id} not found.");

        await EnsureFkAsync(request.CategoryId, request.ManufacturerId, request.BrandId, request.TherapeuticClassId, ct);

        entity.CategoryId = request.CategoryId;
        entity.ManufacturerId = request.ManufacturerId;
        entity.BrandId = request.BrandId;
        entity.TherapeuticClassId = request.TherapeuticClassId;
        entity.ProductCode = request.ProductCode;
        entity.Name = request.Name.Trim();
        entity.GenericName = request.GenericName;
        entity.Form = request.Form;
        entity.Strength = request.Strength;
        entity.StrengthUnit = request.StrengthUnit;
        entity.PackDescription = request.PackDescription;
        entity.PrescriptionRequired = request.PrescriptionRequired;
        entity.IsControlled = request.IsControlled;
        entity.IsTemperatureSensitive = request.IsTemperatureSensitive;
        entity.IsRefrigerated = request.IsRefrigerated;
        entity.IsReturnable = request.IsReturnable;
        entity.IsSaleable = request.IsSaleable;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task DeactivateAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Products.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Product {id} not found.");
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private IQueryable<Product> QueryProducts() =>
        db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .Include(p => p.Brand)
            .Include(p => p.TherapeuticClass)
            .Include(p => p.ProductUnits);

    private async Task EnsureFkAsync(long categoryId, long manufacturerId, long brandId, long therapeuticClassId, CancellationToken ct)
    {
        var errors = new List<string>();
        if (!await db.ProductCategories.AnyAsync(c => c.Id == categoryId, ct))
            errors.Add($"Category {categoryId} not found.");
        if (!await db.Manufacturers.AnyAsync(m => m.Id == manufacturerId, ct))
            errors.Add($"Manufacturer {manufacturerId} not found.");
        if (!await db.Brands.AnyAsync(b => b.Id == brandId && b.ManufacturerId == manufacturerId, ct))
            errors.Add($"Brand {brandId} not found for manufacturer {manufacturerId}.");
        if (!await db.TherapeuticClasses.AnyAsync(t => t.Id == therapeuticClassId, ct))
            errors.Add($"Therapeutic class {therapeuticClassId} not found.");
        if (errors.Count > 0)
            throw new ValidationAppException(errors);
    }

    private static ProductDto Map(Product p)
    {
        var saleUnits = p.ProductUnits?
            .Where(u => u.IsActive && (u.IsSaleUnit || u.IsBaseUnit))
            .ToList() ?? [];
        var defaultSaleUnit = saleUnits.FirstOrDefault(u => u.IsSaleUnit && u.IsBaseUnit)
            ?? saleUnits.FirstOrDefault(u => u.IsSaleUnit)
            ?? saleUnits.FirstOrDefault(u => u.IsBaseUnit);

        return new ProductDto
        {
            Id = p.Id,
            TenantId = p.TenantId,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name,
            ManufacturerId = p.ManufacturerId,
            ManufacturerName = p.Manufacturer?.Name,
            BrandId = p.BrandId,
            BrandName = p.Brand?.Name,
            TherapeuticClassId = p.TherapeuticClassId,
            TherapeuticClassName = p.TherapeuticClass?.Name,
            Sku = p.Sku,
            ProductCode = p.ProductCode,
            Name = p.Name,
            GenericName = p.GenericName,
            Form = p.Form,
            Strength = p.Strength,
            StrengthUnit = p.StrengthUnit,
            PackDescription = p.PackDescription,
            PrescriptionRequired = p.PrescriptionRequired,
            IsControlled = p.IsControlled,
            IsTemperatureSensitive = p.IsTemperatureSensitive,
            IsRefrigerated = p.IsRefrigerated,
            IsReturnable = p.IsReturnable,
            IsSaleable = p.IsSaleable,
            IsActive = p.IsActive,
            DefaultSaleUnitId = defaultSaleUnit?.Id,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }
}
