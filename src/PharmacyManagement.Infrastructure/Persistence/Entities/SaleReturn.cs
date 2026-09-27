using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class SaleReturn
{
    public long Id { get; set; }

    public long SaleId { get; set; }

    public long BranchId { get; set; }

    public string ReturnNumber { get; set; } = null!;

    public DateTime ReturnDate { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = null!;

    public decimal RefundAmount { get; set; }

    public long? RefundPaymentMethodId { get; set; }

    public long? CreatedBy { get; set; }

    public long? ApprovedBy { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual PaymentMethod? RefundPaymentMethod { get; set; }

    public virtual Sale Sale { get; set; } = null!;

    public virtual ICollection<SaleReturnLine> SaleReturnLines { get; set; } = new List<SaleReturnLine>();
}
