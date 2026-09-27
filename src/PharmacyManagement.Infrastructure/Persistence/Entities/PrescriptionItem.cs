using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class PrescriptionItem
{
    public long Id { get; set; }

    public long PrescriptionId { get; set; }

    public long ProductId { get; set; }

    public decimal? DosageAmount { get; set; }

    public string? DosageUnit { get; set; }

    public string? FrequencyCode { get; set; }

    public string? Route { get; set; }

    public decimal? DurationValue { get; set; }

    public string? DurationUnit { get; set; }

    public decimal Quantity { get; set; }

    public string? Instructions { get; set; }

    public bool RefillAllowed { get; set; }

    public int RefillCount { get; set; }

    public virtual ICollection<DispensingItem> DispensingItems { get; set; } = new List<DispensingItem>();

    public virtual Prescription Prescription { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<Refill> Refills { get; set; } = new List<Refill>();

    public virtual ICollection<SaleLine> SaleLines { get; set; } = new List<SaleLine>();
}
