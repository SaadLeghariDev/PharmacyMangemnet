using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class UnitConversion
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public long FromUnitId { get; set; }

    public long ToUnitId { get; set; }

    public decimal ConversionFactor { get; set; }

    public bool IsExact { get; set; }

    public virtual Unit FromUnit { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Unit ToUnit { get; set; } = null!;
}
