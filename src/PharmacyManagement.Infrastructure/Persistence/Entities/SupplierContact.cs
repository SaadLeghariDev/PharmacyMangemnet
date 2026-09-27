using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class SupplierContact
{
    public long Id { get; set; }

    public long SupplierId { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Designation { get; set; }

    public bool IsPrimary { get; set; }

    public virtual Supplier Supplier { get; set; } = null!;
}
