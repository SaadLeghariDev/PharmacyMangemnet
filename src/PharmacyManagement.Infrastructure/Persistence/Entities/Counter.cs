using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Counter
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? CounterType { get; set; }

    public bool IsActive { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<CashShift> CashShifts { get; set; } = new List<CashShift>();

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    public virtual ICollection<Posterminal> Posterminals { get; set; } = new List<Posterminal>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
