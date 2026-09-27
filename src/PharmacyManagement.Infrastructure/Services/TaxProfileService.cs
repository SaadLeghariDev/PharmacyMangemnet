using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Tax;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class TaxProfileService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : ITaxProfileService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<TaxProfileDto>> SearchAsync(TaxProfileQuery query, CancellationToken ct = default)
    {
        _ = RequireTenantId();
        var q = db.TaxProfiles.AsNoTracking().AsQueryable();

        if (query.IsActive is bool active) q = q.Where(t => t.IsActive == active);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(t =>
                t.Name.Contains(s) ||
                (t.TaxType != null && t.TaxType.Contains(s)) ||
                (t.Description != null && t.Description.Contains(s)));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.SortDesc ? q.OrderByDescending(t => t.Name) : q.OrderBy(t => t.Name),
            _ => q.OrderBy(t => t.Name)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<TaxProfileDto>
        {
            Items = items.Select(t => MapProfile(t, includeRates: false)).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<TaxProfileDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        _ = RequireTenantId();
        var entity = await db.TaxProfiles.AsNoTracking()
            .Include(t => t.TaxRates)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        return entity is null ? null : MapProfile(entity, includeRates: true);
    }

    public async Task<TaxProfileDto> CreateAsync(CreateTaxProfileRequest request, CancellationToken ct = default)
    {
        _ = RequireTenantId();
        var entity = new TaxProfile
        {
            Name = request.Name.Trim(),
            TaxType = string.IsNullOrWhiteSpace(request.TaxType) ? null : request.TaxType.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = request.IsActive
        };
        db.TaxProfiles.Add(entity);
        await db.SaveChangesAsync(ct);
        return MapProfile(entity, includeRates: false);
    }

    public async Task<TaxProfileDto> UpdateAsync(long id, UpdateTaxProfileRequest request, CancellationToken ct = default)
    {
        _ = RequireTenantId();
        var entity = await db.TaxProfiles.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException($"Tax profile {id} not found.");

        entity.Name = request.Name.Trim();
        entity.TaxType = string.IsNullOrWhiteSpace(request.TaxType) ? null : request.TaxType.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<IReadOnlyList<TaxRateDto>> GetRatesAsync(long taxProfileId, CancellationToken ct = default)
    {
        _ = RequireTenantId();
        if (!await db.TaxProfiles.AnyAsync(t => t.Id == taxProfileId, ct))
            throw new NotFoundException($"Tax profile {taxProfileId} not found.");

        var rates = await db.TaxRates.AsNoTracking()
            .Where(r => r.TaxProfileId == taxProfileId)
            .OrderByDescending(r => r.EffectiveFrom)
            .ThenByDescending(r => r.Id)
            .ToListAsync(ct);
        return rates.Select(MapRate).ToList();
    }

    public async Task<TaxRateDto> AddRateAsync(long taxProfileId, CreateTaxRateRequest request, CancellationToken ct = default)
    {
        _ = RequireTenantId();
        if (!await db.TaxProfiles.AnyAsync(t => t.Id == taxProfileId, ct))
            throw new NotFoundException($"Tax profile {taxProfileId} not found.");

        var entity = new TaxRate
        {
            TaxProfileId = taxProfileId,
            Rate = Math.Round(request.Rate, 4),
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            IsActive = request.IsActive
        };
        db.TaxRates.Add(entity);
        await db.SaveChangesAsync(ct);
        return MapRate(entity);
    }

    public async Task<TaxRateDto> UpdateRateAsync(
        long taxProfileId, long rateId, UpdateTaxRateRequest request, CancellationToken ct = default)
    {
        _ = RequireTenantId();
        var entity = await db.TaxRates
            .FirstOrDefaultAsync(r => r.Id == rateId && r.TaxProfileId == taxProfileId, ct)
            ?? throw new NotFoundException($"Tax rate {rateId} not found.");

        entity.Rate = Math.Round(request.Rate, 4);
        entity.EffectiveFrom = request.EffectiveFrom;
        entity.EffectiveTo = request.EffectiveTo;
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return MapRate(entity);
    }

    public async Task<IReadOnlyList<ProductTaxProfileDto>> GetProductTaxProfilesAsync(
        long productId, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var product = await db.Products.AsNoTracking()
            .Include(p => p.TaxProfiles)
            .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Product {productId} not found.");

        return product.TaxProfiles
            .OrderBy(t => t.Name)
            .Select(t => new ProductTaxProfileDto
            {
                TaxProfileId = t.Id,
                Name = t.Name,
                TaxType = t.TaxType,
                IsActive = t.IsActive
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ProductTaxProfileDto>> ReplaceProductTaxProfilesAsync(
        long productId, ReplaceProductTaxProfilesRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var product = await db.Products
            .Include(p => p.TaxProfiles)
            .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Product {productId} not found.");

        var ids = request.TaxProfileIds.Distinct().ToList();
        var profiles = await db.TaxProfiles.Where(t => ids.Contains(t.Id)).ToListAsync(ct);
        if (profiles.Count != ids.Count)
            throw new ValidationAppException(["One or more tax profiles were not found."]);

        product.TaxProfiles.Clear();
        foreach (var profile in profiles)
            product.TaxProfiles.Add(profile);

        await db.SaveChangesAsync(ct);
        return await GetProductTaxProfilesAsync(productId, ct);
    }

    private static TaxProfileDto MapProfile(TaxProfile t, bool includeRates) => new()
    {
        Id = t.Id,
        Name = t.Name,
        TaxType = t.TaxType,
        Description = t.Description,
        IsActive = t.IsActive,
        Rates = includeRates
            ? t.TaxRates.OrderByDescending(r => r.EffectiveFrom).Select(MapRate).ToList()
            : Array.Empty<TaxRateDto>()
    };

    private static TaxRateDto MapRate(TaxRate r) => new()
    {
        Id = r.Id,
        TaxProfileId = r.TaxProfileId,
        Rate = r.Rate,
        EffectiveFrom = r.EffectiveFrom,
        EffectiveTo = r.EffectiveTo,
        IsActive = r.IsActive
    };
}
