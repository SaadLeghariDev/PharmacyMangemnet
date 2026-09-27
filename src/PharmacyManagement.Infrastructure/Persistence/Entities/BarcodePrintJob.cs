using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class BarcodePrintJob
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long PrinterDeviceId { get; set; }

    public long ProductId { get; set; }

    public long? BatchId { get; set; }

    public int Quantity { get; set; }

    public long TemplateId { get; set; }

    public string Status { get; set; } = null!;

    public long? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PrintedAt { get; set; }

    public virtual InventoryBatch? Batch { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Device PrinterDevice { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual PrintTemplate Template { get; set; } = null!;
}
