using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Ingredient
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? GenericName { get; set; }

    public string? Code { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<ProductIngredient> ProductIngredients { get; set; } = new List<ProductIngredient>();
}
