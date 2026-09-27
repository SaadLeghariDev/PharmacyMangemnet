using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Products;

public sealed class ProductDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public long ManufacturerId { get; set; }
    public string? ManufacturerName { get; set; }
    public long BrandId { get; set; }
    public string? BrandName { get; set; }
    public long TherapeuticClassId { get; set; }
    public string? TherapeuticClassName { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Form { get; set; }
    public string? Strength { get; set; }
    public string? StrengthUnit { get; set; }
    public string? PackDescription { get; set; }
    public bool PrescriptionRequired { get; set; }
    public bool IsControlled { get; set; }
    public bool IsTemperatureSensitive { get; set; }
    public bool IsRefrigerated { get; set; }
    public bool IsReturnable { get; set; }
    public bool IsSaleable { get; set; }
    public bool IsActive { get; set; }
    /// <summary>Preferred sale unit for POS (base sale unit when present).</summary>
    public long? DefaultSaleUnitId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateProductRequest
{
    public long CategoryId { get; set; }
    public long ManufacturerId { get; set; }
    public long BrandId { get; set; }
    public long TherapeuticClassId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Form { get; set; }
    public string? Strength { get; set; }
    public string? StrengthUnit { get; set; }
    public string? PackDescription { get; set; }
    public bool PrescriptionRequired { get; set; }
    public bool IsControlled { get; set; }
    public bool IsTemperatureSensitive { get; set; }
    public bool IsRefrigerated { get; set; }
    public bool IsReturnable { get; set; } = true;
    public bool IsSaleable { get; set; } = true;
}

public sealed class UpdateProductRequest
{
    public long CategoryId { get; set; }
    public long ManufacturerId { get; set; }
    public long BrandId { get; set; }
    public long TherapeuticClassId { get; set; }
    public string? ProductCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Form { get; set; }
    public string? Strength { get; set; }
    public string? StrengthUnit { get; set; }
    public string? PackDescription { get; set; }
    public bool PrescriptionRequired { get; set; }
    public bool IsControlled { get; set; }
    public bool IsTemperatureSensitive { get; set; }
    public bool IsRefrigerated { get; set; }
    public bool IsReturnable { get; set; }
    public bool IsSaleable { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ProductQuery : PaginationQuery
{
    public long? CategoryId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class BarcodeLookupDto
{
    public long BarcodeId { get; set; }
    public string BarcodeValue { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public long ProductUnitId { get; set; }
    public bool IsPrimary { get; set; }
}
