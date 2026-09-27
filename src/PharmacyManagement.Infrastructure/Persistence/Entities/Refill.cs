using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Refill
{
    public long Id { get; set; }

    public long PrescriptionItemId { get; set; }

    public DateTime RefillDate { get; set; }

    public decimal Quantity { get; set; }

    public long SaleId { get; set; }

    public long? DispensedBy { get; set; }

    public string? Notes { get; set; }

    public virtual PrescriptionItem PrescriptionItem { get; set; } = null!;

    public virtual Sale Sale { get; set; } = null!;
}
