using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Sale
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long CounterId { get; set; }

    public long TerminalId { get; set; }

    public long UserId { get; set; }

    public long? CustomerId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public DateTime SaleDate { get; set; }

    public string SaleType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string CurrencyCode { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal RoundOff { get; set; }

    public decimal NetAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal DueAmount { get; set; }

    public decimal ChangeAmount { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public string? Fbrstatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Branch Branch { get; set; } = null!;

    public virtual Counter Counter { get; set; } = null!;

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<DispensingRecord> DispensingRecords { get; set; } = new List<DispensingRecord>();

    public virtual ICollection<FiscalDocument> FiscalDocuments { get; set; } = new List<FiscalDocument>();

    public virtual ICollection<InvoiceTaxis> InvoiceTaxes { get; set; } = new List<InvoiceTaxis>();

    public virtual ICollection<Refill> Refills { get; set; } = new List<Refill>();

    public virtual ICollection<SaleLine> SaleLines { get; set; } = new List<SaleLine>();

    public virtual ICollection<SalePayment> SalePayments { get; set; } = new List<SalePayment>();

    public virtual ICollection<SaleReturn> SaleReturns { get; set; } = new List<SaleReturn>();

    public virtual Posterminal Terminal { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
