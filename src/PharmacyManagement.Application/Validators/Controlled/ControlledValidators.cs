using FluentValidation;
using PharmacyManagement.Application.DTOs.Controlled;

namespace PharmacyManagement.Application.Validators.Controlled;

public sealed class OpenControlledRegisterRequestValidator : AbstractValidator<OpenControlledRegisterRequest>
{
    public OpenControlledRegisterRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.OpeningBalance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RegisterNumber).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.RegisterNumber));
    }
}

public sealed class PostControlledTransactionRequestValidator : AbstractValidator<PostControlledTransactionRequest>
{
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "Receipt", "Adjustment", "Destruction", "TransferIn", "TransferOut"
    };

    public PostControlledTransactionRequestValidator()
    {
        RuleFor(x => x.TransactionType)
            .NotEmpty()
            .Must(t => Allowed.Contains(t))
            .WithMessage("TransactionType must be Receipt, Adjustment, Destruction, TransferIn, or TransferOut.");
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Remarks).MaximumLength(1000).When(x => x.Remarks is not null);
    }
}
