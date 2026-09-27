using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class JournalLine
{
    public long Id { get; set; }

    public long JournalEntryId { get; set; }

    public int LineNo { get; set; }

    public long AccountId { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public string? Description { get; set; }

    public virtual ChartOfAccount Account { get; set; } = null!;

    public virtual JournalEntry JournalEntry { get; set; } = null!;
}
