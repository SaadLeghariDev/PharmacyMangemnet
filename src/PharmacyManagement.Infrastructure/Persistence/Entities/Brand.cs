using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Brand
{
    public long Id { get; set; }

    public long ManufacturerId { get; set; }

    public string Name { get; set; } = null!;

    public string? Code { get; set; }

    public bool IsActive { get; set; }

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
