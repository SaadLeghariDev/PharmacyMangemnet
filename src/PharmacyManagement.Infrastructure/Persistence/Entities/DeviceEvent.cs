using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class DeviceEvent
{
    public long Id { get; set; }

    public long DeviceId { get; set; }

    public string EventType { get; set; } = null!;

    public string? Payload { get; set; }

    public string Status { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Device Device { get; set; } = null!;
}
