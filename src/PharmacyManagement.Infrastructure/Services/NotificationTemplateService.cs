using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Alerts;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class NotificationTemplateService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : INotificationTemplateService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<NotificationTemplateDto>> SearchAsync(
        NotificationTemplateQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.NotificationTemplates.AsNoTracking().Where(t => t.TenantId == tenantId);

        if (query.IsActive is bool active) q = q.Where(t => t.IsActive == active);
        if (!string.IsNullOrWhiteSpace(query.Channel))
        {
            var channel = query.Channel.Trim();
            q = q.Where(t => t.Channel == channel);
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(t => t.Code.Contains(s) || (t.Subject != null && t.Subject.Contains(s)));
        }

        q = q.OrderBy(t => t.Code);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return new PagedResult<NotificationTemplateDto>
        {
            Items = rows.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<NotificationTemplateDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.NotificationTemplates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<NotificationTemplateDto> CreateAsync(
        CreateNotificationTemplateRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var code = request.Code.Trim();
        if (await db.NotificationTemplates.AnyAsync(t => t.TenantId == tenantId && t.Code == code, ct))
            throw new ConflictException($"Notification template code '{code}' already exists.");

        var entity = new NotificationTemplate
        {
            TenantId = tenantId,
            Code = code,
            Channel = request.Channel.Trim(),
            Subject = string.IsNullOrWhiteSpace(request.Subject) ? null : request.Subject.Trim(),
            Body = request.Body,
            IsActive = request.IsActive
        };
        db.NotificationTemplates.Add(entity);
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<NotificationTemplateDto> UpdateAsync(
        long id, UpdateNotificationTemplateRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Notification template {id} not found.");

        var code = request.Code.Trim();
        if (await db.NotificationTemplates.AnyAsync(t => t.TenantId == tenantId && t.Code == code && t.Id != id, ct))
            throw new ConflictException($"Notification template code '{code}' already exists.");

        entity.Code = code;
        entity.Channel = request.Channel.Trim();
        entity.Subject = string.IsNullOrWhiteSpace(request.Subject) ? null : request.Subject.Trim();
        entity.Body = request.Body;
        entity.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<PagedResult<NotificationLogDto>> SearchLogsAsync(
        NotificationLogQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.NotificationLogs.AsNoTracking()
            .Include(l => l.Template)
            .Where(l => l.Template.TenantId == tenantId);

        if (query.TemplateId is long templateId) q = q.Where(l => l.TemplateId == templateId);
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim();
            q = q.Where(l => l.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(query.ReferenceType))
        {
            var refType = query.ReferenceType.Trim();
            q = q.Where(l => l.ReferenceType == refType);
        }

        q = q.OrderByDescending(l => l.Id);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return new PagedResult<NotificationLogDto>
        {
            Items = rows.Select(l => new NotificationLogDto
            {
                Id = l.Id,
                TemplateId = l.TemplateId,
                TemplateCode = l.Template.Code,
                Recipient = l.Recipient,
                Channel = l.Channel,
                ReferenceType = l.ReferenceType,
                ReferenceId = l.ReferenceId,
                Status = l.Status,
                SentAt = l.SentAt,
                ErrorMessage = l.ErrorMessage
            }).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    private static NotificationTemplateDto Map(NotificationTemplate t) => new()
    {
        Id = t.Id,
        TenantId = t.TenantId,
        Code = t.Code,
        Channel = t.Channel,
        Subject = t.Subject,
        Body = t.Body,
        IsActive = t.IsActive
    };
}
