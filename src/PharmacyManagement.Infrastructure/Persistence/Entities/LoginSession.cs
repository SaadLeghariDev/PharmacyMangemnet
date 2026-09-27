using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class LoginSession
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long TerminalId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public string? Ipaddress { get; set; }

    public string Status { get; set; } = null!;

    public virtual Posterminal Terminal { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
