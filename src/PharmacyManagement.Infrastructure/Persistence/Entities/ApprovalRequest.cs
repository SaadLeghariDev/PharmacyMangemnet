using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class ApprovalRequest
{
    public long Id { get; set; }

    public long? BranchId { get; set; }

    public string RequestType { get; set; } = null!;

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public long? RequestedBy { get; set; }

    public long? ApprovedBy { get; set; }

    public long? RejectedBy { get; set; }

    public string Status { get; set; } = null!;

    public string? Reason { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? DecisionAt { get; set; }
}
