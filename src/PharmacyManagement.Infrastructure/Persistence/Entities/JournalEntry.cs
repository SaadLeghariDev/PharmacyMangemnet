using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class JournalEntry
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public long BranchId { get; set; }

    public string EntryNumber { get; set; } = null!;

    public DateTime EntryDate { get; set; }

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public long? PostedBy { get; set; }

    public DateTime? PostedAt { get; set; }

    public long? ReversalOfEntryId { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<JournalEntry> InverseReversalOfEntry { get; set; } = new List<JournalEntry>();

    public virtual ICollection<JournalLine> JournalLines { get; set; } = new List<JournalLine>();

    public virtual JournalEntry? ReversalOfEntry { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
