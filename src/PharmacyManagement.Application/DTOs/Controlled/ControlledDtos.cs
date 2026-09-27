using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Controlled;

public sealed class ControlledRegisterQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public long? ProductId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ControlledRegisterDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public long ProductId { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }
    public string RegisterNumber { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<ControlledTransactionDto> RecentTransactions { get; set; } = Array.Empty<ControlledTransactionDto>();
}

public sealed class ControlledTransactionDto
{
    public long Id { get; set; }
    public long RegisterId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public decimal Quantity { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public long? PrescriptionId { get; set; }
    public long? DoctorId { get; set; }
    public long? PerformedBy { get; set; }
    public long? WitnessedBy { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Remarks { get; set; }
}

public sealed class OpenControlledRegisterRequest
{
    public long BranchId { get; set; }
    public long ProductId { get; set; }
    public decimal OpeningBalance { get; set; }
    public string? RegisterNumber { get; set; }
}

public sealed class PostControlledTransactionRequest
{
    public string TransactionType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public long? WitnessedBy { get; set; }
    public string? Remarks { get; set; }
}

public sealed class ControlledTransactionQuery : PaginationQuery
{
}
