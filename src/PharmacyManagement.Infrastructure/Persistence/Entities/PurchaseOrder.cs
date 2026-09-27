using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class PurchaseOrder
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long WarehouseId { get; set; }

    public long SupplierId { get; set; }

    public string Ponumber { get; set; } = null!;

    public DateTime Podate { get; set; }

    public DateTime? ExpectedDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public long? CreatedBy { get; set; }

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();

    public virtual ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = new List<PurchaseOrderLine>();

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
