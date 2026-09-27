using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class StockAdjustmentLine
{
    public long Id { get; set; }

    public long StockAdjustmentId { get; set; }

    public long ProductId { get; set; }

    public long BatchId { get; set; }

    public long WarehouseLocationId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public virtual InventoryBatch Batch { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual StockAdjustment StockAdjustment { get; set; } = null!;

    public virtual WarehouseLocation WarehouseLocation { get; set; } = null!;
}
