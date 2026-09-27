using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class SalePayment
{
    public long Id { get; set; }

    public long SaleId { get; set; }

    public long PaymentMethodId { get; set; }

    public decimal Amount { get; set; }

    public string? ReferenceNumber { get; set; }

    public DateTime PaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public long? ParentPaymentId { get; set; }

    public string TransactionType { get; set; } = null!;

    public virtual ICollection<SalePayment> InverseParentPayment { get; set; } = new List<SalePayment>();

    public virtual SalePayment? ParentPayment { get; set; }

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    public virtual Sale Sale { get; set; } = null!;
}
