using FluentValidation;
using PharmacyManagement.Application.DTOs.Fiscal;

namespace PharmacyManagement.Application.Validators.Fiscal;

public sealed class CreateFiscalDocumentRequestValidator : AbstractValidator<CreateFiscalDocumentRequest>
{
    public CreateFiscalDocumentRequestValidator()
    {
        RuleFor(x => x.SaleId).GreaterThan(0);
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Provider).MaximumLength(100).When(x => x.Provider is not null);
    }
}
