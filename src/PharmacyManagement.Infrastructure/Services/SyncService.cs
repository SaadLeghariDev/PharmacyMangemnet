using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Sync;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class SyncService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : ISyncService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<SyncNodeDto>> SearchNodesAsync(SyncNodeQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.SyncNodes.AsNoTracking()
            .Include(n => n.Branch)
            .Include(n => n.Terminal)
            .Where(n => n.TenantId == tenantId);

        if (query.BranchId is long branchId) q = q.Where(n => n.BranchId == branchId);
        if (query.TerminalId is long terminalId) q = q.Where(n => n.TerminalId == terminalId);
        if (query.IsActive is bool active) q = q.Where(n => n.IsActive == active);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(n => n.NodeCode.Contains(s)
                || n.Terminal.TerminalCode.Contains(s)
                || n.Branch.Name.Contains(s)
                || n.Branch.Code.Contains(s));
        }

        q = q.OrderBy(n => n.NodeCode);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<SyncNodeDto>
        {
            Items = rows.Select(MapNode).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<SyncNodeDto?> GetNodeByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.SyncNodes.AsNoTracking()
            .Include(n => n.Branch)
            .Include(n => n.Terminal)
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == tenantId, ct);
        return entity is null ? null : MapNode(entity);
    }

    public async Task<SyncNodeDto> RegisterNodeAsync(RegisterSyncNodeRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var code = request.NodeCode.Trim();

        var branch = await db.Branches.FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Branch {request.BranchId} not found.");
        var terminal = await db.Posterminals
            .Include(t => t.Branch)
            .FirstOrDefaultAsync(t => t.Id == request.TerminalId && t.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Terminal {request.TerminalId} not found.");

        if (terminal.BranchId != branch.Id)
            throw new ValidationAppException(["Terminal must belong to the selected branch."]);

        if (await db.SyncNodes.AnyAsync(n => n.NodeCode == code, ct))
            throw new ConflictException($"Sync node code '{code}' is already registered.");

        var entity = new SyncNode
        {
            TenantId = tenantId,
            BranchId = branch.Id,
            TerminalId = terminal.Id,
            NodeCode = code,
            LastSequence = 0,
            IsActive = true
        };
        db.SyncNodes.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetNodeByIdAsync(entity.Id, ct))!;
    }

    public async Task<SyncNodeDto> UpdateNodeAsync(long id, UpdateSyncNodeRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.SyncNodes.FirstOrDefaultAsync(n => n.Id == id && n.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Sync node {id} not found.");

        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return (await GetNodeByIdAsync(entity.Id, ct))!;
    }

    public async Task<PagedResult<SyncBatchDto>> SearchBatchesAsync(SyncBatchQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.SyncBatches.AsNoTracking()
            .Include(b => b.SyncNode)
            .Include(b => b.SyncItems)
            .Where(b => b.SyncNode.TenantId == tenantId);

        if (query.SyncNodeId is long nodeId) q = q.Where(b => b.SyncNodeId == nodeId);
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!SyncBatchStatuses.IsKnown(query.Status))
                throw new ValidationAppException([
                    $"Status must be one of: {SyncBatchStatuses.InProgress}, {SyncBatchStatuses.Completed}, {SyncBatchStatuses.Failed}, {SyncBatchStatuses.Partial}."
                ]);
            var status = SyncBatchStatuses.Normalize(query.Status);
            q = q.Where(b => b.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(b => b.BatchNumber.Contains(s) || b.SyncNode.NodeCode.Contains(s));
        }

        q = q.OrderByDescending(b => b.StartedAt);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<SyncBatchDto>
        {
            Items = rows.Select(b => MapBatch(b, includeItems: false)).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<SyncBatchDto?> GetBatchByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.SyncBatches.AsNoTracking()
            .Include(b => b.SyncNode)
            .Include(b => b.SyncItems)
            .FirstOrDefaultAsync(b => b.Id == id && b.SyncNode.TenantId == tenantId, ct);
        return entity is null ? null : MapBatch(entity, includeItems: true);
    }

    public async Task<SyncBatchDto> CreateBatchAsync(CreateSyncBatchRequest request, CancellationToken ct = default)
    {
        var node = await RequireActiveNodeAsync(request.SyncNodeId, ct);
        var batchNumber = string.IsNullOrWhiteSpace(request.BatchNumber)
            ? await AllocateBatchNumberAsync(node.Id, ct)
            : request.BatchNumber.Trim();

        if (await db.SyncBatches.AnyAsync(b => b.SyncNodeId == node.Id && b.BatchNumber == batchNumber, ct))
            throw new ConflictException($"Batch number '{batchNumber}' already exists for this node.");

        var entity = new SyncBatch
        {
            SyncNodeId = node.Id,
            BatchNumber = batchNumber,
            StartedAt = DateTime.UtcNow,
            Status = SyncBatchStatuses.InProgress
        };
        db.SyncBatches.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetBatchByIdAsync(entity.Id, ct))!;
    }

    public async Task<SyncBatchDto> UpdateBatchStatusAsync(
        long id, UpdateSyncBatchStatusRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        if (!SyncBatchStatuses.IsKnown(request.Status))
            throw new ValidationAppException([
                $"Status must be one of: {SyncBatchStatuses.InProgress}, {SyncBatchStatuses.Completed}, {SyncBatchStatuses.Failed}, {SyncBatchStatuses.Partial}."
            ]);

        var status = SyncBatchStatuses.Normalize(request.Status);
        var entity = await db.SyncBatches
            .Include(b => b.SyncNode)
            .Include(b => b.SyncItems)
            .FirstOrDefaultAsync(b => b.Id == id && b.SyncNode.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Sync batch {id} not found.");

        if (SyncBatchStatuses.IsTerminal(entity.Status) && entity.Status != status)
            throw new ValidationAppException([$"Batch is already {entity.Status} and cannot change to {status}."]);

        entity.Status = status;
        if (SyncBatchStatuses.IsTerminal(status))
            entity.CompletedAt ??= DateTime.UtcNow;
        else
            entity.CompletedAt = null;

        await db.SaveChangesAsync(ct);
        return (await GetBatchByIdAsync(entity.Id, ct))!;
    }

    public async Task<PagedResult<SyncItemDto>> SearchItemsAsync(
        long batchId, SyncItemQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        _ = await db.SyncBatches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == batchId && b.SyncNode.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Sync batch {batchId} not found.");

        var q = db.SyncItems.AsNoTracking().Where(i => i.SyncBatchId == batchId);
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!SyncItemStatuses.IsKnown(query.Status))
                throw new ValidationAppException([
                    $"Status must be one of: {SyncItemStatuses.Pending}, {SyncItemStatuses.Processed}, {SyncItemStatuses.Failed}, {SyncItemStatuses.Skipped}."
                ]);
            var status = SyncItemStatuses.Normalize(query.Status);
            q = q.Where(i => i.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(i => i.EntityName.Contains(s)
                || (i.ErrorMessage != null && i.ErrorMessage.Contains(s)));
        }

        q = q.OrderBy(i => i.Id);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<SyncItemDto>
        {
            Items = rows.Select(MapItem).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<IReadOnlyList<SyncItemDto>> AddItemsAsync(
        long batchId, CreateSyncItemsRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var batch = await db.SyncBatches
            .Include(b => b.SyncNode)
            .FirstOrDefaultAsync(b => b.Id == batchId && b.SyncNode.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Sync batch {batchId} not found.");

        if (batch.Status != SyncBatchStatuses.InProgress)
            throw new ValidationAppException(["Items can only be added to an InProgress batch."]);

        var created = new List<SyncItem>();
        foreach (var item in request.Items)
        {
            created.Add(BuildItem(batch.Id, item));
        }

        db.SyncItems.AddRange(created);
        await db.SaveChangesAsync(ct);
        return created.Select(MapItem).ToList();
    }

    public async Task<SyncItemDto> UpdateItemStatusAsync(
        long id, UpdateSyncItemStatusRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        if (!SyncItemStatuses.IsKnown(request.Status))
            throw new ValidationAppException([
                $"Status must be one of: {SyncItemStatuses.Pending}, {SyncItemStatuses.Processed}, {SyncItemStatuses.Failed}, {SyncItemStatuses.Skipped}."
            ]);

        var status = SyncItemStatuses.Normalize(request.Status);
        var entity = await db.SyncItems
            .Include(i => i.SyncBatch).ThenInclude(b => b.SyncNode)
            .FirstOrDefaultAsync(i => i.Id == id && i.SyncBatch.SyncNode.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Sync item {id} not found.");

        if (entity.SyncBatch.Status != SyncBatchStatuses.InProgress && status != entity.Status)
            throw new ValidationAppException(["Item status can only change while the batch is InProgress."]);

        entity.Status = status;
        if (status == SyncItemStatuses.Pending)
        {
            entity.ProcessedAt = null;
            entity.ErrorMessage = null;
        }
        else
        {
            entity.ProcessedAt ??= DateTime.UtcNow;
            entity.ErrorMessage = status == SyncItemStatuses.Failed
                ? TrimOrNull(request.ErrorMessage) ?? "Processing failed."
                : TrimOrNull(request.ErrorMessage);
        }

        await db.SaveChangesAsync(ct);
        return MapItem(entity);
    }

    public async Task<SyncPushPullResultDto> PushAsync(SyncPushRequest request, CancellationToken ct = default)
    {
        var node = await RequireActiveNodeAsync(request.SyncNodeId, ct);
        var batch = new SyncBatch
        {
            SyncNodeId = node.Id,
            BatchNumber = await AllocateBatchNumberAsync(node.Id, ct),
            StartedAt = DateTime.UtcNow,
            Status = SyncBatchStatuses.InProgress
        };
        db.SyncBatches.Add(batch);
        await db.SaveChangesAsync(ct);

        var items = request.Items.Select(i => BuildItem(batch.Id, i)).ToList();
        db.SyncItems.AddRange(items);
        await db.SaveChangesAsync(ct);

        if (request.AutoComplete)
        {
            foreach (var item in items)
            {
                if (request.SimulateFailures)
                {
                    item.Status = SyncItemStatuses.Failed;
                    item.ErrorMessage = "Simulated sync failure.";
                    item.ProcessedAt = DateTime.UtcNow;
                }
                else
                {
                    item.Status = SyncItemStatuses.Processed;
                    item.ProcessedAt = DateTime.UtcNow;
                }
            }

            ApplyBatchCompletion(batch, items);
            node.LastSequence = Math.Max(node.LastSequence, items.Max(i => i.Version));
            node.LastSyncAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        return new SyncPushPullResultDto
        {
            Batch = (await GetBatchByIdAsync(batch.Id, ct))!,
            Node = (await GetNodeByIdAsync(node.Id, ct))!
        };
    }

    public async Task<SyncPushPullResultDto> PullAsync(SyncPullRequest request, CancellationToken ct = default)
    {
        var node = await RequireActiveNodeAsync(request.SyncNodeId, ct);
        var since = request.SinceSequence ?? node.LastSequence;
        var limit = request.Limit is < 1 or > 500 ? 100 : request.Limit;

        // Server-side simulation: copy Processed items from other nodes in the same tenant
        // whose Version is greater than the caller's since-sequence.
        var sourceItems = await db.SyncItems.AsNoTracking()
            .Include(i => i.SyncBatch).ThenInclude(b => b.SyncNode)
            .Where(i => i.SyncBatch.SyncNode.TenantId == node.TenantId
                && i.SyncBatch.SyncNodeId != node.Id
                && i.Status == SyncItemStatuses.Processed
                && i.Version > since)
            .OrderBy(i => i.Version).ThenBy(i => i.Id)
            .Take(limit)
            .ToListAsync(ct);

        var batch = new SyncBatch
        {
            SyncNodeId = node.Id,
            BatchNumber = await AllocateBatchNumberAsync(node.Id, ct),
            StartedAt = DateTime.UtcNow,
            Status = SyncBatchStatuses.InProgress
        };
        db.SyncBatches.Add(batch);
        await db.SaveChangesAsync(ct);

        var pulled = sourceItems.Select(src => new SyncItem
        {
            SyncBatchId = batch.Id,
            EntityName = src.EntityName,
            EntityId = src.EntityId,
            Operation = src.Operation,
            Payload = src.Payload,
            Version = src.Version,
            Status = SyncItemStatuses.Processed,
            ProcessedAt = DateTime.UtcNow
        }).ToList();

        if (pulled.Count > 0)
            db.SyncItems.AddRange(pulled);

        ApplyBatchCompletion(batch, pulled);
        if (pulled.Count > 0)
            node.LastSequence = Math.Max(node.LastSequence, pulled.Max(i => i.Version));
        node.LastSyncAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return new SyncPushPullResultDto
        {
            Batch = (await GetBatchByIdAsync(batch.Id, ct))!,
            Node = (await GetNodeByIdAsync(node.Id, ct))!
        };
    }

    public async Task<PagedResult<IdempotencyKeyDto>> SearchIdempotencyKeysAsync(
        IdempotencyKeyQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.IdempotencyKeys.AsNoTracking()
            .Include(k => k.Terminal).ThenInclude(t => t.Branch)
            .Where(k => k.Terminal.Branch.TenantId == tenantId);

        if (query.TerminalId is long terminalId) q = q.Where(k => k.TerminalId == terminalId);
        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            var et = query.EntityType.Trim();
            q = q.Where(k => k.EntityType == et);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(k => k.Key.Contains(s) || k.EntityType.Contains(s));
        }

        q = q.OrderByDescending(k => k.CreatedAt);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<IdempotencyKeyDto>
        {
            Items = rows.Select(k => new IdempotencyKeyDto
            {
                Id = k.Id,
                TerminalId = k.TerminalId,
                TerminalCode = k.Terminal?.TerminalCode,
                Key = k.Key,
                EntityType = k.EntityType,
                EntityId = k.EntityId,
                CreatedAt = k.CreatedAt
            }).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    private async Task<SyncNode> RequireActiveNodeAsync(long syncNodeId, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        var node = await db.SyncNodes
            .FirstOrDefaultAsync(n => n.Id == syncNodeId && n.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Sync node {syncNodeId} not found.");
        if (!node.IsActive)
            throw new ValidationAppException(["Sync node is inactive."]);
        return node;
    }

    private async Task<string> AllocateBatchNumberAsync(long syncNodeId, CancellationToken ct)
    {
        var count = await db.SyncBatches.CountAsync(b => b.SyncNodeId == syncNodeId, ct);
        return $"SB-{(count + 1):D6}";
    }

    private static SyncItem BuildItem(long batchId, CreateSyncItemRequest request)
    {
        if (!SyncItemOperations.IsKnown(request.Operation))
            throw new ValidationAppException([
                $"Operation must be one of: {SyncItemOperations.Insert}, {SyncItemOperations.Update}, {SyncItemOperations.Delete}."
            ]);

        return new SyncItem
        {
            SyncBatchId = batchId,
            EntityName = request.EntityName.Trim(),
            EntityId = request.EntityId,
            Operation = SyncItemOperations.Normalize(request.Operation),
            Payload = string.IsNullOrWhiteSpace(request.Payload) ? null : request.Payload,
            Version = request.Version <= 0 ? 1 : request.Version,
            Status = SyncItemStatuses.Pending
        };
    }

    private static void ApplyBatchCompletion(SyncBatch batch, IReadOnlyList<SyncItem> items)
    {
        if (items.Count == 0)
        {
            batch.Status = SyncBatchStatuses.Completed;
            batch.CompletedAt = DateTime.UtcNow;
            return;
        }

        var failed = items.Count(i => i.Status == SyncItemStatuses.Failed);
        var pending = items.Count(i => i.Status == SyncItemStatuses.Pending);
        if (pending > 0)
        {
            // Leave InProgress if anything still pending
            batch.Status = SyncBatchStatuses.InProgress;
            batch.CompletedAt = null;
            return;
        }

        if (failed == items.Count)
            batch.Status = SyncBatchStatuses.Failed;
        else if (failed > 0)
            batch.Status = SyncBatchStatuses.Partial;
        else
            batch.Status = SyncBatchStatuses.Completed;

        batch.CompletedAt = DateTime.UtcNow;
    }

    private static SyncNodeDto MapNode(SyncNode n) => new()
    {
        Id = n.Id,
        TenantId = n.TenantId,
        BranchId = n.BranchId,
        BranchName = n.Branch?.Name,
        TerminalId = n.TerminalId,
        TerminalCode = n.Terminal?.TerminalCode,
        NodeCode = n.NodeCode,
        LastSyncAt = n.LastSyncAt,
        LastSequence = n.LastSequence,
        IsActive = n.IsActive
    };

    private static SyncBatchDto MapBatch(SyncBatch b, bool includeItems)
    {
        var items = b.SyncItems?.ToList() ?? [];
        return new SyncBatchDto
        {
            Id = b.Id,
            SyncNodeId = b.SyncNodeId,
            NodeCode = b.SyncNode?.NodeCode,
            BatchNumber = b.BatchNumber,
            StartedAt = b.StartedAt,
            CompletedAt = b.CompletedAt,
            Status = b.Status,
            ItemCount = items.Count,
            PendingCount = items.Count(i => i.Status == SyncItemStatuses.Pending),
            ProcessedCount = items.Count(i => i.Status == SyncItemStatuses.Processed),
            FailedCount = items.Count(i => i.Status == SyncItemStatuses.Failed),
            SkippedCount = items.Count(i => i.Status == SyncItemStatuses.Skipped),
            Items = includeItems ? items.OrderBy(i => i.Id).Select(MapItem).ToList() : Array.Empty<SyncItemDto>()
        };
    }

    private static SyncItemDto MapItem(SyncItem i) => new()
    {
        Id = i.Id,
        SyncBatchId = i.SyncBatchId,
        EntityName = i.EntityName,
        EntityId = i.EntityId,
        Operation = i.Operation,
        Payload = i.Payload,
        Version = i.Version,
        ProcessedAt = i.ProcessedAt,
        Status = i.Status,
        ErrorMessage = i.ErrorMessage
    };

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
