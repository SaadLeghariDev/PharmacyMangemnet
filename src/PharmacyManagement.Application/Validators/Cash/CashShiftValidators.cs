using FluentValidation;
using PharmacyManagement.Application.DTOs.Cash;

namespace PharmacyManagement.Application.Validators.Cash;

public sealed class OpenCashShiftRequestValidator : AbstractValidator<OpenCashShiftRequest>
{
    public OpenCashShiftRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.CounterId).GreaterThan(0);
        RuleFor(x => x.TerminalId).GreaterThan(0);
        RuleFor(x => x.OpeningAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Remarks).MaximumLength(500).When(x => x.Remarks is not null);
    }
}

public sealed class CashDrawerMovementRequestValidator : AbstractValidator<CashDrawerMovementRequest>
{
    public CashDrawerMovementRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Remarks).MaximumLength(500).When(x => x.Remarks is not null);
        RuleFor(x => x.ReferenceType).MaximumLength(100).When(x => x.ReferenceType is not null);
    }
}

public sealed class CloseCashShiftRequestValidator : AbstractValidator<CloseCashShiftRequest>
{
    public CloseCashShiftRequestValidator()
    {
        RuleFor(x => x.ClosingAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Remarks).MaximumLength(500).When(x => x.Remarks is not null);
    }
}
