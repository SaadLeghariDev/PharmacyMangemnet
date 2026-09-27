using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Attachment
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public string FileName { get; set; } = null!;

    public string StoragePath { get; set; } = null!;

    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    public string? Hash { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<EntityAttachment> EntityAttachments { get; set; } = new List<EntityAttachment>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual Tenant Tenant { get; set; } = null!;
}
