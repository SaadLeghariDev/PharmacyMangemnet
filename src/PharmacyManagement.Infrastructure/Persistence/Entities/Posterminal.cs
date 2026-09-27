using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Posterminal
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long CounterId { get; set; }

    public string TerminalCode { get; set; } = null!;

    public string? ComputerName { get; set; }

    public string? MacAddress { get; set; }

    public string? Ipaddress { get; set; }

    public string? SerialNumber { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsOnline { get; set; }

    public DateTime? LastSyncAt { get; set; }

    public DateTime? LastHeartbeatAt { get; set; }

    public bool IsActive { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<CashShift> CashShifts { get; set; } = new List<CashShift>();

    public virtual Counter Counter { get; set; } = null!;

    public virtual ICollection<DeviceAssignment> DeviceAssignments { get; set; } = new List<DeviceAssignment>();

    public virtual ICollection<HeldSale> HeldSales { get; set; } = new List<HeldSale>();

    public virtual ICollection<IdempotencyKey> IdempotencyKeys { get; set; } = new List<IdempotencyKey>();

    public virtual ICollection<LoginSession> LoginSessions { get; set; } = new List<LoginSession>();

    public virtual ICollection<NumberSequence> NumberSequences { get; set; } = new List<NumberSequence>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<SyncNode> SyncNodes { get; set; } = new List<SyncNode>();
}
