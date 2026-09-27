using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class NotificationLog
{
    public long Id { get; set; }

    public long TemplateId { get; set; }

    public string Recipient { get; set; } = null!;

    public string Channel { get; set; } = null!;

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public string? ErrorMessage { get; set; }

    public virtual NotificationTemplate Template { get; set; } = null!;
}
