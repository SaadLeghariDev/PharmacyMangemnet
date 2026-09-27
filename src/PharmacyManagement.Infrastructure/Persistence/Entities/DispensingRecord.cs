using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class DispensingRecord
{
    public long Id { get; set; }

    public long PrescriptionId { get; set; }

    public long SaleId { get; set; }

    public long CustomerId { get; set; }

    public long? DispensedBy { get; set; }

    public DateTime DispensedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<DispensingItem> DispensingItems { get; set; } = new List<DispensingItem>();

    public virtual Prescription Prescription { get; set; } = null!;

    public virtual Sale Sale { get; set; } = null!;
}
