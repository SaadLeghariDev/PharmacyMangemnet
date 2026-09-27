using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class ProductAlias
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public string Alias { get; set; } = null!;

    public string? AliasType { get; set; }

    public virtual Product Product { get; set; } = null!;
}
