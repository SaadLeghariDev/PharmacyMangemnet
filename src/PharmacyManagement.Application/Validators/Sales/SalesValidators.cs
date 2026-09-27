using FluentValidation;
using PharmacyManagement.Application.DTOs.Sales;

namespace PharmacyManagement.Application.Validators.Sales;

public sealed class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    private static readonly string[] SaleTypes = ["Retail", "Credit", "Wholesale"];

    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.CounterId).GreaterThan(0);
        RuleFor(x => x.TerminalId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.SaleType).Must(t => SaleTypes.Contains(t))
            .WithMessage("SaleType must be Retail, Credit, or Wholesale.");
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.Lines).NotEmpty();
        RuleFor(x => x.CustomerId).NotNull().When(x => x.SaleType == "Credit")
            .WithMessage("CustomerId is required for credit sales.");
        RuleFor(x => x.IdempotencyKey).MaximumLength(150).When(x => x.IdempotencyKey is not null);
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).GreaterThan(0);
            line.RuleFor(l => l.ProductUnitId).GreaterThan(0);
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice!).GreaterThanOrEqualTo(0).When(l => l.UnitPrice.HasValue);
            line.RuleFor(l => l.DiscountAmount).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.TaxAmount).GreaterThanOrEqualTo(0);
            line.RuleForEach(l => l.ManualBatches!).ChildRules(batch =>
            {
                batch.RuleFor(b => b.BatchId).GreaterThan(0);
                batch.RuleFor(b => b.WarehouseLocationId).GreaterThan(0);
                batch.RuleFor(b => b.Quantity).GreaterThan(0);
            }).When(l => l.ManualBatches is { Count: > 0 });
        });
        RuleForEach(x => x.Payments).ChildRules(pay =>
        {
            pay.RuleFor(p => p.PaymentMethodId).GreaterThan(0);
            pay.RuleFor(p => p.Amount).GreaterThan(0);
            pay.RuleFor(p => p.ReferenceNumber).MaximumLength(150).When(p => p.ReferenceNumber is not null);
        });
    }
}

public sealed class RecordSalePaymentRequestValidator : AbstractValidator<RecordSalePaymentRequest>
{
    public RecordSalePaymentRequestValidator()
    {
        RuleFor(x => x.PaymentMethodId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ReferenceNumber).MaximumLength(150).When(x => x.ReferenceNumber is not null);
    }
}

public sealed class HoldSaleRequestValidator : AbstractValidator<HoldSaleRequest>
{
    public HoldSaleRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.TerminalId).GreaterThan(0);
        RuleFor(x => x.CartData).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateSaleReturnRequestValidator : AbstractValidator<CreateSaleReturnRequest>
{
    private static readonly string[] Conditions = ["Good", "Damaged", "Expired", "NonResalable"];

    public CreateSaleReturnRequestValidator()
    {
        RuleFor(x => x.SaleId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500).When(x => x.Reason is not null);
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.SaleLineId).GreaterThan(0);
            line.RuleFor(l => l.BatchId).GreaterThan(0);
            line.RuleFor(l => l.ProductUnitId).GreaterThan(0);
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.RefundPrice!).GreaterThanOrEqualTo(0).When(l => l.RefundPrice.HasValue);
            line.RuleFor(l => l.Condition).Must(c => Conditions.Contains(c))
                .WithMessage("Condition must be Good, Damaged, Expired, or NonResalable.");
        });
    }
}
