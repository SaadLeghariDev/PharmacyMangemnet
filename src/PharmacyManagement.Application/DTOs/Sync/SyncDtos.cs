using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Sync;

public sealed class SyncNodeDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public long BranchId { get; set; }
    public string? BranchName { get; set; }
    public long TerminalId { get; set; }
    public string? TerminalCode { get; set; }
    public string NodeCode { get; set; } = string.Empty;
    public DateTime? LastSyncAt { get; set; }
    public long LastSequence { get; set; }
    public bool IsActive { get; set; }
}

public sealed class SyncNodeQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public long? TerminalId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class RegisterSyncNodeRequest
{
    public long BranchId { get; set; }
    public long TerminalId { get; set; }
    public string NodeCode { get; set; } = string.Empty;
}

public sealed class UpdateSyncNodeRequest
{
    public bool IsActive { get; set; } = true;
}

public sealed class SyncItemDto
{
    public long Id { get; set; }
    public long SyncBatchId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public long Version { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public sealed class SyncBatchDto
{
    public long Id { get; set; }
    public long SyncNodeId { get; set; }
    public string? NodeCode { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public int PendingCount { get; set; }
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
    public int SkippedCount { get; set; }
    public IReadOnlyList<SyncItemDto> Items { get; set; } = Array.Empty<SyncItemDto>();
}

public sealed class SyncBatchQuery : PaginationQuery
{
    public long? SyncNodeId { get; set; }
    public string? Status { get; set; }
}

public sealed class CreateSyncBatchRequest
{
    public long SyncNodeId { get; set; }
    public string? BatchNumber { get; set; }
}

public sealed class UpdateSyncBatchStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public sealed class SyncItemQuery : PaginationQuery
{
    public string? Status { get; set; }
}

public sealed class CreateSyncItemRequest
{
    public string EntityName { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class CreateSyncItemsRequest
{
    public IReadOnlyList<CreateSyncItemRequest> Items { get; set; } = Array.Empty<CreateSyncItemRequest>();
}

public sealed class UpdateSyncItemStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public sealed class SyncPushRequest
{
    public long SyncNodeId { get; set; }
    public IReadOnlyList<CreateSyncItemRequest> Items { get; set; } = Array.Empty<CreateSyncItemRequest>();
    /// <summary>When true (default), apply items and complete the batch.</summary>
    public bool AutoComplete { get; set; } = true;
    /// <summary>When true, mark all items Failed instead of Processed (simulation).</summary>
    public bool SimulateFailures { get; set; }
}

public sealed class SyncPullRequest
{
    public long SyncNodeId { get; set; }
    public long? SinceSequence { get; set; }
    public int Limit { get; set; } = 100;
}

public sealed class SyncPushPullResultDto
{
    public SyncBatchDto Batch { get; set; } = null!;
    public SyncNodeDto Node { get; set; } = null!;
}

public sealed class IdempotencyKeyDto
{
    public long Id { get; set; }
    public long TerminalId { get; set; }
    public string? TerminalCode { get; set; }
    public string Key { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public long? EntityId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class IdempotencyKeyQuery : PaginationQuery
{
    public long? TerminalId { get; set; }
    public string? EntityType { get; set; }
}
