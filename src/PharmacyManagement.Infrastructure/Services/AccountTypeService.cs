using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Finance;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class AccountTypeService(PharmacyManagementDbContext db) : IAccountTypeService
{
    public async Task<PagedResult<AccountTypeDto>> SearchAsync(AccountTypeQuery query, CancellationToken ct = default)
    {
        var q = db.AccountTypes.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(a => a.Name.Contains(s));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "id" => query.SortDesc ? q.OrderByDescending(a => a.Id) : q.OrderBy(a => a.Id),
            _ => q.OrderBy(a => a.Id)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<AccountTypeDto>
        {
            Items = items.Select(a => new AccountTypeDto { Id = a.Id, Name = a.Name }).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }
}
