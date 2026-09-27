using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class StockCount
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long WarehouseId { get; set; }

    public string CountNumber { get; set; } = null!;

    public DateTime CountDate { get; set; }

    public string Status { get; set; } = null!;

    public string? CountType { get; set; }

    public long? CreatedBy { get; set; }

    public long? ApprovedBy { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<StockCountLine> StockCountLines { get; set; } = new List<StockCountLine>();

    public virtual Warehouse Warehouse { get; set; } = null!;
}
