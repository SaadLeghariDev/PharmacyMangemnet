using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class CustomerLedger
{
    public long Id { get; set; }

    public long CustomerId { get; set; }

    public long BranchId { get; set; }

    public DateTime TransactionDate { get; set; }

    public string TransactionType { get; set; } = null!;

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public long SequenceNo { get; set; }

    public string? Remarks { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;
}
