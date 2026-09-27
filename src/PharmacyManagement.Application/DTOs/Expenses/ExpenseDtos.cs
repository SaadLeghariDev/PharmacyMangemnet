using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Expenses;

public sealed class ExpenseCategoryQuery : PaginationQuery
{
}

public sealed class ExpenseCategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public sealed class CreateExpenseCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public sealed class UpdateExpenseCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public sealed class ExpenseQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public long? CategoryId { get; set; }
    public long? PaymentMethodId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public sealed class ExpenseDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public string? BranchName { get; set; }
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryCode { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public decimal Amount { get; set; }
    public long PaymentMethodId { get; set; }
    public string? PaymentMethodCode { get; set; }
    public string? PaymentMethodName { get; set; }
    public string? PaymentMethodType { get; set; }
    public string? Description { get; set; }
    public long? CreatedBy { get; set; }
    public long? ApprovedBy { get; set; }
}

public sealed class CreateExpenseRequest
{
    public long BranchId { get; set; }
    public long CategoryId { get; set; }
    public DateTime ExpenseDate { get; set; }
    public decimal Amount { get; set; }
    public long PaymentMethodId { get; set; }
    public string? Description { get; set; }
    /// <summary>Required when payment method is Cash — open cash shift is posted for this terminal.</summary>
    public long? TerminalId { get; set; }
}

public sealed class UpdateExpenseRequest
{
    public string? Description { get; set; }
}
