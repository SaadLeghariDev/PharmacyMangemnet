using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class InventoryBatchLocation
{
    public long Id { get; set; }

    public long BatchId { get; set; }

    public long WarehouseLocationId { get; set; }

    public decimal QuantityOnHand { get; set; }

    public decimal ReservedQuantity { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual InventoryBatch Batch { get; set; } = null!;

    public virtual WarehouseLocation WarehouseLocation { get; set; } = null!;
}
