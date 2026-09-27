using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class StockAdjustment
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long WarehouseId { get; set; }

    public string AdjustmentNumber { get; set; } = null!;

    public DateTime AdjustmentDate { get; set; }

    public string AdjustmentType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public long ReasonCodeId { get; set; }

    public long? CreatedBy { get; set; }

    public long? ApprovedBy { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ReasonCode ReasonCode { get; set; } = null!;

    public virtual ICollection<StockAdjustmentLine> StockAdjustmentLines { get; set; } = new List<StockAdjustmentLine>();

    public virtual Warehouse Warehouse { get; set; } = null!;
}
