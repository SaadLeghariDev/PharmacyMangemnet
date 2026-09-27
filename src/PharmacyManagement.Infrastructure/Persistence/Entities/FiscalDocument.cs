using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class FiscalDocument
{
    public long Id { get; set; }

    public long SaleId { get; set; }

    public string Provider { get; set; } = null!;

    public string DocumentType { get; set; } = null!;

    public string? InternalInvoiceNumber { get; set; }

    public string? ExternalInvoiceNumber { get; set; }

    public string? FbrinvoiceNumber { get; set; }

    public string SubmissionStatus { get; set; } = null!;

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ResponseAt { get; set; }

    public string? Qrdata { get; set; }

    public string? VerificationUrl { get; set; }

    public string? RawRequest { get; set; }

    public string? RawResponse { get; set; }

    public virtual ICollection<FiscalDocumentLine> FiscalDocumentLines { get; set; } = new List<FiscalDocumentLine>();

    public virtual ICollection<FiscalSubmission> FiscalSubmissions { get; set; } = new List<FiscalSubmission>();

    public virtual Sale Sale { get; set; } = null!;
}
