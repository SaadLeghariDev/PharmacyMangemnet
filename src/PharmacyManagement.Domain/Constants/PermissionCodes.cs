namespace PharmacyManagement.Domain.Constants;

public static class PermissionCodes
{
    public const string OrgView = "ORG.VIEW";
    public const string OrgEdit = "ORG.EDIT";
    public const string SecUsers = "SEC.USERS";
    public const string SecRoles = "SEC.ROLES";
    public const string ProdView = "PROD.VIEW";
    public const string ProdEdit = "PROD.EDIT";
}

public static class AppClaimTypes
{
    public const string UserId = "uid";
    public const string TenantId = "tid";
    public const string Permission = "perm";
    public const string BranchId = "bid";
}
