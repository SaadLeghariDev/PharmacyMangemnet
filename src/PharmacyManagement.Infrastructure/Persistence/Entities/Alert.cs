using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Alert
{
    public long Id { get; set; }

    public long AlertRuleId { get; set; }

    public long BranchId { get; set; }

    public long? ProductId { get; set; }

    public long? BatchId { get; set; }

    public string Severity { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public long? ResolvedBy { get; set; }

    public virtual AlertRule AlertRule { get; set; } = null!;

    public virtual InventoryBatch? Batch { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Product? Product { get; set; }
}
