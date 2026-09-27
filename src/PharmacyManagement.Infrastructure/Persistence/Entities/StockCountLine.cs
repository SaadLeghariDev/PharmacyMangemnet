using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class StockCountLine
{
    public long Id { get; set; }

    public long StockCountId { get; set; }

    public long ProductId { get; set; }

    public long BatchId { get; set; }

    public long WarehouseLocationId { get; set; }

    public decimal SystemQuantity { get; set; }

    public decimal CountedQuantity { get; set; }

    public decimal VarianceQuantity { get; set; }

    public long ReasonCodeId { get; set; }

    public virtual InventoryBatch Batch { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ReasonCode ReasonCode { get; set; } = null!;

    public virtual StockCount StockCount { get; set; } = null!;

    public virtual WarehouseLocation WarehouseLocation { get; set; } = null!;
}
