using FluentValidation;
using PharmacyManagement.Application.DTOs.Finance;

namespace PharmacyManagement.Application.Validators.Finance;

public sealed class CreateChartOfAccountRequestValidator : AbstractValidator<CreateChartOfAccountRequest>
{
    public CreateChartOfAccountRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Account code is required.").MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Account name is required.").MaximumLength(150);
        RuleFor(x => x.AccountTypeId).GreaterThan(0).WithMessage("Account type is required.");
        RuleFor(x => x.ParentAccountId).GreaterThan(0).When(x => x.ParentAccountId.HasValue);
    }
}

public sealed class UpdateChartOfAccountRequestValidator : AbstractValidator<UpdateChartOfAccountRequest>
{
    public UpdateChartOfAccountRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Account name is required.").MaximumLength(150);
        RuleFor(x => x.AccountTypeId).GreaterThan(0).WithMessage("Account type is required.");
        RuleFor(x => x.ParentAccountId).GreaterThan(0).When(x => x.ParentAccountId.HasValue);
    }
}

public sealed class CreateJournalLineRequestValidator : AbstractValidator<CreateJournalLineRequest>
{
    public CreateJournalLineRequestValidator()
    {
        RuleFor(x => x.AccountId).GreaterThan(0).WithMessage("Account is required.");
        RuleFor(x => x.Debit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Credit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
        RuleFor(x => x)
            .Must(x => (x.Debit > 0 && x.Credit == 0) || (x.Credit > 0 && x.Debit == 0))
            .WithMessage("Each line must have either a debit or a credit (not both, not neither).");
    }
}

public sealed class CreateJournalEntryRequestValidator : AbstractValidator<CreateJournalEntryRequest>
{
    public CreateJournalEntryRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage("Branch is required.");
        RuleFor(x => x.EntryDate).NotEmpty().WithMessage("Entry date is required.");
        RuleFor(x => x.ReferenceType).MaximumLength(100).When(x => x.ReferenceType is not null);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least two journal lines are required.")
            .Must(lines => lines.Count >= 2).WithMessage("At least two journal lines are required.");
        RuleForEach(x => x.Lines).SetValidator(new CreateJournalLineRequestValidator());
    }
}
