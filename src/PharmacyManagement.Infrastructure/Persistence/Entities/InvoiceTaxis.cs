using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class InvoiceTaxis
{
    public long Id { get; set; }

    public long SaleId { get; set; }

    public long? SaleLineId { get; set; }

    public long TaxProfileId { get; set; }

    public decimal TaxRate { get; set; }

    public decimal TaxableAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public virtual Sale Sale { get; set; } = null!;

    public virtual SaleLine? SaleLine { get; set; }

    public virtual TaxProfile TaxProfile { get; set; } = null!;
}
