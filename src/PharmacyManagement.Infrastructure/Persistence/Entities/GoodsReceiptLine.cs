using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class GoodsReceiptLine
{
    public long Id { get; set; }

    public long GoodsReceiptId { get; set; }

    public long ProductId { get; set; }

    public long ProductUnitId { get; set; }

    public decimal OrderedQuantity { get; set; }

    public decimal ReceivedQuantity { get; set; }

    public decimal FreeQuantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal NetCost { get; set; }

    public virtual GoodsReceipt GoodsReceipt { get; set; } = null!;

    public virtual ICollection<GoodsReceiptLineBatch> GoodsReceiptLineBatches { get; set; } = new List<GoodsReceiptLineBatch>();

    public virtual ICollection<InventoryBatch> InventoryBatches { get; set; } = new List<InventoryBatch>();

    public virtual Product Product { get; set; } = null!;

    public virtual ProductUnit ProductUnit { get; set; } = null!;

    public virtual ICollection<SupplierReturnLine> SupplierReturnLines { get; set; } = new List<SupplierReturnLine>();
}
