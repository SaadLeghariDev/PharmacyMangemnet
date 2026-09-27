using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class ProductPrice
{
    public long Id { get; set; }

    public long PriceListId { get; set; }

    public long ProductId { get; set; }

    public long ProductUnitId { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SalePrice { get; set; }

    public decimal Mrp { get; set; }

    public decimal DiscountPercent { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public virtual PriceList PriceList { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ProductUnit ProductUnit { get; set; } = null!;
}
