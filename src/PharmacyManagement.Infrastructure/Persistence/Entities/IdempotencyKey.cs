using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class IdempotencyKey
{
    public long Id { get; set; }

    public long TerminalId { get; set; }

    public string Key { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public long? EntityId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Posterminal Terminal { get; set; } = null!;
}
