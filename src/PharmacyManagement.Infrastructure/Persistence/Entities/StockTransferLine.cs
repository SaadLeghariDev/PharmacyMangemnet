using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class StockTransferLine
{
    public long Id { get; set; }

    public long StockTransferId { get; set; }

    public long ProductId { get; set; }

    public long BatchId { get; set; }

    public long ProductUnitId { get; set; }

    public long FromLocationId { get; set; }

    public long ToLocationId { get; set; }

    public decimal Quantity { get; set; }

    public decimal ReceivedQuantity { get; set; }

    public virtual InventoryBatch Batch { get; set; } = null!;

    public virtual WarehouseLocation FromLocation { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ProductUnit ProductUnit { get; set; } = null!;

    public virtual StockTransfer StockTransfer { get; set; } = null!;

    public virtual WarehouseLocation ToLocation { get; set; } = null!;
}
