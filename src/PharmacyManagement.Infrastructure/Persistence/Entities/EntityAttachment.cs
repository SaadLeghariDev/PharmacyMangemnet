using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class EntityAttachment
{
    public long Id { get; set; }

    public long AttachmentId { get; set; }

    public string EntityName { get; set; } = null!;

    public long EntityId { get; set; }

    public virtual Attachment Attachment { get; set; } = null!;
}
