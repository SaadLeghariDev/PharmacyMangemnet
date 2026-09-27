using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Finance;

public sealed class AccountTypeQuery : PaginationQuery
{
}

public sealed class AccountTypeDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class ChartOfAccountQuery : PaginationQuery
{
    public long? AccountTypeId { get; set; }
    public long? ParentAccountId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsSystemAccount { get; set; }
}

public sealed class ChartOfAccountDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public long? ParentAccountId { get; set; }
    public string? ParentAccountCode { get; set; }
    public string? ParentAccountName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long AccountTypeId { get; set; }
    public string? AccountTypeName { get; set; }
    public bool IsSystemAccount { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateChartOfAccountRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long AccountTypeId { get; set; }
    public long? ParentAccountId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateChartOfAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public long AccountTypeId { get; set; }
    public long? ParentAccountId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class JournalEntryQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public sealed class JournalLineDto
{
    public long Id { get; set; }
    public int LineNo { get; set; }
    public long AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
}

public sealed class JournalEntryDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public long BranchId { get; set; }
    public string? BranchName { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public long? PostedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public long? ReversalOfEntryId { get; set; }
    public string? ReversalOfEntryNumber { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public IReadOnlyList<JournalLineDto> Lines { get; set; } = Array.Empty<JournalLineDto>();
}

public sealed class CreateJournalLineRequest
{
    public long AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
}

public sealed class CreateJournalEntryRequest
{
    public long BranchId { get; set; }
    public DateTime EntryDate { get; set; }
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public string? Description { get; set; }
    public List<CreateJournalLineRequest> Lines { get; set; } = [];
}
