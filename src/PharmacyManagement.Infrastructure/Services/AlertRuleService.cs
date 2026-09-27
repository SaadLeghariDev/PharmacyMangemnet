using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Alerts;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class AlertRuleService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IAlertRuleService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<AlertRuleDto>> SearchAsync(AlertRuleQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.AlertRules.AsNoTracking()
            .Include(r => r.Branch)
            .Where(r => r.TenantId == tenantId);

        if (query.BranchId is long branchId) q = q.Where(r => r.BranchId == branchId);
        if (query.IsActive is bool active) q = q.Where(r => r.IsActive == active);
        if (!string.IsNullOrWhiteSpace(query.AlertType))
        {
            var type = query.AlertType.Trim();
            q = q.Where(r => r.AlertType == type);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(r => r.AlertType.Contains(s) || (r.Branch != null && r.Branch.Code.Contains(s)));
        }

        q = q.OrderBy(r => r.AlertType).ThenBy(r => r.Id);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return new PagedResult<AlertRuleDto>
        {
            Items = rows.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<AlertRuleDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.AlertRules.AsNoTracking()
            .Include(r => r.Branch)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<AlertRuleDto> CreateAsync(CreateAlertRuleRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await ValidateBranchAsync(tenantId, request.BranchId, ct);

        var entity = new AlertRule
        {
            TenantId = tenantId,
            BranchId = request.BranchId,
            AlertType = AlertTypes.Normalize(request.AlertType),
            Threshold = request.Threshold,
            DaysBeforeExpiry = request.DaysBeforeExpiry,
            IsActive = request.IsActive
        };
        db.AlertRules.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<AlertRuleDto> UpdateAsync(long id, UpdateAlertRuleRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.AlertRules
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Alert rule {id} not found.");

        await ValidateBranchAsync(tenantId, request.BranchId, ct);

        entity.BranchId = request.BranchId;
        entity.AlertType = AlertTypes.Normalize(request.AlertType);
        entity.Threshold = request.Threshold;
        entity.DaysBeforeExpiry = request.DaysBeforeExpiry;
        entity.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    private async Task ValidateBranchAsync(long tenantId, long? branchId, CancellationToken ct)
    {
        if (branchId is null) return;
        _ = await db.Branches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == branchId && b.TenantId == tenantId && b.IsActive, ct)
            ?? throw new ValidationAppException(["Branch not found."]);
    }

    private static AlertRuleDto Map(AlertRule r) => new()
    {
        Id = r.Id,
        TenantId = r.TenantId,
        BranchId = r.BranchId,
        BranchCode = r.Branch?.Code,
        AlertType = r.AlertType,
        Threshold = r.Threshold,
        DaysBeforeExpiry = r.DaysBeforeExpiry,
        IsActive = r.IsActive
    };
}
