using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Expenses;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class ExpenseCategoryService(PharmacyManagementDbContext db) : IExpenseCategoryService
{
    public async Task<PagedResult<ExpenseCategoryDto>> SearchAsync(ExpenseCategoryQuery query, CancellationToken ct = default)
    {
        var q = db.ExpenseCategories.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(c => c.Name.Contains(s) || c.Code.Contains(s));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.SortDesc ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name),
            "code" => query.SortDesc ? q.OrderByDescending(c => c.Code) : q.OrderBy(c => c.Code),
            _ => q.OrderBy(c => c.Code)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<ExpenseCategoryDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<ExpenseCategoryDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await db.ExpenseCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryRequest request, CancellationToken ct = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.ExpenseCategories.AnyAsync(c => c.Code == code, ct))
            throw new ConflictException($"Expense category code '{code}' already exists.");

        var entity = new ExpenseCategory
        {
            Name = request.Name.Trim(),
            Code = code
        };
        db.ExpenseCategories.Add(entity);
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<ExpenseCategoryDto> UpdateAsync(long id, UpdateExpenseCategoryRequest request, CancellationToken ct = default)
    {
        var entity = await db.ExpenseCategories.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException($"Expense category {id} not found.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.ExpenseCategories.AnyAsync(c => c.Code == code && c.Id != id, ct))
            throw new ConflictException($"Expense category code '{code}' already exists.");

        entity.Name = request.Name.Trim();
        entity.Code = code;
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    private static ExpenseCategoryDto Map(ExpenseCategory c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Code = c.Code
    };
}
