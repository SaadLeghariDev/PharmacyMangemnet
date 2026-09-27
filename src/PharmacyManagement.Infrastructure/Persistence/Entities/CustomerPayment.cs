using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class CustomerPayment
{
    public long Id { get; set; }

    public long CustomerId { get; set; }

    public long BranchId { get; set; }

    public long PaymentMethodId { get; set; }

    public decimal Amount { get; set; }

    public string? ReferenceNumber { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? Remarks { get; set; }

    public long? ReceivedBy { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
