using FluentValidation;
using PharmacyManagement.Application.DTOs.Admin;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Application.Validators.Admin;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(200);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.EmployeeCode).MaximumLength(50);
    }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.EmployeeCode).MaximumLength(50);
        RuleFor(x => x.Password).MinimumLength(8).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Password));
    }
}

public sealed class AssignUserRolesRequestValidator : AbstractValidator<AssignUserRolesRequest>
{
    public AssignUserRolesRequestValidator()
    {
        RuleFor(x => x.RoleIds).NotNull();
    }
}

public sealed class AssignUserBranchesRequestValidator : AbstractValidator<AssignUserBranchesRequest>
{
    public AssignUserBranchesRequestValidator()
    {
        RuleFor(x => x.BranchIds).NotNull();
    }
}

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class AssignRolePermissionsRequestValidator : AbstractValidator<AssignRolePermissionsRequest>
{
    public AssignRolePermissionsRequestValidator()
    {
        RuleFor(x => x.PermissionIds).NotNull();
    }
}

public sealed class UpsertSettingRequestValidator : AbstractValidator<UpsertSettingRequest>
{
    public UpsertSettingRequestValidator()
    {
        RuleFor(x => x.SettingKey).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SettingValue).MaximumLength(4000);
    }
}

public sealed class CreateReasonCodeRequestValidator : AbstractValidator<CreateReasonCodeRequest>
{
    public CreateReasonCodeRequestValidator()
    {
        RuleFor(x => x.ReasonType)
            .Must(ReasonTypes.IsKnown)
            .WithMessage($"ReasonType must be {ReasonTypes.Return}, {ReasonTypes.Adjustment}, {ReasonTypes.Count}, {ReasonTypes.Void}, or {ReasonTypes.Other}.");
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UpdateReasonCodeRequestValidator : AbstractValidator<UpdateReasonCodeRequest>
{
    public UpdateReasonCodeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
