using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Expense
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long CategoryId { get; set; }

    public string ExpenseNumber { get; set; } = null!;

    public DateTime ExpenseDate { get; set; }

    public decimal Amount { get; set; }

    public long PaymentMethodId { get; set; }

    public string? Description { get; set; }

    public long? CreatedBy { get; set; }

    public long? ApprovedBy { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ExpenseCategory Category { get; set; } = null!;

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
