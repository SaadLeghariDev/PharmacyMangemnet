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
    public const string PosSale = "POS.SALE";
    public const string PosHold = "POS.HOLD";
    public const string PosReturn = "POS.RETURN";
    public const string PosVoid = "POS.VOID";
    public const string FinCash = "FIN.CASH";
    public const string CustView = "CUST.VIEW";
    public const string CustEdit = "CUST.EDIT";
    public const string RxDispense = "RX.DISPENSE";
    public const string CtrlManage = "CTRL.MANAGE";
    public const string FiscalSubmit = "FISCAL.SUBMIT";
}

public static class DocumentTypes
{
    public const string PurchaseOrder = "PO";
    public const string GoodsReceipt = "GRN";
    public const string StockTransfer = "TRANSFER";
    public const string StockAdjustment = "ADJ";
    public const string StockCount = "COUNT";
    public const string Sale = "SALE";
    public const string SaleReturn = "RETURN";
    public const string Customer = "CUSTOMER";
    public const string Prescription = "PRESCRIPTION";
    public const string ControlledRegister = "CTRL_REGISTER";
}

public static class MovementTypes
{
    public const string GoodsReceipt = "GoodsReceipt";
    public const string TransferOut = "TransferOut";
    public const string TransferIn = "TransferIn";
    public const string Adjustment = "Adjustment";
    public const string StockCount = "StockCount";
    public const string Sale = "Sale";
    public const string Return = "Return";
    public const string Void = "Void";
}

public static class BatchStatuses
{
    public const string Available = "Available";
    public const string Quarantine = "Quarantine";
}

public static class AppClaimTypes
{
    public const string UserId = "uid";
    public const string TenantId = "tid";
    public const string Permission = "perm";
    public const string BranchId = "bid";
}
