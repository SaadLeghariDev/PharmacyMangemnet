using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class WarehouseLocation
{
    public long Id { get; set; }

    public long WarehouseId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? RackNo { get; set; }

    public string? ShelfNo { get; set; }

    public string? BinNo { get; set; }

    public string? LocationType { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<GoodsReceiptLineBatch> GoodsReceiptLineBatches { get; set; } = new List<GoodsReceiptLineBatch>();

    public virtual ICollection<InventoryBatchLocation> InventoryBatchLocations { get; set; } = new List<InventoryBatchLocation>();

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual ICollection<SaleReturnLine> SaleReturnLines { get; set; } = new List<SaleReturnLine>();

    public virtual ICollection<StockAdjustmentLine> StockAdjustmentLines { get; set; } = new List<StockAdjustmentLine>();

    public virtual ICollection<StockCountLine> StockCountLines { get; set; } = new List<StockCountLine>();

    public virtual ICollection<StockTransferLine> StockTransferLineFromLocations { get; set; } = new List<StockTransferLine>();

    public virtual ICollection<StockTransferLine> StockTransferLineToLocations { get; set; } = new List<StockTransferLine>();

    public virtual Warehouse Warehouse { get; set; } = null!;
}
