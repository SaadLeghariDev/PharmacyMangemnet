using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Cash;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class CashShiftService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : ICashShiftService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    private long RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedAppException("User scope required.");

    public async Task<PagedResult<CashShiftDto>> SearchAsync(CashShiftQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.CashShifts.AsNoTracking()
            .Include(s => s.CashTransactions)
            .Where(s => s.Branch.TenantId == tenantId);

        if (query.BranchId is long branchId) q = q.Where(s => s.BranchId == branchId);
        if (query.TerminalId is long terminalId) q = q.Where(s => s.TerminalId == terminalId);
        if (query.UserId is long userId) q = q.Where(s => s.UserId == userId);
        if (!string.IsNullOrWhiteSpace(query.Status)) q = q.Where(s => s.Status == query.Status);

        q = q.OrderByDescending(s => s.Id);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<CashShiftDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<CashShiftDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.CashShifts.AsNoTracking()
            .Include(s => s.CashTransactions)
            .FirstOrDefaultAsync(s => s.Id == id && s.Branch.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<CashShiftDto?> GetCurrentAsync(long terminalId, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.CashShifts.AsNoTracking()
            .Include(s => s.CashTransactions)
            .Where(s => s.TerminalId == terminalId && s.Status == "Open" && s.Branch.TenantId == tenantId)
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync(ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<CashShiftDto> OpenAsync(OpenCashShiftRequest request, CancellationToken ct = default)
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

            if (!await db.Counters.AnyAsync(c => c.Id == request.CounterId && c.BranchId == branch.Id && c.IsActive, ct))
                throw new ValidationAppException(["Counter not found for branch."]);

            if (!await db.Posterminals.AnyAsync(t =>
                    t.Id == request.TerminalId && t.BranchId == branch.Id && t.CounterId == request.CounterId && t.IsActive, ct))
                throw new ValidationAppException(["POS terminal not found for branch/counter."]);

            if (await db.CashShifts.AnyAsync(s => s.TerminalId == request.TerminalId && s.Status == "Open", ct))
                throw new ConflictException("An open cash shift already exists for this terminal.");

            var opening = Math.Round(request.OpeningAmount, 4);
            var shift = new CashShift
            {
                BranchId = request.BranchId,
                CounterId = request.CounterId,
                TerminalId = request.TerminalId,
                UserId = userId,
                OpeningAmount = opening,
                OpeningAt = now,
                Status = "Open"
            };
            db.CashShifts.Add(shift);
            await db.SaveChangesAsync(ct);

            // CashTransactions.Amount CHECK requires <> 0 — skip Opening row when float is zero.
            if (opening > 0)
            {
                db.CashTransactions.Add(new CashTransaction
                {
                    CashShiftId = shift.Id,
                    TransactionType = "Opening",
                    Amount = opening,
                    Remarks = request.Remarks ?? "Opening float",
                    CreatedBy = userId,
                    CreatedAt = now
                });
                await db.SaveChangesAsync(ct);
            }

            if (tx is not null) await tx.CommitAsync(ct);
            return (await GetByIdAsync(shift.Id, ct))!;
        }
        catch
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    public Task<CashShiftDto> PayInAsync(long id, CashDrawerMovementRequest request, CancellationToken ct = default) =>
        AddMovementAsync(id, "PayIn", request, ct);

    public Task<CashShiftDto> PayOutAsync(long id, CashDrawerMovementRequest request, CancellationToken ct = default) =>
        AddMovementAsync(id, "PayOut", request, ct);

    public async Task<CashShiftDto> CloseAsync(long id, CloseCashShiftRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var userId = RequireUserId();
        var now = DateTime.UtcNow;

        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var shift = await db.CashShifts
                .Include(s => s.CashTransactions)
                .Include(s => s.Branch)
                .FirstOrDefaultAsync(s => s.Id == id && s.Branch.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Cash shift {id} not found.");

            if (shift.Status != "Open")
                throw new ValidationAppException([$"Only Open shifts can be closed (current: {shift.Status})."]);

            var expected = ComputeExpected(shift.OpeningAmount, shift.CashTransactions);
            var closing = Math.Round(request.ClosingAmount, 4);
            var variance = Math.Round(closing - expected, 4);

            shift.ClosingAmount = closing;
            shift.ExpectedAmount = expected;
            shift.VarianceAmount = variance;
            shift.ClosingAt = now;
            shift.Status = "Closed";

            if (closing > 0)
            {
                db.CashTransactions.Add(new CashTransaction
                {
                    CashShiftId = shift.Id,
                    TransactionType = "Closing",
                    Amount = closing,
                    Remarks = request.Remarks ?? $"Closed with variance {variance}",
                    CreatedBy = userId,
                    CreatedAt = now
                });
            }

            await db.SaveChangesAsync(ct);
            if (tx is not null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            throw;
        }

        return (await GetByIdAsync(id, ct))!;
    }

    private async Task<CashShiftDto> AddMovementAsync(
        long id, string type, CashDrawerMovementRequest request, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        var userId = RequireUserId();
        var now = DateTime.UtcNow;

        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var shift = await db.CashShifts
                .Include(s => s.Branch)
                .FirstOrDefaultAsync(s => s.Id == id && s.Branch.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Cash shift {id} not found.");

            if (shift.Status != "Open")
                throw new ValidationAppException([$"Cash shift is not open (current: {shift.Status})."]);

            db.CashTransactions.Add(new CashTransaction
            {
                CashShiftId = shift.Id,
                TransactionType = type,
                ReferenceType = request.ReferenceType,
                ReferenceId = request.ReferenceId,
                Amount = Math.Round(request.Amount, 4),
                Remarks = request.Remarks,
                CreatedBy = userId,
                CreatedAt = now
            });

            await db.SaveChangesAsync(ct);
            if (tx is not null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            throw;
        }

        return (await GetByIdAsync(id, ct))!;
    }

    internal static decimal ComputeExpected(decimal openingAmount, IEnumerable<CashTransaction> txns)
    {
        decimal sale = 0, payIn = 0, refund = 0, payOut = 0, expense = 0;
        foreach (var t in txns)
        {
            var amt = Math.Abs(t.Amount);
            switch (t.TransactionType)
            {
                case "Sale": sale += amt; break;
                case "PayIn": payIn += amt; break;
                case "Refund": refund += amt; break;
                case "PayOut": payOut += amt; break;
                case "Expense": expense += amt; break;
            }
        }

        return Math.Round(openingAmount + sale + payIn - refund - payOut - expense, 4);
    }

    private static CashShiftDto Map(CashShift s)
    {
        var txns = s.CashTransactions.OrderBy(t => t.Id).ToList();
        var expected = s.ExpectedAmount ?? ComputeExpected(s.OpeningAmount, txns);
        return new CashShiftDto
        {
            Id = s.Id,
            BranchId = s.BranchId,
            CounterId = s.CounterId,
            TerminalId = s.TerminalId,
            UserId = s.UserId,
            OpeningAmount = s.OpeningAmount,
            OpeningAt = s.OpeningAt,
            ClosingAmount = s.ClosingAmount,
            ExpectedAmount = s.ExpectedAmount ?? expected,
            VarianceAmount = s.VarianceAmount,
            ClosingAt = s.ClosingAt,
            Status = s.Status,
            RunningTotal = expected,
            Transactions = txns.Select(t => new CashTransactionDto
            {
                Id = t.Id,
                CashShiftId = t.CashShiftId,
                TransactionType = t.TransactionType,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                Amount = t.Amount,
                Remarks = t.Remarks,
                CreatedBy = t.CreatedBy,
                CreatedAt = t.CreatedAt
            }).ToList()
        };
    }
}
