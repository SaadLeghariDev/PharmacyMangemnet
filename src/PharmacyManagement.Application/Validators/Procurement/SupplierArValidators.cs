using FluentValidation;
using PharmacyManagement.Application.DTOs.Procurement;

namespace PharmacyManagement.Application.Validators.Procurement;

public sealed class CreateSupplierPaymentRequestValidator : AbstractValidator<CreateSupplierPaymentRequest>
{
    public CreateSupplierPaymentRequestValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0).WithMessage("Supplier is required.");
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage("Branch is required.");
        RuleFor(x => x.PaymentMethodId).GreaterThan(0).WithMessage("Payment method is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.PaymentDate).NotEmpty().WithMessage("Payment date is required.");
        RuleFor(x => x.ReferenceNumber).MaximumLength(100).When(x => x.ReferenceNumber is not null);
        RuleFor(x => x.Remarks).MaximumLength(500).When(x => x.Remarks is not null);
        RuleFor(x => x.TerminalId).GreaterThan(0).When(x => x.TerminalId.HasValue);
    }
}

public sealed class CreateSupplierReturnLineRequestValidator : AbstractValidator<CreateSupplierReturnLineRequest>
{
    public CreateSupplierReturnLineRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product is required.");
        RuleFor(x => x.BatchId).GreaterThan(0).WithMessage("Batch is required.");
        RuleFor(x => x.ProductUnitId).GreaterThan(0).WithMessage("Product unit is required.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0).WithMessage("Unit cost cannot be negative.");
    }
}

public sealed class CreateSupplierReturnRequestValidator : AbstractValidator<CreateSupplierReturnRequest>
{
    public CreateSupplierReturnRequestValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0).WithMessage("Supplier is required.");
        RuleFor(x => x.BranchId).GreaterThan(0).WithMessage("Branch is required.");
        RuleFor(x => x.WarehouseId).GreaterThan(0).WithMessage("Warehouse is required.");
        RuleFor(x => x.Reason).MaximumLength(500).When(x => x.Reason is not null);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one return line is required.");
        RuleForEach(x => x.Lines).SetValidator(new CreateSupplierReturnLineRequestValidator());
    }
}
