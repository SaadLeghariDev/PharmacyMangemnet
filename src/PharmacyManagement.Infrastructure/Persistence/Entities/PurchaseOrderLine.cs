using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class PurchaseOrderLine
{
    public long Id { get; set; }

    public long PurchaseOrderId { get; set; }

    public long ProductId { get; set; }

    public long ProductUnitId { get; set; }

    public decimal Quantity { get; set; }

    public decimal FreeQuantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal NetAmount { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductUnit ProductUnit { get; set; } = null!;

    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
}
