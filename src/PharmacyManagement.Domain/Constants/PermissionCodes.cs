namespace PharmacyManagement.Domain.Constants;

public static class PermissionCodes
{
    public const string OrgView = "ORG.VIEW";
    public const string OrgEdit = "ORG.EDIT";
    public const string SecUsers = "SEC.USERS";
    public const string SecRoles = "SEC.ROLES";
    public const string ProdView = "PROD.VIEW";
    public const string ProdEdit = "PROD.EDIT";
    public const string InvView = "INV.VIEW";
    public const string InvAdjust = "INV.ADJUST";
    public const string InvTransfer = "INV.TRANSFER";
    public const string ProcPo = "PROC.PO";
    public const string ProcGrn = "PROC.GRN";
}

public static class DocumentTypes
{
    public const string PurchaseOrder = "PO";
    public const string GoodsReceipt = "GRN";
    public const string StockTransfer = "TRANSFER";
    public const string StockAdjustment = "ADJ";
    public const string StockCount = "COUNT";
}

public static class MovementTypes
{
    public const string GoodsReceipt = "GoodsReceipt";
    public const string TransferOut = "TransferOut";
    public const string TransferIn = "TransferIn";
    public const string Adjustment = "Adjustment";
    public const string StockCount = "StockCount";
}

public static class BatchStatuses
{
    public const string Available = "Available";
}

public static class AppClaimTypes
{
    public const string UserId = "uid";
    public const string TenantId = "tid";
    public const string Permission = "perm";
    public const string BranchId = "bid";
}
