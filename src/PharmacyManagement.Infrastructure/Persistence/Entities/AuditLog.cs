using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class AuditLog
{
    public long Id { get; set; }

    public long? TenantId { get; set; }

    public long? BranchId { get; set; }

    public long? UserId { get; set; }

    public string EntityName { get; set; } = null!;

    public long? EntityId { get; set; }

    public string Action { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? Ipaddress { get; set; }

    public long? TerminalId { get; set; }

    public DateTime CreatedAt { get; set; }
}
