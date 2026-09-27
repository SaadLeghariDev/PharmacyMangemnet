using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class DispensingItem
{
    public long Id { get; set; }

    public long DispensingRecordId { get; set; }

    public long PrescriptionItemId { get; set; }

    public long ProductId { get; set; }

    public long BatchId { get; set; }

    public decimal Quantity { get; set; }

    public virtual InventoryBatch Batch { get; set; } = null!;

    public virtual DispensingRecord DispensingRecord { get; set; } = null!;

    public virtual PrescriptionItem PrescriptionItem { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
