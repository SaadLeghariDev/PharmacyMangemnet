using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class FiscalDocumentLine
{
    public long Id { get; set; }

    public long FiscalDocumentId { get; set; }

    public long SaleLineId { get; set; }

    public decimal TaxableAmount { get; set; }

    public decimal TaxRate { get; set; }

    public decimal TaxAmount { get; set; }

    public virtual FiscalDocument FiscalDocument { get; set; } = null!;

    public virtual SaleLine SaleLine { get; set; } = null!;
}
