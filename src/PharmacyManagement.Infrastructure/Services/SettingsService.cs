using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Admin;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class SettingsService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : ISettingsService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<TenantSettingDto>> SearchTenantSettingsAsync(SettingQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.TenantSettings.AsNoTracking().Where(s => s.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(query.Key))
        {
            var key = query.Key.Trim();
            q = q.Where(s => s.SettingKey == key);
        }
        else if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.SettingKey.Contains(s) || (x.SettingValue != null && x.SettingValue.Contains(s)));
        }

        q = q.OrderBy(x => x.SettingKey);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<TenantSettingDto>
        {
            Items = rows.Select(MapTenant).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<TenantSettingDto?> GetTenantSettingAsync(string key, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.TenantSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.SettingKey == key, ct);
        return entity is null ? null : MapTenant(entity);
    }

    public async Task<TenantSettingDto> UpsertTenantSettingAsync(UpsertSettingRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var key = request.SettingKey.Trim();
        var entity = await db.TenantSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.SettingKey == key, ct);
        if (entity is null)
        {
            entity = new TenantSetting
            {
                TenantId = tenantId,
                SettingKey = key,
                SettingValue = request.SettingValue,
                IsEncrypted = request.IsEncrypted
            };
            db.TenantSettings.Add(entity);
        }
        else
        {
            entity.SettingValue = request.SettingValue;
            entity.IsEncrypted = request.IsEncrypted;
        }

        await db.SaveChangesAsync(ct);
        return MapTenant(entity);
    }

    public async Task<PagedResult<BranchSettingDto>> SearchBranchSettingsAsync(BranchSettingQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.BranchSettings.AsNoTracking()
            .Include(s => s.Branch)
            .Where(s => s.Branch.TenantId == tenantId);

        if (query.BranchId is long branchId) q = q.Where(s => s.BranchId == branchId);
        if (!string.IsNullOrWhiteSpace(query.Key))
        {
            var key = query.Key.Trim();
            q = q.Where(s => s.SettingKey == key);
        }
        else if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.SettingKey.Contains(s) || (x.SettingValue != null && x.SettingValue.Contains(s)));
        }

        q = q.OrderBy(x => x.BranchId).ThenBy(x => x.SettingKey);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<BranchSettingDto>
        {
            Items = rows.Select(MapBranch).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<BranchSettingDto?> GetBranchSettingAsync(long branchId, string key, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await EnsureBranchAsync(tenantId, branchId, ct);
        var entity = await db.BranchSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.BranchId == branchId && s.SettingKey == key, ct);
        return entity is null ? null : MapBranch(entity);
    }

    public async Task<BranchSettingDto> UpsertBranchSettingAsync(long branchId, UpsertSettingRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await EnsureBranchAsync(tenantId, branchId, ct);
        var key = request.SettingKey.Trim();
        var entity = await db.BranchSettings
            .FirstOrDefaultAsync(s => s.BranchId == branchId && s.SettingKey == key, ct);
        if (entity is null)
        {
            entity = new BranchSetting
            {
                BranchId = branchId,
                SettingKey = key,
                SettingValue = request.SettingValue,
                IsEncrypted = request.IsEncrypted
            };
            db.BranchSettings.Add(entity);
        }
        else
        {
            entity.SettingValue = request.SettingValue;
            entity.IsEncrypted = request.IsEncrypted;
        }

        await db.SaveChangesAsync(ct);
        return MapBranch(entity);
    }

    private async Task EnsureBranchAsync(long tenantId, long branchId, CancellationToken ct)
    {
        var exists = await db.Branches.AnyAsync(b => b.Id == branchId && b.TenantId == tenantId, ct);
        if (!exists) throw new NotFoundException($"Branch {branchId} not found.");
    }

    private static TenantSettingDto MapTenant(TenantSetting s) => new()
    {
        Id = s.Id,
        TenantId = s.TenantId,
        SettingKey = s.SettingKey,
        SettingValue = s.SettingValue,
        IsEncrypted = s.IsEncrypted
    };

    private static BranchSettingDto MapBranch(BranchSetting s) => new()
    {
        Id = s.Id,
        BranchId = s.BranchId,
        SettingKey = s.SettingKey,
        SettingValue = s.SettingValue,
        IsEncrypted = s.IsEncrypted
    };
}
