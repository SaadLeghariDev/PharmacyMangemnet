using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Customers;

public sealed class CustomerQuery : PaginationQuery
{
    public bool? IsActive { get; set; }
    public bool? IsPatient { get; set; }
}

public sealed class CustomerDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Cnic { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsPatient { get; set; }
    public bool IsActive { get; set; }
    public decimal Balance { get; set; }
    public decimal AvailableCredit { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateCustomerRequest
{
    /// <summary>Optional; when omitted, allocated via NumberSequences (CUSTOMER / C-).</summary>
    public string? CustomerCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Cnic { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsPatient { get; set; }
}

public sealed class UpdateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Cnic { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsPatient { get; set; }
}

public sealed class CustomerLedgerQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public sealed class CustomerLedgerEntryDto
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long BranchId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public long SequenceNo { get; set; }
    public string? Remarks { get; set; }
}

public sealed class RecordCustomerPaymentRequest
{
    public long BranchId { get; set; }
    public long PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Remarks { get; set; }
}

public sealed class CustomerPaymentDto
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long BranchId { get; set; }
    public long PaymentMethodId { get; set; }
    public string? PaymentMethodCode { get; set; }
    public string? PaymentMethodName { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Remarks { get; set; }
    public long? ReceivedBy { get; set; }
}
