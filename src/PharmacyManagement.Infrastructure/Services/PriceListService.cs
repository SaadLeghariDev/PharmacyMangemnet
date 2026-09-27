using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Pricing;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class PriceListService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IPriceListService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<PriceListDto>> SearchAsync(PriceListQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.PriceLists.AsNoTracking().Where(p => p.TenantId == tenantId);

        if (query.IsActive is bool active) q = q.Where(p => p.IsActive == active);
        if (query.IsDefault is bool isDefault) q = q.Where(p => p.IsDefault == isDefault);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(p => p.Name.Contains(s) || (p.PriceType != null && p.PriceType.Contains(s)));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.SortDesc ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name),
            _ => q.OrderByDescending(p => p.IsDefault).ThenBy(p => p.Name)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<PriceListDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<PriceListDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.PriceLists.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<PriceListDto> CreateAsync(CreatePriceListRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = new PriceList
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            PriceType = string.IsNullOrWhiteSpace(request.PriceType) ? null : request.PriceType.Trim(),
            CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
            IsDefault = request.IsDefault,
            IsActive = request.IsActive
        };

        if (entity.IsDefault)
            await ClearOtherDefaultsAsync(tenantId, excludeId: null, ct);

        db.PriceLists.Add(entity);
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<PriceListDto> UpdateAsync(long id, UpdatePriceListRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.PriceLists
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Price list {id} not found.");

        entity.Name = request.Name.Trim();
        entity.PriceType = string.IsNullOrWhiteSpace(request.PriceType) ? null : request.PriceType.Trim();
        entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
        entity.IsActive = request.IsActive;

        if (request.IsDefault && !entity.IsDefault)
            await ClearOtherDefaultsAsync(tenantId, excludeId: entity.Id, ct);

        entity.IsDefault = request.IsDefault;
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    private async Task ClearOtherDefaultsAsync(long tenantId, long? excludeId, CancellationToken ct)
    {
        var others = await db.PriceLists
            .Where(p => p.TenantId == tenantId && p.IsDefault && (excludeId == null || p.Id != excludeId))
            .ToListAsync(ct);
        foreach (var other in others)
            other.IsDefault = false;
    }

    private static PriceListDto Map(PriceList p) => new()
    {
        Id = p.Id,
        TenantId = p.TenantId,
        Name = p.Name,
        PriceType = p.PriceType,
        CurrencyCode = p.CurrencyCode,
        IsDefault = p.IsDefault,
        IsActive = p.IsActive
    };
}
