using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Procurement;

public sealed class SupplierPaymentQuery : PaginationQuery
{
    public long? SupplierId { get; set; }
    public long? BranchId { get; set; }
    public long? PaymentMethodId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public sealed class SupplierPaymentDto
{
    public long Id { get; set; }
    public long SupplierId { get; set; }
    public string? SupplierCode { get; set; }
    public string? SupplierName { get; set; }
    public long BranchId { get; set; }
    public string? BranchName { get; set; }
    public long PaymentMethodId { get; set; }
    public string? PaymentMethodCode { get; set; }
    public string? PaymentMethodName { get; set; }
    public string? PaymentMethodType { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Remarks { get; set; }
    public long? PaidBy { get; set; }
}

public sealed class CreateSupplierPaymentRequest
{
    public long SupplierId { get; set; }
    public long BranchId { get; set; }
    public long PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Remarks { get; set; }
    /// <summary>Required when payment method is Cash — open cash shift is posted for this terminal.</summary>
    public long? TerminalId { get; set; }
}

public sealed class SupplierReturnQuery : PaginationQuery
{
    public long? SupplierId { get; set; }
    public long? BranchId { get; set; }
    public long? WarehouseId { get; set; }
    public string? Status { get; set; }
}

public sealed class SupplierReturnLineDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public long BatchId { get; set; }
    public long? GoodsReceiptLineId { get; set; }
    public long ProductUnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineAmount { get; set; }
}

public sealed class SupplierReturnDto
{
    public long Id { get; set; }
    public long SupplierId { get; set; }
    public string? SupplierCode { get; set; }
    public string? SupplierName { get; set; }
    public long BranchId { get; set; }
    public string? BranchName { get; set; }
    public long WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public IReadOnlyList<SupplierReturnLineDto> Lines { get; set; } = Array.Empty<SupplierReturnLineDto>();
}

public sealed class CreateSupplierReturnLineRequest
{
    public long ProductId { get; set; }
    public long BatchId { get; set; }
    public long ProductUnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public long? GoodsReceiptLineId { get; set; }
    /// <summary>Optional; when omitted, stock is taken from an available location of the batch in the return warehouse.</summary>
    public long? WarehouseLocationId { get; set; }
}

public sealed class CreateSupplierReturnRequest
{
    public long SupplierId { get; set; }
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? Reason { get; set; }
    public List<CreateSupplierReturnLineRequest> Lines { get; set; } = [];
}

public sealed class SupplierLedgerQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public sealed class SupplierLedgerEntryDto
{
    public long Id { get; set; }
    public long SupplierId { get; set; }
    public long BranchId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public long SequenceNo { get; set; }
    public string? Remarks { get; set; }
    /// <summary>Running balance (Debit − Credit) through this sequence for the supplier (optionally branch-filtered).</summary>
    public decimal RunningBalance { get; set; }
}
