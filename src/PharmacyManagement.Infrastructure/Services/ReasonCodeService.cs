using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Admin;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class ReasonCodeService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IReasonCodeService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<ReasonCodeDto>> SearchAsync(ReasonCodeQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.ReasonCodes.AsNoTracking().Where(r => r.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(query.ReasonType))
        {
            var type = query.ReasonType.Trim();
            q = q.Where(r => r.ReasonType == type);
        }

        if (query.IsActive is bool active) q = q.Where(r => r.IsActive == active);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(r => r.Code.Contains(s) || r.Name.Contains(s));
        }

        q = q.OrderBy(r => r.ReasonType).ThenBy(r => r.Code);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<ReasonCodeDto>
        {
            Items = rows.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<ReasonCodeDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.ReasonCodes.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<ReasonCodeDto> CreateAsync(CreateReasonCodeRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        if (!ReasonTypes.IsKnown(request.ReasonType))
            throw new ValidationAppException([$"ReasonType must be one of: {ReasonTypes.Return}, {ReasonTypes.Adjustment}, {ReasonTypes.Count}, {ReasonTypes.Void}, {ReasonTypes.Other}."]);

        var reasonType = ReasonTypes.Normalize(request.ReasonType);
        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.ReasonCodes.AnyAsync(r => r.TenantId == tenantId && r.ReasonType == reasonType && r.Code == code, ct))
            throw new ConflictException($"Reason code '{code}' already exists for type '{reasonType}'.");

        var entity = new ReasonCode
        {
            TenantId = tenantId,
            ReasonType = reasonType,
            Code = code,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = request.IsActive
        };
        db.ReasonCodes.Add(entity);
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<ReasonCodeDto> UpdateAsync(long id, UpdateReasonCodeRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.ReasonCodes.FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Reason code {id} not found.");

        entity.Name = request.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    private static ReasonCodeDto Map(ReasonCode r) => new()
    {
        Id = r.Id,
        TenantId = r.TenantId,
        ReasonType = r.ReasonType,
        Code = r.Code,
        Name = r.Name,
        Description = r.Description,
        IsActive = r.IsActive
    };
}
