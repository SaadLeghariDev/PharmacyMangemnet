using FluentValidation;
using PharmacyManagement.Application.DTOs.Inventory;

namespace PharmacyManagement.Application.Validators.Inventory;

public sealed class CreateStockTransferRequestValidator : AbstractValidator<CreateStockTransferRequest>
{
    public CreateStockTransferRequestValidator()
    {
        RuleFor(x => x.FromWarehouseId).GreaterThan(0);
        RuleFor(x => x.ToWarehouseId).GreaterThan(0)
            .Must((req, to) => to != req.FromWarehouseId)
            .WithMessage("From and to warehouses must differ.");
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).GreaterThan(0);
            line.RuleFor(l => l.BatchId).GreaterThan(0);
            line.RuleFor(l => l.ProductUnitId).GreaterThan(0);
            line.RuleFor(l => l.FromLocationId).GreaterThan(0);
            line.RuleFor(l => l.ToLocationId).GreaterThan(0);
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}

public sealed class CreateStockAdjustmentRequestValidator : AbstractValidator<CreateStockAdjustmentRequest>
{
    private static readonly HashSet<string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Increase", "Decrease", "WriteOff", "Damage", "Expiry" };

    public CreateStockAdjustmentRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.ReasonCodeId).GreaterThan(0);
        RuleFor(x => x.AdjustmentType).NotEmpty()
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage("AdjustmentType must be Increase, Decrease, WriteOff, Damage, or Expiry.");
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).GreaterThan(0);
            line.RuleFor(l => l.BatchId).GreaterThan(0);
            line.RuleFor(l => l.WarehouseLocationId).GreaterThan(0);
            line.RuleFor(l => l.Quantity).NotEqual(0);
            line.RuleFor(l => l.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class CreateStockCountRequestValidator : AbstractValidator<CreateStockCountRequest>
{
    public CreateStockCountRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).GreaterThan(0);
            line.RuleFor(l => l.BatchId).GreaterThan(0);
            line.RuleFor(l => l.WarehouseLocationId).GreaterThan(0);
            line.RuleFor(l => l.CountedQuantity).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.ReasonCodeId).GreaterThan(0);
        });
    }
}

public sealed class FefoQueryValidator : AbstractValidator<FefoQuery>
{
    public FefoQueryValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.RequiredQuantity).GreaterThan(0).When(x => x.RequiredQuantity.HasValue);
    }
}
