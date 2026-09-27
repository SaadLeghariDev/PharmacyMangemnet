using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class ReorderRule
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long WarehouseId { get; set; }

    public long ProductId { get; set; }

    public decimal MinimumStock { get; set; }

    public decimal MaximumStock { get; set; }

    public decimal ReorderPoint { get; set; }

    public decimal ReorderQuantity { get; set; }

    public long PreferredSupplierId { get; set; }

    public bool IsActive { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Supplier PreferredSupplier { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
