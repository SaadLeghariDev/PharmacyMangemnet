using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class TenantSetting
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public bool IsEncrypted { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
