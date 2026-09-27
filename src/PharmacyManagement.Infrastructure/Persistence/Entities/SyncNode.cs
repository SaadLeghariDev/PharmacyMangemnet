using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class SyncNode
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public long BranchId { get; set; }

    public long TerminalId { get; set; }

    public string NodeCode { get; set; } = null!;

    public DateTime? LastSyncAt { get; set; }

    public long LastSequence { get; set; }

    public bool IsActive { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<SyncBatch> SyncBatches { get; set; } = new List<SyncBatch>();

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual Posterminal Terminal { get; set; } = null!;
}
