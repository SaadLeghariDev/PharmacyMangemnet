using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class TaxProfile
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? TaxType { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<InvoiceTaxis> InvoiceTaxes { get; set; } = new List<InvoiceTaxis>();

    public virtual ICollection<TaxRate> TaxRates { get; set; } = new List<TaxRate>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
