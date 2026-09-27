using FluentValidation;
using PharmacyManagement.Application.DTOs.Sync;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Application.Validators.Sync;

public sealed class RegisterSyncNodeRequestValidator : AbstractValidator<RegisterSyncNodeRequest>
{
    public RegisterSyncNodeRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.TerminalId).GreaterThan(0);
        RuleFor(x => x.NodeCode).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateSyncNodeRequestValidator : AbstractValidator<UpdateSyncNodeRequest>
{
    public UpdateSyncNodeRequestValidator()
    {
        // IsActive is boolean — nothing else to validate
    }
}

public sealed class CreateSyncBatchRequestValidator : AbstractValidator<CreateSyncBatchRequest>
{
    public CreateSyncBatchRequestValidator()
    {
        RuleFor(x => x.SyncNodeId).GreaterThan(0);
        RuleFor(x => x.BatchNumber).MaximumLength(100).When(x => x.BatchNumber is not null);
    }
}

public sealed class UpdateSyncBatchStatusRequestValidator : AbstractValidator<UpdateSyncBatchStatusRequest>
{
    public UpdateSyncBatchStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(SyncBatchStatuses.IsKnown)
            .WithMessage(
                $"Status must be one of: {SyncBatchStatuses.InProgress}, {SyncBatchStatuses.Completed}, {SyncBatchStatuses.Failed}, {SyncBatchStatuses.Partial}.");
    }
}

public sealed class CreateSyncItemRequestValidator : AbstractValidator<CreateSyncItemRequest>
{
    public CreateSyncItemRequestValidator()
    {
        RuleFor(x => x.EntityName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.EntityId).GreaterThan(0);
        RuleFor(x => x.Operation)
            .NotEmpty()
            .Must(SyncItemOperations.IsKnown)
            .WithMessage(
                $"Operation must be one of: {SyncItemOperations.Insert}, {SyncItemOperations.Update}, {SyncItemOperations.Delete}.");
        RuleFor(x => x.Version).GreaterThan(0);
    }
}

public sealed class CreateSyncItemsRequestValidator : AbstractValidator<CreateSyncItemsRequest>
{
    public CreateSyncItemsRequestValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new CreateSyncItemRequestValidator());
    }
}

public sealed class UpdateSyncItemStatusRequestValidator : AbstractValidator<UpdateSyncItemStatusRequest>
{
    public UpdateSyncItemStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(SyncItemStatuses.IsKnown)
            .WithMessage(
                $"Status must be one of: {SyncItemStatuses.Pending}, {SyncItemStatuses.Processed}, {SyncItemStatuses.Failed}, {SyncItemStatuses.Skipped}.");
        RuleFor(x => x.ErrorMessage).MaximumLength(2000);
    }
}

public sealed class SyncPushRequestValidator : AbstractValidator<SyncPushRequest>
{
    public SyncPushRequestValidator()
    {
        RuleFor(x => x.SyncNodeId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new CreateSyncItemRequestValidator());
    }
}

public sealed class SyncPullRequestValidator : AbstractValidator<SyncPullRequest>
{
    public SyncPullRequestValidator()
    {
        RuleFor(x => x.SyncNodeId).GreaterThan(0);
        RuleFor(x => x.Limit).InclusiveBetween(1, 500);
    }
}
