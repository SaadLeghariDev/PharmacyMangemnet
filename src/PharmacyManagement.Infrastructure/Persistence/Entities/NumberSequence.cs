using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class NumberSequence
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public long? BranchId { get; set; }

    public long? TerminalId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string? Prefix { get; set; }

    public long CurrentNumber { get; set; }

    public int NumberLength { get; set; }

    public string? ResetPeriod { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual Posterminal? Terminal { get; set; }
}
