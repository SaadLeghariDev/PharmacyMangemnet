using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class SaleReturnLine
{
    public long Id { get; set; }

    public long SaleReturnId { get; set; }

    public long SaleLineId { get; set; }

    public long ProductId { get; set; }

    public long BatchId { get; set; }

    public long ProductUnitId { get; set; }

    public decimal Quantity { get; set; }

    public decimal RefundPrice { get; set; }

    public string? Condition { get; set; }

    public bool ReturnToStock { get; set; }

    public long? DestinationLocationId { get; set; }

    public virtual InventoryBatch Batch { get; set; } = null!;

    public virtual WarehouseLocation? DestinationLocation { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductUnit ProductUnit { get; set; } = null!;

    public virtual SaleLine SaleLine { get; set; } = null!;

    public virtual SaleReturn SaleReturn { get; set; } = null!;
}
