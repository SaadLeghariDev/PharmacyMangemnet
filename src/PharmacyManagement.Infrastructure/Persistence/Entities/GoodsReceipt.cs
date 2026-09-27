using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class GoodsReceipt
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long WarehouseId { get; set; }

    public long SupplierId { get; set; }

    public long? PurchaseOrderId { get; set; }

    public string Grnnumber { get; set; } = null!;

    public DateTime ReceiptDate { get; set; }

    public string Status { get; set; } = null!;

    public string? InvoiceNumber { get; set; }

    public DateOnly? InvoiceDate { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal NetAmount { get; set; }

    public long? ReceivedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<GoodsReceiptLine> GoodsReceiptLines { get; set; } = new List<GoodsReceiptLine>();

    public virtual PurchaseOrder? PurchaseOrder { get; set; }

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
