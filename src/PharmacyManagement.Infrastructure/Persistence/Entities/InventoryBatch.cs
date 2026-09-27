using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class InventoryBatch
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public long GoodsReceiptLineId { get; set; }

    public long SupplierId { get; set; }

    public long WarehouseId { get; set; }

    public string BatchNumber { get; set; } = null!;

    public DateOnly? ManufacturingDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public decimal QuantityReceived { get; set; }

    public decimal FreeQuantity { get; set; }

    public decimal PurchaseCost { get; set; }

    public decimal Mrp { get; set; }

    public decimal SalePrice { get; set; }

    public string BatchStatus { get; set; } = null!;

    public bool IsRecalled { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public virtual ICollection<BarcodePrintJob> BarcodePrintJobs { get; set; } = new List<BarcodePrintJob>();

    public virtual ICollection<DispensingItem> DispensingItems { get; set; } = new List<DispensingItem>();

    public virtual GoodsReceiptLine GoodsReceiptLine { get; set; } = null!;

    public virtual ICollection<InventoryBatchLocation> InventoryBatchLocations { get; set; } = new List<InventoryBatchLocation>();

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<SaleLineBatch> SaleLineBatches { get; set; } = new List<SaleLineBatch>();

    public virtual ICollection<SaleReturnLine> SaleReturnLines { get; set; } = new List<SaleReturnLine>();

    public virtual ICollection<StockAdjustmentLine> StockAdjustmentLines { get; set; } = new List<StockAdjustmentLine>();

    public virtual ICollection<StockCountLine> StockCountLines { get; set; } = new List<StockCountLine>();

    public virtual ICollection<StockTransferLine> StockTransferLines { get; set; } = new List<StockTransferLine>();

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual ICollection<SupplierReturnLine> SupplierReturnLines { get; set; } = new List<SupplierReturnLine>();

    public virtual Warehouse Warehouse { get; set; } = null!;
}
