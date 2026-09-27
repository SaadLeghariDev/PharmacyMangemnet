using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Sales;

public sealed class SaleQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public long? TerminalId { get; set; }
    public long? CustomerId { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public sealed class SaleLineBatchDto
{
    public long Id { get; set; }
    public long BatchId { get; set; }
    public string? BatchNumber { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal BaseQuantity { get; set; }
    public decimal UnitCost { get; set; }
}

public sealed class SaleLineDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }
    public long ProductUnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal BaseQuantity { get; set; }
    public decimal ConversionFactor { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Mrp { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public IReadOnlyList<SaleLineBatchDto> Batches { get; set; } = Array.Empty<SaleLineBatchDto>();
}

public sealed class SalePaymentDto
{
    public long Id { get; set; }
    public long PaymentMethodId { get; set; }
    public string? PaymentMethodCode { get; set; }
    public string? PaymentMethodName { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public long? ParentPaymentId { get; set; }
}

public sealed class SaleDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public long CounterId { get; set; }
    public long TerminalId { get; set; }
    public long UserId { get; set; }
    public long? CustomerId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string SaleType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "PKR";
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal RoundOff { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string? FbrStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<SaleLineDto> Lines { get; set; } = Array.Empty<SaleLineDto>();
    public IReadOnlyList<SalePaymentDto> Payments { get; set; } = Array.Empty<SalePaymentDto>();
}

public sealed class SaleReceiptDto
{
    public SaleDto Sale { get; set; } = null!;
    public string? CustomerName { get; set; }
    public string? CustomerCode { get; set; }
    public string? BranchName { get; set; }
    public string? CounterCode { get; set; }
    public string? TerminalCode { get; set; }
    public string? CashierName { get; set; }
}

public sealed class ManualBatchAllocationRequest
{
    public long BatchId { get; set; }
    public long WarehouseLocationId { get; set; }
    public decimal Quantity { get; set; }
}

public sealed class CreateSaleLineRequest
{
    public long ProductId { get; set; }
    public long ProductUnitId { get; set; }
    public decimal Quantity { get; set; }
    /// <summary>Optional; when omitted server resolves from product/batch sale price.</summary>
    public decimal? UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    /// <summary>Optional authorized override of FEFO. Quantities in sale unit.</summary>
    public IReadOnlyList<ManualBatchAllocationRequest>? ManualBatches { get; set; }
}

public sealed class CreateSalePaymentRequest
{
    public long PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
}

public sealed class CreateSaleRequest
{
    public long BranchId { get; set; }
    public long CounterId { get; set; }
    public long TerminalId { get; set; }
    public long WarehouseId { get; set; }
    public long? CustomerId { get; set; }
    public string SaleType { get; set; } = "Retail";
    public string CurrencyCode { get; set; } = "PKR";
    public decimal RoundOff { get; set; }
    public string? IdempotencyKey { get; set; }
    public IReadOnlyList<CreateSaleLineRequest> Lines { get; set; } = Array.Empty<CreateSaleLineRequest>();
    public IReadOnlyList<CreateSalePaymentRequest> Payments { get; set; } = Array.Empty<CreateSalePaymentRequest>();
}

public sealed class RecordSalePaymentRequest
{
    public long PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
}

public sealed class HeldSaleQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public long? TerminalId { get; set; }
    public string? Status { get; set; }
}

public sealed class HeldSaleDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public long TerminalId { get; set; }
    public long UserId { get; set; }
    public long? CustomerId { get; set; }
    public string CartData { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime HeldAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class HoldSaleRequest
{
    public long BranchId { get; set; }
    public long TerminalId { get; set; }
    public long? CustomerId { get; set; }
    public string CartData { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public sealed class SaleReturnLineDto
{
    public long Id { get; set; }
    public long SaleLineId { get; set; }
    public long ProductId { get; set; }
    public long BatchId { get; set; }
    public long ProductUnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal RefundPrice { get; set; }
    public string? Condition { get; set; }
    public bool ReturnToStock { get; set; }
    public long? DestinationLocationId { get; set; }
}

public sealed class SaleReturnDto
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public long BranchId { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
    public long? RefundPaymentMethodId { get; set; }
    public long? CreatedBy { get; set; }
    public long? ApprovedBy { get; set; }
    public IReadOnlyList<SaleReturnLineDto> Lines { get; set; } = Array.Empty<SaleReturnLineDto>();
}

public sealed class CreateSaleReturnLineRequest
{
    public long SaleLineId { get; set; }
    public long BatchId { get; set; }
    public long ProductUnitId { get; set; }
    public decimal Quantity { get; set; }
    /// <summary>Optional; defaults to proportional unit net from original line.</summary>
    public decimal? RefundPrice { get; set; }
    /// <summary>Good | Damaged | Expired | NonResalable</summary>
    public string Condition { get; set; } = "Good";
    public long? DestinationLocationId { get; set; }
}

public sealed class CreateSaleReturnRequest
{
    public long SaleId { get; set; }
    public string? Reason { get; set; }
    public long? RefundPaymentMethodId { get; set; }
    /// <summary>When true (default), posts immediately: stock/quarantine + refund.</summary>
    public bool PostImmediately { get; set; } = true;
    public IReadOnlyList<CreateSaleReturnLineRequest> Lines { get; set; } = Array.Empty<CreateSaleReturnLineRequest>();
}

public sealed class SaleReturnQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public long? SaleId { get; set; }
    public string? Status { get; set; }
}
