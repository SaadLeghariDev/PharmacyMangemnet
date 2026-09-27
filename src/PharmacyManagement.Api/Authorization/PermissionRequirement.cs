using Microsoft.AspNetCore.Authorization;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasClaim(AppClaimTypes.Permission, requirement.Permission))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public static class PermissionPolicies
{
    public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddAuthorization(options =>
        {
            void Add(string code) =>
                options.AddPolicy(code, p => p.Requirements.Add(new PermissionRequirement(code)));

            Add(PermissionCodes.OrgView);
            Add(PermissionCodes.OrgEdit);
            Add(PermissionCodes.SecUsers);
            Add(PermissionCodes.SecRoles);
            Add(PermissionCodes.ProdView);
            Add(PermissionCodes.ProdEdit);
            Add(PermissionCodes.InvView);
            Add(PermissionCodes.InvAdjust);
            Add(PermissionCodes.InvTransfer);
            Add(PermissionCodes.ProcPo);
            Add(PermissionCodes.ProcGrn);
        });
        return services;
    }
}
