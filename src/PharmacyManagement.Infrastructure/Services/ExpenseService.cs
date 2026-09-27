using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Expenses;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class ExpenseService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    INumberSequenceService sequences) : IExpenseService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    private long RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedAppException("User scope required.");

    public async Task<PagedResult<ExpenseDto>> SearchAsync(ExpenseQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.Expenses.AsNoTracking()
            .Include(e => e.Branch)
            .Include(e => e.Category)
            .Include(e => e.PaymentMethod)
            .Where(e => e.Branch.TenantId == tenantId);

        if (query.BranchId is long branchId) q = q.Where(e => e.BranchId == branchId);
        if (query.CategoryId is long categoryId) q = q.Where(e => e.CategoryId == categoryId);
        if (query.PaymentMethodId is long paymentMethodId) q = q.Where(e => e.PaymentMethodId == paymentMethodId);
        if (query.FromDate is DateTime from) q = q.Where(e => e.ExpenseDate >= from);
        if (query.ToDate is DateTime to) q = q.Where(e => e.ExpenseDate <= to);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(e =>
                e.ExpenseNumber.Contains(s) ||
                (e.Description != null && e.Description.Contains(s)) ||
                e.Category.Name.Contains(s) ||
                e.Category.Code.Contains(s));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "amount" => query.SortDesc ? q.OrderByDescending(e => e.Amount) : q.OrderBy(e => e.Amount),
            "number" => query.SortDesc ? q.OrderByDescending(e => e.ExpenseNumber) : q.OrderBy(e => e.ExpenseNumber),
            "date" => query.SortDesc ? q.OrderByDescending(e => e.ExpenseDate) : q.OrderBy(e => e.ExpenseDate),
            _ => q.OrderByDescending(e => e.ExpenseDate).ThenByDescending(e => e.Id)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<ExpenseDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<ExpenseDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Expenses.AsNoTracking()
            .Include(e => e.Branch)
            .Include(e => e.Category)
            .Include(e => e.PaymentMethod)
            .FirstOrDefaultAsync(e => e.Id == id && e.Branch.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var userId = RequireUserId();
        var now = DateTime.UtcNow;

        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var branch = await db.Branches.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == tenantId && b.IsActive, ct)
                ?? throw new ValidationAppException(["Branch not found."]);

            var category = await db.ExpenseCategories.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct)
                ?? throw new ValidationAppException(["Expense category not found."]);

            var paymentMethod = await db.PaymentMethods.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.PaymentMethodId && p.IsActive, ct)
                ?? throw new ValidationAppException(["Payment method not found."]);

            var isCash = IsCashPaymentMethod(paymentMethod);
            if (isCash && (request.TerminalId is null or <= 0))
                throw new ValidationAppException(["Terminal is required for cash expenses."]);

            CashShift? openShift = null;
            if (isCash)
            {
                var terminalId = request.TerminalId!.Value;
                if (!await db.Posterminals.AnyAsync(t =>
                        t.Id == terminalId && t.BranchId == branch.Id && t.IsActive, ct))
                    throw new ValidationAppException(["POS terminal not found for branch."]);

                openShift = await db.CashShifts
                    .FirstOrDefaultAsync(s => s.TerminalId == terminalId && s.Status == "Open", ct)
                    ?? throw new ValidationAppException(["No open cash shift for the selected terminal."]);
            }

            var expenseNumber = await sequences.AllocateNextAsync(
                tenantId, DocumentTypes.Expense, branch.Id, null, "EXP-", ct);

            var amount = Math.Round(request.Amount, 4);
            var entity = new Expense
            {
                BranchId = branch.Id,
                CategoryId = category.Id,
                ExpenseNumber = expenseNumber,
                ExpenseDate = request.ExpenseDate,
                Amount = amount,
                PaymentMethodId = paymentMethod.Id,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                CreatedBy = userId,
                ApprovedBy = userId
            };
            db.Expenses.Add(entity);
            await db.SaveChangesAsync(ct);

            if (openShift is not null)
            {
                // Match PayOut convention: store positive Amount; TransactionType drives direction.
                db.CashTransactions.Add(new CashTransaction
                {
                    CashShiftId = openShift.Id,
                    TransactionType = "Expense",
                    ReferenceType = "Expense",
                    ReferenceId = entity.Id,
                    Amount = amount,
                    Remarks = string.IsNullOrWhiteSpace(entity.Description)
                        ? entity.ExpenseNumber
                        : $"{entity.ExpenseNumber}: {entity.Description}",
                    CreatedBy = userId,
                    CreatedAt = now
                });
                await db.SaveChangesAsync(ct);
            }

            if (tx is not null) await tx.CommitAsync(ct);

            return (await GetByIdAsync(entity.Id, ct))!;
        }
        catch
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<ExpenseDto> UpdateAsync(long id, UpdateExpenseRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Expenses
            .Include(e => e.Branch)
            .FirstOrDefaultAsync(e => e.Id == id && e.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Expense {id} not found.");

        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(id, ct))!;
    }

    internal static bool IsCashPaymentMethod(PaymentMethod method) =>
        string.Equals(method.Type, "Cash", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(method.Code, "CASH", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(method.Name, "Cash", StringComparison.OrdinalIgnoreCase);

    private static ExpenseDto Map(Expense e) => new()
    {
        Id = e.Id,
        BranchId = e.BranchId,
        BranchName = e.Branch?.Name,
        CategoryId = e.CategoryId,
        CategoryName = e.Category?.Name,
        CategoryCode = e.Category?.Code,
        ExpenseNumber = e.ExpenseNumber,
        ExpenseDate = e.ExpenseDate,
        Amount = e.Amount,
        PaymentMethodId = e.PaymentMethodId,
        PaymentMethodCode = e.PaymentMethod?.Code,
        PaymentMethodName = e.PaymentMethod?.Name,
        PaymentMethodType = e.PaymentMethod?.Type,
        Description = e.Description,
        CreatedBy = e.CreatedBy,
        ApprovedBy = e.ApprovedBy
    };
}
