using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class ProductIngredient
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public long IngredientId { get; set; }

    public decimal? Strength { get; set; }

    public string? StrengthUnit { get; set; }

    public decimal? Percentage { get; set; }

    public bool IsPrimary { get; set; }

    public virtual Ingredient Ingredient { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
