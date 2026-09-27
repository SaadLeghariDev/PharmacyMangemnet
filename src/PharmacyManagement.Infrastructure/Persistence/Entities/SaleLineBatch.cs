using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class SaleLineBatch
{
    public long Id { get; set; }

    public long SaleLineId { get; set; }

    public long BatchId { get; set; }

    public decimal Quantity { get; set; }

    public decimal BaseQuantity { get; set; }

    public decimal UnitCost { get; set; }

    public virtual InventoryBatch Batch { get; set; } = null!;

    public virtual SaleLine SaleLine { get; set; } = null!;
}
