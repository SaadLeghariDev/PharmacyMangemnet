using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class SupplierReturnLine
{
    public long Id { get; set; }

    public long SupplierReturnId { get; set; }

    public long ProductId { get; set; }

    public long BatchId { get; set; }

    public long? GoodsReceiptLineId { get; set; }

    public long ProductUnitId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public virtual InventoryBatch Batch { get; set; } = null!;

    public virtual GoodsReceiptLine? GoodsReceiptLine { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductUnit ProductUnit { get; set; } = null!;

    public virtual SupplierReturn SupplierReturn { get; set; } = null!;
}
