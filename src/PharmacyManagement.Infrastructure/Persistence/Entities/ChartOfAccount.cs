using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class ChartOfAccount
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public long? ParentAccountId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public long AccountTypeId { get; set; }

    public bool IsSystemAccount { get; set; }

    public bool IsActive { get; set; }

    public virtual AccountType AccountType { get; set; } = null!;

    public virtual ICollection<ChartOfAccount> InverseParentAccount { get; set; } = new List<ChartOfAccount>();

    public virtual ICollection<JournalLine> JournalLines { get; set; } = new List<JournalLine>();

    public virtual ChartOfAccount? ParentAccount { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
