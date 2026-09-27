using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class HeldSale
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long TerminalId { get; set; }

    public long UserId { get; set; }

    public long? CustomerId { get; set; }

    public string CartData { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public DateTime HeldAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual Branch Branch { get; set; } = null!;

    public virtual Customer? Customer { get; set; }

    public virtual Posterminal Terminal { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
