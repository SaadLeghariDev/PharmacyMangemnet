using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Fiscal;

public sealed class FiscalDocumentQuery : PaginationQuery
{
    public long? SaleId { get; set; }
    public string? SubmissionStatus { get; set; }
}

public sealed class FiscalDocumentLineDto
{
    public long Id { get; set; }
    public long SaleLineId { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
}

public sealed class FiscalSubmissionDto
{
    public long Id { get; set; }
    public int AttemptNo { get; set; }
    public string? RequestId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? HttpStatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public sealed class FiscalDocumentDto
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string? InternalInvoiceNumber { get; set; }
    public string? ExternalInvoiceNumber { get; set; }
    public string? FbrInvoiceNumber { get; set; }
    public string SubmissionStatus { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ResponseAt { get; set; }
    public string? QrData { get; set; }
    public string? VerificationUrl { get; set; }
    public string? SaleFbrStatus { get; set; }
    public IReadOnlyList<FiscalDocumentLineDto> Lines { get; set; } = Array.Empty<FiscalDocumentLineDto>();
    public IReadOnlyList<FiscalSubmissionDto> Submissions { get; set; } = Array.Empty<FiscalSubmissionDto>();
}

public sealed class CreateFiscalDocumentRequest
{
    public long SaleId { get; set; }
    public string? Provider { get; set; }
    public string DocumentType { get; set; } = "SaleInvoice";
}

public sealed class FiscalSubmitRequest
{
    /// <summary>When true, mock gateway returns failure (for retry testing).</summary>
    public bool ForceFailure { get; set; }
}
