using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class SyncItem
{
    public long Id { get; set; }

    public long SyncBatchId { get; set; }

    public string EntityName { get; set; } = null!;

    public long EntityId { get; set; }

    public string Operation { get; set; } = null!;

    public string? Payload { get; set; }

    public long Version { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string Status { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public virtual SyncBatch SyncBatch { get; set; } = null!;
}
