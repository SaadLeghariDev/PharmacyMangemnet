using FluentValidation;
using PharmacyManagement.Application.DTOs.Alerts;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Application.Validators.Alerts;

public sealed class CreateReorderRuleRequestValidator : AbstractValidator<CreateReorderRuleRequest>
{
    public CreateReorderRuleRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.PreferredSupplierId).GreaterThan(0);
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaximumStock).GreaterThanOrEqualTo(x => x.MinimumStock)
            .WithMessage("MaximumStock must be greater than or equal to MinimumStock.");
        RuleFor(x => x.ReorderPoint).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReorderQuantity).GreaterThan(0);
    }
}

public sealed class UpdateReorderRuleRequestValidator : AbstractValidator<UpdateReorderRuleRequest>
{
    public UpdateReorderRuleRequestValidator()
    {
        RuleFor(x => x.PreferredSupplierId).GreaterThan(0);
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaximumStock).GreaterThanOrEqualTo(x => x.MinimumStock)
            .WithMessage("MaximumStock must be greater than or equal to MinimumStock.");
        RuleFor(x => x.ReorderPoint).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReorderQuantity).GreaterThan(0);
    }
}

public sealed class CreateAlertRuleRequestValidator : AbstractValidator<CreateAlertRuleRequest>
{
    public CreateAlertRuleRequestValidator()
    {
        RuleFor(x => x.AlertType)
            .Must(AlertTypes.IsKnown)
            .WithMessage($"AlertType must be {AlertTypes.LowStock} or {AlertTypes.Expiry}.");
        RuleFor(x => x.Threshold).GreaterThanOrEqualTo(0).When(x => x.Threshold.HasValue);
        RuleFor(x => x.DaysBeforeExpiry)
            .NotNull().When(x => AlertTypes.IsExpiry(x.AlertType))
            .WithMessage("DaysBeforeExpiry is required for Expiry rules.")
            .GreaterThanOrEqualTo(0).When(x => x.DaysBeforeExpiry.HasValue);
        RuleFor(x => x.BranchId).GreaterThan(0).When(x => x.BranchId.HasValue);
    }
}

public sealed class UpdateAlertRuleRequestValidator : AbstractValidator<UpdateAlertRuleRequest>
{
    public UpdateAlertRuleRequestValidator()
    {
        RuleFor(x => x.AlertType)
            .Must(AlertTypes.IsKnown)
            .WithMessage($"AlertType must be {AlertTypes.LowStock} or {AlertTypes.Expiry}.");
        RuleFor(x => x.Threshold).GreaterThanOrEqualTo(0).When(x => x.Threshold.HasValue);
        RuleFor(x => x.DaysBeforeExpiry)
            .NotNull().When(x => AlertTypes.IsExpiry(x.AlertType))
            .WithMessage("DaysBeforeExpiry is required for Expiry rules.")
            .GreaterThanOrEqualTo(0).When(x => x.DaysBeforeExpiry.HasValue);
        RuleFor(x => x.BranchId).GreaterThan(0).When(x => x.BranchId.HasValue);
    }
}

public sealed class CreateNotificationTemplateRequestValidator : AbstractValidator<CreateNotificationTemplateRequest>
{
    public CreateNotificationTemplateRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Channel).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Subject).MaximumLength(250).When(x => x.Subject is not null);
        RuleFor(x => x.Body).NotEmpty();
    }
}

public sealed class UpdateNotificationTemplateRequestValidator : AbstractValidator<UpdateNotificationTemplateRequest>
{
    public UpdateNotificationTemplateRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Channel).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Subject).MaximumLength(250).When(x => x.Subject is not null);
        RuleFor(x => x.Body).NotEmpty();
    }
}
