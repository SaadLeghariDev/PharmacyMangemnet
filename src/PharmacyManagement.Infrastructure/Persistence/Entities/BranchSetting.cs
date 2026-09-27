using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class BranchSetting
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public bool IsEncrypted { get; set; }

    public virtual Branch Branch { get; set; } = null!;
}
