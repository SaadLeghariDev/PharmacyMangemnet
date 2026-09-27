using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class ProductCategory
{
    public long Id { get; set; }

    public long? ParentCategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Code { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ProductCategory> InverseParentCategory { get; set; } = new List<ProductCategory>();

    public virtual ProductCategory? ParentCategory { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
