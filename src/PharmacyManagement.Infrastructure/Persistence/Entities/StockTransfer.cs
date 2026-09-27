using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class StockTransfer
{
    public long Id { get; set; }

    public string TransferNumber { get; set; } = null!;

    public long FromWarehouseId { get; set; }

    public long ToWarehouseId { get; set; }

    public DateTime TransferDate { get; set; }

    public string Status { get; set; } = null!;

    public long? RequestedBy { get; set; }

    public long? ApprovedBy { get; set; }

    public long? ReceivedBy { get; set; }

    public virtual Warehouse FromWarehouse { get; set; } = null!;

    public virtual ICollection<StockTransferLine> StockTransferLines { get; set; } = new List<StockTransferLine>();

    public virtual Warehouse ToWarehouse { get; set; } = null!;
}
