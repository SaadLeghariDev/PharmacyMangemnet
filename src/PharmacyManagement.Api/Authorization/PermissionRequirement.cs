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
            Add(PermissionCodes.InvReorder);
            Add(PermissionCodes.AlertView);
            Add(PermissionCodes.AlertManage);
            Add(PermissionCodes.ProcPo);
            Add(PermissionCodes.ProcGrn);
            Add(PermissionCodes.ProcSupplierPay);
            Add(PermissionCodes.ProcSupplierReturn);
            Add(PermissionCodes.PosSale);
            Add(PermissionCodes.PosHold);
            Add(PermissionCodes.PosReturn);
            Add(PermissionCodes.PosVoid);
            Add(PermissionCodes.FinCash);
            Add(PermissionCodes.FinExpense);
            Add(PermissionCodes.PriceView);
            Add(PermissionCodes.PriceEdit);
            Add(PermissionCodes.TaxView);
            Add(PermissionCodes.TaxEdit);
            Add(PermissionCodes.CustView);
            Add(PermissionCodes.CustEdit);
            Add(PermissionCodes.RxDispense);
            Add(PermissionCodes.CtrlManage);
            Add(PermissionCodes.FiscalSubmit);
            Add(PermissionCodes.HwView);
            Add(PermissionCodes.HwManage);
            Add(PermissionCodes.PrintManage);
        });
        return services;
    }
}
