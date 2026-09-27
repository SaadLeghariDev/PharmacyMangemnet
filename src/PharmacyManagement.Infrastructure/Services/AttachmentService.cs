using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Hardware;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class AttachmentService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IAttachmentService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<AttachmentDto>> SearchAsync(AttachmentQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.Attachments.AsNoTracking().Where(a => a.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(a => a.FileName.Contains(s) || a.StoragePath.Contains(s));
        }

        q = q.OrderByDescending(a => a.CreatedAt);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<AttachmentDto>
        {
            Items = rows.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<AttachmentDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Attachments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<AttachmentDto> CreateAsync(CreateAttachmentRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = new Attachment
        {
            TenantId = tenantId,
            FileName = request.FileName.Trim(),
            StoragePath = request.StoragePath.Trim(),
            ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? null : request.ContentType.Trim(),
            FileSize = request.FileSize,
            Hash = string.IsNullOrWhiteSpace(request.Hash) ? null : request.Hash.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        db.Attachments.Add(entity);
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<PagedResult<EntityAttachmentDto>> SearchLinksAsync(
        EntityAttachmentQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.EntityAttachments.AsNoTracking()
            .Include(e => e.Attachment)
            .Where(e => e.Attachment.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            var name = query.EntityName.Trim();
            q = q.Where(e => e.EntityName == name);
        }

        if (query.EntityId is long entityId) q = q.Where(e => e.EntityId == entityId);
        if (query.AttachmentId is long attachmentId) q = q.Where(e => e.AttachmentId == attachmentId);

        q = q.OrderByDescending(e => e.Id);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<EntityAttachmentDto>
        {
            Items = rows.Select(MapLink).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<EntityAttachmentDto> LinkAsync(
        CreateEntityAttachmentRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var attachment = await db.Attachments
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId && a.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Attachment {request.AttachmentId} not found.");

        var entity = new EntityAttachment
        {
            AttachmentId = attachment.Id,
            EntityName = request.EntityName.Trim(),
            EntityId = request.EntityId
        };
        db.EntityAttachments.Add(entity);
        await db.SaveChangesAsync(ct);

        return new EntityAttachmentDto
        {
            Id = entity.Id,
            AttachmentId = entity.AttachmentId,
            EntityName = entity.EntityName,
            EntityId = entity.EntityId,
            FileName = attachment.FileName,
            StoragePath = attachment.StoragePath,
            ContentType = attachment.ContentType
        };
    }

    private static AttachmentDto Map(Attachment a) => new()
    {
        Id = a.Id,
        TenantId = a.TenantId,
        FileName = a.FileName,
        StoragePath = a.StoragePath,
        ContentType = a.ContentType,
        FileSize = a.FileSize,
        Hash = a.Hash,
        CreatedAt = a.CreatedAt
    };

    private static EntityAttachmentDto MapLink(EntityAttachment e) => new()
    {
        Id = e.Id,
        AttachmentId = e.AttachmentId,
        EntityName = e.EntityName,
        EntityId = e.EntityId,
        FileName = e.Attachment?.FileName,
        StoragePath = e.Attachment?.StoragePath,
        ContentType = e.Attachment?.ContentType
    };
}
