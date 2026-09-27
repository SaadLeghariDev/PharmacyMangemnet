using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Tax;

public sealed class TaxProfileQuery : PaginationQuery
{
    public bool? IsActive { get; set; }
}

public sealed class TaxProfileDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxType { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<TaxRateDto> Rates { get; set; } = Array.Empty<TaxRateDto>();
}

public sealed class CreateTaxProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TaxType { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateTaxProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TaxType { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class TaxRateDto
{
    public long Id { get; set; }
    public long TaxProfileId { get; set; }
    public decimal Rate { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateTaxRateRequest
{
    public decimal Rate { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateTaxRateRequest
{
    public decimal Rate { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ProductTaxProfileDto
{
    public long TaxProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxType { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ReplaceProductTaxProfilesRequest
{
    public IReadOnlyList<long> TaxProfileIds { get; set; } = Array.Empty<long>();
}

public sealed class InvoiceTaxDto
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public long? SaleLineId { get; set; }
    public long TaxProfileId { get; set; }
    public string? TaxProfileName { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
}
