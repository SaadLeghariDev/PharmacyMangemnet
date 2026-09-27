using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Finance;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class ChartOfAccountService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IChartOfAccountService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<ChartOfAccountDto>> SearchAsync(ChartOfAccountQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.ChartOfAccounts.AsNoTracking()
            .Include(a => a.AccountType)
            .Include(a => a.ParentAccount)
            .Where(a => a.TenantId == tenantId);

        if (query.AccountTypeId is long typeId) q = q.Where(a => a.AccountTypeId == typeId);
        if (query.ParentAccountId is long parentId) q = q.Where(a => a.ParentAccountId == parentId);
        if (query.IsActive is bool active) q = q.Where(a => a.IsActive == active);
        if (query.IsSystemAccount is bool system) q = q.Where(a => a.IsSystemAccount == system);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(a => a.Code.Contains(s) || a.Name.Contains(s));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.SortDesc ? q.OrderByDescending(a => a.Name) : q.OrderBy(a => a.Name),
            "type" => query.SortDesc ? q.OrderByDescending(a => a.AccountType.Name) : q.OrderBy(a => a.AccountType.Name),
            _ => q.OrderBy(a => a.Code)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<ChartOfAccountDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<ChartOfAccountDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.ChartOfAccounts.AsNoTracking()
            .Include(a => a.AccountType)
            .Include(a => a.ParentAccount)
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<ChartOfAccountDto> CreateAsync(CreateChartOfAccountRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var code = request.Code.Trim().ToUpperInvariant();

        if (!await db.AccountTypes.AnyAsync(t => t.Id == request.AccountTypeId, ct))
            throw new ValidationAppException(["Account type not found."]);

        if (await db.ChartOfAccounts.AnyAsync(a => a.TenantId == tenantId && a.Code == code, ct))
            throw new ConflictException($"Account code '{code}' already exists.");

        if (request.ParentAccountId is long parentId)
        {
            var parent = await db.ChartOfAccounts.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == parentId && a.TenantId == tenantId, ct)
                ?? throw new ValidationAppException(["Parent account not found."]);
            if (!parent.IsActive)
                throw new ValidationAppException(["Parent account is inactive."]);
        }

        var entity = new ChartOfAccount
        {
            TenantId = tenantId,
            ParentAccountId = request.ParentAccountId,
            Code = code,
            Name = request.Name.Trim(),
            AccountTypeId = request.AccountTypeId,
            IsSystemAccount = false,
            IsActive = request.IsActive
        };
        db.ChartOfAccounts.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<ChartOfAccountDto> UpdateAsync(long id, UpdateChartOfAccountRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.ChartOfAccounts.FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Chart of account {id} not found.");

        if (!await db.AccountTypes.AnyAsync(t => t.Id == request.AccountTypeId, ct))
            throw new ValidationAppException(["Account type not found."]);

        if (request.ParentAccountId is long parentId)
        {
            if (parentId == id)
                throw new ValidationAppException(["An account cannot be its own parent."]);
            var parent = await db.ChartOfAccounts.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == parentId && a.TenantId == tenantId, ct)
                ?? throw new ValidationAppException(["Parent account not found."]);
            if (!parent.IsActive)
                throw new ValidationAppException(["Parent account is inactive."]);
        }

        if (entity.IsSystemAccount && request.AccountTypeId != entity.AccountTypeId)
            throw new ValidationAppException(["System account type cannot be changed."]);

        entity.Name = request.Name.Trim();
        entity.AccountTypeId = request.AccountTypeId;
        entity.ParentAccountId = request.ParentAccountId;
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<ChartOfAccountDto> DeactivateAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.ChartOfAccounts.FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Chart of account {id} not found.");

        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(id, ct))!;
    }

    private static ChartOfAccountDto Map(ChartOfAccount a) => new()
    {
        Id = a.Id,
        TenantId = a.TenantId,
        ParentAccountId = a.ParentAccountId,
        ParentAccountCode = a.ParentAccount?.Code,
        ParentAccountName = a.ParentAccount?.Name,
        Code = a.Code,
        Name = a.Name,
        AccountTypeId = a.AccountTypeId,
        AccountTypeName = a.AccountType?.Name,
        IsSystemAccount = a.IsSystemAccount,
        IsActive = a.IsActive
    };
}
