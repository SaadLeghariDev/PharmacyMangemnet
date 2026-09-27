using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class SyncBatch
{
    public long Id { get; set; }

    public long SyncNodeId { get; set; }

    public string BatchNumber { get; set; } = null!;

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<SyncItem> SyncItems { get; set; } = new List<SyncItem>();

    public virtual SyncNode SyncNode { get; set; } = null!;
}
