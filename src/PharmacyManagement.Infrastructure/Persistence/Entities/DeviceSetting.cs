using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class DeviceSetting
{
    public long Id { get; set; }

    public long DeviceId { get; set; }

    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public bool IsEncrypted { get; set; }

    public virtual Device Device { get; set; } = null!;
}
