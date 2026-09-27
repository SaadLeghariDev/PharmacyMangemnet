using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class CashShift
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long CounterId { get; set; }

    public long TerminalId { get; set; }

    public long UserId { get; set; }

    public decimal OpeningAmount { get; set; }

    public DateTime OpeningAt { get; set; }

    public decimal? ClosingAmount { get; set; }

    public decimal? ExpectedAmount { get; set; }

    public decimal? VarianceAmount { get; set; }

    public DateTime? ClosingAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<CashTransaction> CashTransactions { get; set; } = new List<CashTransaction>();

    public virtual Counter Counter { get; set; } = null!;

    public virtual Posterminal Terminal { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
