using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class SupplierService(PharmacyManagementDbContext db, ICurrentUserService currentUser) : ISupplierService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<SupplierDto>> SearchAsync(PaginationQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.Suppliers.AsNoTracking().Where(s => s.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Name.Contains(s) || x.Code.Contains(s) || (x.CompanyName != null && x.CompanyName.Contains(s)));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "code" => query.SortDesc ? q.OrderByDescending(x => x.Code) : q.OrderBy(x => x.Code),
            "name" => query.SortDesc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
            _ => q.OrderBy(x => x.Code)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<SupplierDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<SupplierDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        if (await db.Suppliers.AnyAsync(s => s.TenantId == tenantId && s.Code == request.Code, ct))
            throw new ConflictException($"Supplier code '{request.Code}' already exists.");

        var entity = new Supplier
        {
            TenantId = tenantId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            CompanyName = request.CompanyName,
            Ntn = request.Ntn,
            Strn = request.Strn,
            DrugLicenseNo = request.DrugLicenseNo,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            CreditLimit = request.CreditLimit,
            PaymentTermsDays = request.PaymentTermsDays,
            IsActive = true
        };
        db.Suppliers.Add(entity);
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<SupplierDto> UpdateAsync(long id, UpdateSupplierRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Supplier {id} not found.");

        entity.Name = request.Name.Trim();
        entity.CompanyName = request.CompanyName;
        entity.Ntn = request.Ntn;
        entity.Strn = request.Strn;
        entity.DrugLicenseNo = request.DrugLicenseNo;
        entity.Phone = request.Phone;
        entity.Email = request.Email;
        entity.Address = request.Address;
        entity.CreditLimit = request.CreditLimit;
        entity.PaymentTermsDays = request.PaymentTermsDays;
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task DeactivateAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Supplier {id} not found.");
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    private static SupplierDto Map(Supplier s) => new()
    {
        Id = s.Id,
        TenantId = s.TenantId,
        Code = s.Code,
        Name = s.Name,
        CompanyName = s.CompanyName,
        Ntn = s.Ntn,
        Strn = s.Strn,
        DrugLicenseNo = s.DrugLicenseNo,
        Phone = s.Phone,
        Email = s.Email,
        Address = s.Address,
        CreditLimit = s.CreditLimit,
        PaymentTermsDays = s.PaymentTermsDays,
        IsActive = s.IsActive
    };
}
