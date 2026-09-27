using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class SaleLine
{
    public long Id { get; set; }

    public long SaleId { get; set; }

    public long ProductId { get; set; }

    public long ProductUnitId { get; set; }

    public decimal Quantity { get; set; }

    public decimal BaseQuantity { get; set; }

    public decimal ConversionFactor { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Mrp { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal NetAmount { get; set; }

    public long? PrescriptionItemId { get; set; }

    public virtual ICollection<FiscalDocumentLine> FiscalDocumentLines { get; set; } = new List<FiscalDocumentLine>();

    public virtual ICollection<InvoiceTaxis> InvoiceTaxes { get; set; } = new List<InvoiceTaxis>();

    public virtual PrescriptionItem? PrescriptionItem { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductUnit ProductUnit { get; set; } = null!;

    public virtual Sale Sale { get; set; } = null!;

    public virtual ICollection<SaleLineBatch> SaleLineBatches { get; set; } = new List<SaleLineBatch>();

    public virtual ICollection<SaleReturnLine> SaleReturnLines { get; set; } = new List<SaleReturnLine>();
}
