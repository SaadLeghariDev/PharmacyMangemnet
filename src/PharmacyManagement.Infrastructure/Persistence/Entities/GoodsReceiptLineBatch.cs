using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class GoodsReceiptLineBatch
{
    public long Id { get; set; }

    public long GoodsReceiptLineId { get; set; }

    public string BatchNumber { get; set; } = null!;

    public DateOnly? ManufacturingDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public decimal Mrp { get; set; }

    public decimal SalePrice { get; set; }

    public decimal Quantity { get; set; }

    public decimal FreeQuantity { get; set; }

    public long WarehouseLocationId { get; set; }

    public virtual GoodsReceiptLine GoodsReceiptLine { get; set; } = null!;

    public virtual WarehouseLocation WarehouseLocation { get; set; } = null!;
}
