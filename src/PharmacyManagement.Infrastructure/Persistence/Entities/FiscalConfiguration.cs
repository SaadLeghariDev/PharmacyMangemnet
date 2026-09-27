using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class FiscalConfiguration
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public long? BranchId { get; set; }

    public string IntegrationType { get; set; } = null!;

    public string ProviderName { get; set; } = null!;

    public string? Ntn { get; set; }

    public string? Strn { get; set; }

    public string? Posid { get; set; }

    public string? ApiBaseUrl { get; set; }

    public string? CredentialsReference { get; set; }

    public string? Environment { get; set; }

    public bool IsEnabled { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
