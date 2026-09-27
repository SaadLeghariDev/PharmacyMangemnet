using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class AlertRule
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public long? BranchId { get; set; }

    public string AlertType { get; set; } = null!;

    public decimal? Threshold { get; set; }

    public int? DaysBeforeExpiry { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public virtual Branch? Branch { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
