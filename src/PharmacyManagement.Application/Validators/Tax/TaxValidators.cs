using FluentValidation;
using PharmacyManagement.Application.DTOs.Tax;

namespace PharmacyManagement.Application.Validators.Tax;

public sealed class CreateTaxProfileRequestValidator : AbstractValidator<CreateTaxProfileRequest>
{
    public CreateTaxProfileRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tax profile name is required.").MaximumLength(150);
        RuleFor(x => x.TaxType).MaximumLength(50).When(x => x.TaxType is not null);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
    }
}

public sealed class UpdateTaxProfileRequestValidator : AbstractValidator<UpdateTaxProfileRequest>
{
    public UpdateTaxProfileRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tax profile name is required.").MaximumLength(150);
        RuleFor(x => x.TaxType).MaximumLength(50).When(x => x.TaxType is not null);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
    }
}

public sealed class CreateTaxRateRequestValidator : AbstractValidator<CreateTaxRateRequest>
{
    public CreateTaxRateRequestValidator()
    {
        RuleFor(x => x.Rate).GreaterThanOrEqualTo(0).WithMessage("Tax rate must be zero or greater.");
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo must be on or after EffectiveFrom.");
    }
}

public sealed class UpdateTaxRateRequestValidator : AbstractValidator<UpdateTaxRateRequest>
{
    public UpdateTaxRateRequestValidator()
    {
        RuleFor(x => x.Rate).GreaterThanOrEqualTo(0).WithMessage("Tax rate must be zero or greater.");
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo must be on or after EffectiveFrom.");
    }
}

public sealed class ReplaceProductTaxProfilesRequestValidator : AbstractValidator<ReplaceProductTaxProfilesRequest>
{
    public ReplaceProductTaxProfilesRequestValidator()
    {
        RuleFor(x => x.TaxProfileIds).NotNull();
        RuleForEach(x => x.TaxProfileIds).GreaterThan(0);
    }
}
