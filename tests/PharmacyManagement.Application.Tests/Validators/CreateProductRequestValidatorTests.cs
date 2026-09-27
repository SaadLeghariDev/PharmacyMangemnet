using FluentAssertions;
using PharmacyManagement.Application.DTOs.Products;
using PharmacyManagement.Application.Validators.Products;

namespace PharmacyManagement.Application.Tests.Validators;

public class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator = new();

    [Fact]
    public void Valid_product_passes()
    {
        var result = _validator.Validate(new CreateProductRequest
        {
            CategoryId = 1,
            ManufacturerId = 1,
            BrandId = 1,
            TherapeuticClassId = 1,
            Sku = "SKU-001",
            Name = "Paracetamol 500mg"
        });
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Invalid_category_fails(long categoryId)
    {
        var result = _validator.Validate(new CreateProductRequest
        {
            CategoryId = categoryId,
            ManufacturerId = 1,
            BrandId = 1,
            TherapeuticClassId = 1,
            Sku = "SKU-001",
            Name = "Paracetamol 500mg"
        });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_sku_fails()
    {
        var result = _validator.Validate(new CreateProductRequest
        {
            CategoryId = 1,
            ManufacturerId = 1,
            BrandId = 1,
            TherapeuticClassId = 1,
            Sku = "",
            Name = "Paracetamol 500mg"
        });
        result.IsValid.Should().BeFalse();
    }
}
