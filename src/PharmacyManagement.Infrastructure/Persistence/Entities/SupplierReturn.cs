using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class SupplierReturn
{
    public long Id { get; set; }

    public long SupplierId { get; set; }

    public long BranchId { get; set; }

    public long WarehouseId { get; set; }

    public string ReturnNumber { get; set; } = null!;

    public DateTime ReturnDate { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual ICollection<SupplierReturnLine> SupplierReturnLines { get; set; } = new List<SupplierReturnLine>();

    public virtual Warehouse Warehouse { get; set; } = null!;
}
