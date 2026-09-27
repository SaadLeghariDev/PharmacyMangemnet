using FluentValidation;
using PharmacyManagement.Application.DTOs.Pricing;

namespace PharmacyManagement.Application.Validators.Pricing;

public sealed class CreatePriceListRequestValidator : AbstractValidator<CreatePriceListRequest>
{
    public CreatePriceListRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Price list name is required.").MaximumLength(150);
        RuleFor(x => x.PriceType).MaximumLength(50).When(x => x.PriceType is not null);
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3).WithMessage("Currency code must be 3 characters.");
    }
}

public sealed class UpdatePriceListRequestValidator : AbstractValidator<UpdatePriceListRequest>
{
    public UpdatePriceListRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Price list name is required.").MaximumLength(150);
        RuleFor(x => x.PriceType).MaximumLength(50).When(x => x.PriceType is not null);
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3).WithMessage("Currency code must be 3 characters.");
    }
}

public sealed class CreateProductPriceRequestValidator : AbstractValidator<CreateProductPriceRequest>
{
    public CreateProductPriceRequestValidator()
    {
        RuleFor(x => x.PriceListId).GreaterThan(0);
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.ProductUnitId).GreaterThan(0);
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Mrp).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo must be after EffectiveFrom.");
    }
}

public sealed class UpdateProductPriceRequestValidator : AbstractValidator<UpdateProductPriceRequest>
{
    public UpdateProductPriceRequestValidator()
    {
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Mrp).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo must be after EffectiveFrom.");
    }
}
