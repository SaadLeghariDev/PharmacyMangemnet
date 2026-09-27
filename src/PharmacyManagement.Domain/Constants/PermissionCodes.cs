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
    public const string InvReorder = "INV.REORDER";
    public const string AlertView = "ALERT.VIEW";
    public const string AlertManage = "ALERT.MANAGE";
    public const string ProcPo = "PROC.PO";
    public const string ProcGrn = "PROC.GRN";
    public const string ProcSupplierPay = "PROC.SUPPLIER_PAY";
    public const string ProcSupplierReturn = "PROC.SUPPLIER_RETURN";
    public const string PosSale = "POS.SALE";
    public const string PosHold = "POS.HOLD";
    public const string PosReturn = "POS.RETURN";
    public const string PosVoid = "POS.VOID";
    public const string FinCash = "FIN.CASH";
    public const string FinExpense = "FIN.EXPENSE";
    public const string PriceView = "PRICE.VIEW";
    public const string PriceEdit = "PRICE.EDIT";
    public const string TaxView = "TAX.VIEW";
    public const string TaxEdit = "TAX.EDIT";
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
    public const string Expense = "EXPENSE";
    public const string SupplierReturn = "SUPPLIER_RETURN";
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
    /// <summary>No CHECK constraint on InventoryMovements.MovementType — SupplierReturn is allowed.</summary>
    public const string SupplierReturn = "SupplierReturn";
}

public static class BatchStatuses
{
    public const string Available = "Available";
    public const string Quarantine = "Quarantine";
}

public static class AlertTypes
{
    public const string LowStock = "LowStock";
    public const string Expiry = "Expiry";

    public static bool IsKnown(string? value) =>
        string.Equals(value, LowStock, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, Expiry, StringComparison.OrdinalIgnoreCase);

    public static bool IsExpiry(string? value) =>
        string.Equals(value, Expiry, StringComparison.OrdinalIgnoreCase);

    public static bool IsLowStock(string? value) =>
        string.Equals(value, LowStock, StringComparison.OrdinalIgnoreCase);

    public static string Normalize(string value) =>
        IsExpiry(value) ? Expiry : LowStock;
}

public static class AlertSeverities
{
    public const string Info = "Info";
    public const string Warning = "Warning";
    public const string Critical = "Critical";
}

public static class AlertStatuses
{
    public const string Open = "Open";
    public const string Acknowledged = "Acknowledged";
    public const string Resolved = "Resolved";
    public const string Dismissed = "Dismissed";
}

public static class NotificationLogStatuses
{
    public const string Pending = "Pending";
    public const string Sent = "Sent";
    public const string Failed = "Failed";
}

public static class AppClaimTypes
{
    public const string UserId = "uid";
    public const string TenantId = "tid";
    public const string Permission = "perm";
    public const string BranchId = "bid";
}
