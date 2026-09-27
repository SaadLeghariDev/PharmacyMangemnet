using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Admin;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class AuditLogService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IAuditLogService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<AuditLogDto>> SearchAsync(AuditLogQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.AuditLogs.AsNoTracking().Where(a => a.TenantId == null || a.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            var name = query.EntityName.Trim();
            q = q.Where(a => a.EntityName == name);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            var action = query.Action.Trim();
            q = q.Where(a => a.Action == action);
        }

        if (query.UserId is long userId) q = q.Where(a => a.UserId == userId);
        if (query.BranchId is long branchId) q = q.Where(a => a.BranchId == branchId);
        if (query.FromUtc is DateTime from) q = q.Where(a => a.CreatedAt >= from);
        if (query.ToUtc is DateTime to) q = q.Where(a => a.CreatedAt <= to);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(a =>
                a.EntityName.Contains(s) ||
                a.Action.Contains(s) ||
                (a.OldValues != null && a.OldValues.Contains(s)) ||
                (a.NewValues != null && a.NewValues.Contains(s)));
        }

        q = q.OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.Id);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<AuditLogDto>
        {
            Items = rows.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<AuditLogDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.AuditLogs.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && (a.TenantId == null || a.TenantId == tenantId), ct);
        return entity is null ? null : Map(entity);
    }

    private static AuditLogDto Map(Persistence.Entities.AuditLog a) => new()
    {
        Id = a.Id,
        TenantId = a.TenantId,
        BranchId = a.BranchId,
        UserId = a.UserId,
        EntityName = a.EntityName,
        EntityId = a.EntityId,
        Action = a.Action,
        OldValues = a.OldValues,
        NewValues = a.NewValues,
        IpAddress = a.Ipaddress,
        TerminalId = a.TerminalId,
        CreatedAt = a.CreatedAt
    };
}
