using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Procurement;
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

    public async Task<PagedResult<SupplierLedgerEntryDto>> GetLedgerAsync(
        long supplierId, SupplierLedgerQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        if (!await db.Suppliers.AnyAsync(s => s.Id == supplierId && s.TenantId == tenantId, ct))
            throw new NotFoundException($"Supplier {supplierId} not found.");

        var q = db.SupplierLedgers.AsNoTracking().Where(l => l.SupplierId == supplierId);
        if (query.BranchId is long branchId) q = q.Where(l => l.BranchId == branchId);
        if (query.FromDate is DateTime from) q = q.Where(l => l.TransactionDate >= from);
        if (query.ToDate is DateTime to) q = q.Where(l => l.TransactionDate <= to);

        // Ascending for running balance; page over that ordered set.
        var ordered = await q.OrderBy(l => l.SequenceNo).ThenBy(l => l.Id).ToListAsync(ct);
        decimal running = 0;
        var withBalance = ordered.Select(l =>
        {
            running = Math.Round(running + l.Debit - l.Credit, 4);
            return new SupplierLedgerEntryDto
            {
                Id = l.Id,
                SupplierId = l.SupplierId,
                BranchId = l.BranchId,
                TransactionDate = l.TransactionDate,
                TransactionType = l.TransactionType,
                ReferenceType = l.ReferenceType,
                ReferenceId = l.ReferenceId,
                Debit = l.Debit,
                Credit = l.Credit,
                SequenceNo = l.SequenceNo,
                Remarks = l.Remarks,
                RunningBalance = running
            };
        }).ToList();

        // Newest first for display (matches customer ledger UX).
        withBalance.Reverse();
        var total = withBalance.Count;
        var pageItems = withBalance
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return new PagedResult<SupplierLedgerEntryDto>
        {
            Items = pageItems,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
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
