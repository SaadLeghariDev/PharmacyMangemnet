using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class FiscalSubmission
{
    public long Id { get; set; }

    public long FiscalDocumentId { get; set; }

    public int AttemptNo { get; set; }

    public string? RequestId { get; set; }

    public string Status { get; set; } = null!;

    public int? HttpStatusCode { get; set; }

    public string? RequestPayload { get; set; }

    public string? ResponsePayload { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime SubmittedAt { get; set; }

    public virtual FiscalDocument FiscalDocument { get; set; } = null!;
}
