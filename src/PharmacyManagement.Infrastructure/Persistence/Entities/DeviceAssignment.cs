using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class DeviceAssignment
{
    public long Id { get; set; }

    public long DeviceId { get; set; }

    public long TerminalId { get; set; }

    public DateTime AssignedFrom { get; set; }

    public DateTime? AssignedTo { get; set; }

    public bool IsActive { get; set; }

    public virtual Device Device { get; set; } = null!;

    public virtual Posterminal Terminal { get; set; } = null!;
}
