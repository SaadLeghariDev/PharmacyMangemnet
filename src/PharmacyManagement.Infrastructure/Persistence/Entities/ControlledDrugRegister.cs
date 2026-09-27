using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class ControlledDrugRegister
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long ProductId { get; set; }

    public string RegisterNumber { get; set; } = null!;

    public decimal OpeningBalance { get; set; }

    public decimal CurrentBalance { get; set; }

    public bool IsActive { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<ControlledDrugTransaction> ControlledDrugTransactions { get; set; } = new List<ControlledDrugTransaction>();

    public virtual Product Product { get; set; } = null!;
}
