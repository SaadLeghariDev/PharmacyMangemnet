using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Purchasing;

public sealed class SupplierDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Ntn { get; set; }
    public string? Strn { get; set; }
    public string? DrugLicenseNo { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateSupplierRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Ntn { get; set; }
    public string? Strn { get; set; }
    public string? DrugLicenseNo { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; }
}

public sealed class UpdateSupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Ntn { get; set; }
    public string? Strn { get; set; }
    public string? DrugLicenseNo { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class PurchaseOrderLineDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public long ProductUnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal FreeQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
}

public sealed class PurchaseOrderDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public long SupplierId { get; set; }
    public string PoNumber { get; set; } = string.Empty;
    public DateTime PoDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public long? CreatedBy { get; set; }
    public long? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public IReadOnlyList<PurchaseOrderLineDto> Lines { get; set; } = Array.Empty<PurchaseOrderLineDto>();
}

public sealed class PurchaseOrderLineRequest
{
    public long ProductId { get; set; }
    public long ProductUnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal FreeQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
}

public sealed class CreatePurchaseOrderRequest
{
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public long SupplierId { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public string? Remarks { get; set; }
    public List<PurchaseOrderLineRequest> Lines { get; set; } = new();
}

public sealed class UpdatePurchaseOrderRequest
{
    public DateTime? ExpectedDate { get; set; }
    public string? Remarks { get; set; }
    public List<PurchaseOrderLineRequest> Lines { get; set; } = new();
}

public sealed class PurchaseOrderQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public long? SupplierId { get; set; }
    public string? Status { get; set; }
}

public sealed class GoodsReceiptLineBatchDto
{
    public long Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly? ManufacturingDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public decimal Mrp { get; set; }
    public decimal SalePrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal FreeQuantity { get; set; }
    public long WarehouseLocationId { get; set; }
}

public sealed class GoodsReceiptLineDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public long ProductUnitId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal FreeQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetCost { get; set; }
    public IReadOnlyList<GoodsReceiptLineBatchDto> Batches { get; set; } = Array.Empty<GoodsReceiptLineBatchDto>();
}

public sealed class GoodsReceiptDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public long SupplierId { get; set; }
    public long? PurchaseOrderId { get; set; }
    public string GrnNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public long? ReceivedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<GoodsReceiptLineDto> Lines { get; set; } = Array.Empty<GoodsReceiptLineDto>();
}

public sealed class GoodsReceiptLineBatchRequest
{
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly? ManufacturingDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public decimal Mrp { get; set; }
    public decimal SalePrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal FreeQuantity { get; set; }
    public long WarehouseLocationId { get; set; }
}

public sealed class GoodsReceiptLineRequest
{
    public long ProductId { get; set; }
    public long ProductUnitId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal FreeQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public List<GoodsReceiptLineBatchRequest> Batches { get; set; } = new();
}

public sealed class CreateGoodsReceiptRequest
{
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public long SupplierId { get; set; }
    public long? PurchaseOrderId { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public List<GoodsReceiptLineRequest> Lines { get; set; } = new();
}

public sealed class GoodsReceiptQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public long? SupplierId { get; set; }
    public string? Status { get; set; }
}
