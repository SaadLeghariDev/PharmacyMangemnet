using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Device
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long? CounterId { get; set; }

    public long DeviceTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public string? ConnectionType { get; set; }

    public string? Ip { get; set; }

    public int? Port { get; set; }

    public string? Comport { get; set; }

    public string? MacAddress { get; set; }

    public string? DriverName { get; set; }

    public string? DriverVersion { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public virtual ICollection<BarcodePrintJob> BarcodePrintJobs { get; set; } = new List<BarcodePrintJob>();

    public virtual Branch Branch { get; set; } = null!;

    public virtual Counter? Counter { get; set; }

    public virtual ICollection<DeviceAssignment> DeviceAssignments { get; set; } = new List<DeviceAssignment>();

    public virtual ICollection<DeviceEvent> DeviceEvents { get; set; } = new List<DeviceEvent>();

    public virtual ICollection<DeviceSetting> DeviceSettings { get; set; } = new List<DeviceSetting>();

    public virtual DeviceType DeviceType { get; set; } = null!;
}
