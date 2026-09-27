using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Cash;

public sealed class CashShiftQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public long? TerminalId { get; set; }
    public long? UserId { get; set; }
    public string? Status { get; set; }
}

public sealed class CashTransactionDto
{
    public long Id { get; set; }
    public long CashShiftId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public decimal Amount { get; set; }
    public string? Remarks { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class CashShiftDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public long CounterId { get; set; }
    public long TerminalId { get; set; }
    public long UserId { get; set; }
    public decimal OpeningAmount { get; set; }
    public DateTime OpeningAt { get; set; }
    public decimal? ClosingAmount { get; set; }
    public decimal? ExpectedAmount { get; set; }
    public decimal? VarianceAmount { get; set; }
    public DateTime? ClosingAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal RunningTotal { get; set; }
    public IReadOnlyList<CashTransactionDto> Transactions { get; set; } = Array.Empty<CashTransactionDto>();
}

public sealed class OpenCashShiftRequest
{
    public long BranchId { get; set; }
    public long CounterId { get; set; }
    public long TerminalId { get; set; }
    public decimal OpeningAmount { get; set; }
    public string? Remarks { get; set; }
}

public sealed class CashDrawerMovementRequest
{
    public decimal Amount { get; set; }
    public string? Remarks { get; set; }
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
}

public sealed class CloseCashShiftRequest
{
    public decimal ClosingAmount { get; set; }
    public string? Remarks { get; set; }
}
