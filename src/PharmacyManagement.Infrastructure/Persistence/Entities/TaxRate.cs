using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class TaxRate
{
    public long Id { get; set; }

    public long TaxProfileId { get; set; }

    public decimal Rate { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public bool IsActive { get; set; }

    public virtual TaxProfile TaxProfile { get; set; } = null!;
}
