using FluentValidation;
using PharmacyManagement.Application.DTOs.Expenses;

namespace PharmacyManagement.Application.Validators.Expenses;

public sealed class CreateExpenseCategoryRequestValidator : AbstractValidator<CreateExpenseCategoryRequest>
{
    public CreateExpenseCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Category name is required.").MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().WithMessage("Category code is required.").MaximumLength(50);
    }
}

public sealed class UpdateExpenseCategoryRequestValidator : AbstractValidator<UpdateExpenseCategoryRequest>
{
    public UpdateExpenseCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Category name is required.").MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().WithMessage("Category code is required.").MaximumLength(50);
    }
}

public sealed class CreateExpenseRequestValidator : AbstractValidator<CreateExpenseRequest>
{
    public CreateExpenseRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage("Branch is required.");
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Expense category is required.");
        RuleFor(x => x.PaymentMethodId).GreaterThan(0).WithMessage("Payment method is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.ExpenseDate).NotEmpty().WithMessage("Expense date is required.");
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.TerminalId).GreaterThan(0).When(x => x.TerminalId.HasValue);
    }
}

public sealed class UpdateExpenseRequestValidator : AbstractValidator<UpdateExpenseRequest>
{
    public UpdateExpenseRequestValidator()
    {
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}
