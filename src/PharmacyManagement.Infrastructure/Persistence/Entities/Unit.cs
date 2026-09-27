using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Unit
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string ShortCode { get; set; } = null!;

    public string? UnitType { get; set; }

    public bool DecimalAllowed { get; set; }

    public virtual ICollection<ProductUnit> ProductUnits { get; set; } = new List<ProductUnit>();

    public virtual ICollection<UnitConversion> UnitConversionFromUnits { get; set; } = new List<UnitConversion>();

    public virtual ICollection<UnitConversion> UnitConversionToUnits { get; set; } = new List<UnitConversion>();
}
