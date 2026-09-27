using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class PriceList
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public string Name { get; set; } = null!;

    public string? PriceType { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    public virtual Tenant Tenant { get; set; } = null!;
}
