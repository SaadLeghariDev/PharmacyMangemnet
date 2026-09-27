using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Pricing;

public sealed class PriceListQuery : PaginationQuery
{
    public bool? IsActive { get; set; }
    public bool? IsDefault { get; set; }
}

public sealed class PriceListDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PriceType { get; set; }
    public string CurrencyCode { get; set; } = "PKR";
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreatePriceListRequest
{
    public string Name { get; set; } = string.Empty;
    public string? PriceType { get; set; }
    public string CurrencyCode { get; set; } = "PKR";
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdatePriceListRequest
{
    public string Name { get; set; } = string.Empty;
    public string? PriceType { get; set; }
    public string CurrencyCode { get; set; } = "PKR";
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ProductPriceQuery : PaginationQuery
{
    public long? ProductId { get; set; }
    public long? PriceListId { get; set; }
    public long? ProductUnitId { get; set; }
    public bool? ActiveOnly { get; set; }
}

public sealed class ProductPriceDto
{
    public long Id { get; set; }
    public long PriceListId { get; set; }
    public string? PriceListName { get; set; }
    public bool PriceListIsDefault { get; set; }
    public long ProductId { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductName { get; set; }
    public long ProductUnitId { get; set; }
    public string? UnitName { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal Mrp { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public sealed class CreateProductPriceRequest
{
    public long PriceListId { get; set; }
    public long ProductId { get; set; }
    public long ProductUnitId { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal Mrp { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public sealed class UpdateProductPriceRequest
{
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal Mrp { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
