using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Finance;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class JournalEntryService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    INumberSequenceService sequences) : IJournalEntryService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    private long RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedAppException("User scope required.");

    public async Task<PagedResult<JournalEntryDto>> SearchAsync(JournalEntryQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.JournalEntries.AsNoTracking()
            .Include(j => j.Branch)
            .Include(j => j.JournalLines)
            .Where(j => j.TenantId == tenantId);

        if (query.BranchId is long branchId) q = q.Where(j => j.BranchId == branchId);
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = JournalEntryStatuses.IsKnown(query.Status)
                ? JournalEntryStatuses.Normalize(query.Status)
                : query.Status.Trim();
            q = q.Where(j => j.Status == status);
        }
        if (query.FromDate is DateTime from) q = q.Where(j => j.EntryDate >= from);
        if (query.ToDate is DateTime to) q = q.Where(j => j.EntryDate <= to);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(j =>
                j.EntryNumber.Contains(s) ||
                (j.Description != null && j.Description.Contains(s)) ||
                (j.ReferenceType != null && j.ReferenceType.Contains(s)));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "number" => query.SortDesc ? q.OrderByDescending(j => j.EntryNumber) : q.OrderBy(j => j.EntryNumber),
            "status" => query.SortDesc ? q.OrderByDescending(j => j.Status) : q.OrderBy(j => j.Status),
            "date" => query.SortDesc ? q.OrderByDescending(j => j.EntryDate) : q.OrderBy(j => j.EntryDate),
            _ => q.OrderByDescending(j => j.EntryDate).ThenByDescending(j => j.Id)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<JournalEntryDto>
        {
            Items = items.Select(j => Map(j, includeLines: false)).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<JournalEntryDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.JournalEntries.AsNoTracking()
            .Include(j => j.Branch)
            .Include(j => j.ReversalOfEntry)
            .Include(j => j.JournalLines).ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(j => j.Id == id && j.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity, includeLines: true);
    }

    public async Task<JournalEntryDto> CreateDraftAsync(CreateJournalEntryRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();

        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var branch = await db.Branches.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == tenantId && b.IsActive, ct)
                ?? throw new ValidationAppException(["Branch not found."]);

            var normalizedLines = await ValidateAndNormalizeLinesAsync(tenantId, request.Lines, ct);

            var entryNumber = await sequences.AllocateNextAsync(
                tenantId, DocumentTypes.Journal, branch.Id, null, "JE-", ct);

            var entity = new JournalEntry
            {
                TenantId = tenantId,
                BranchId = branch.Id,
                EntryNumber = entryNumber,
                EntryDate = request.EntryDate,
                ReferenceType = string.IsNullOrWhiteSpace(request.ReferenceType) ? null : request.ReferenceType.Trim(),
                ReferenceId = request.ReferenceId,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Status = JournalEntryStatuses.Draft
            };
            db.JournalEntries.Add(entity);
            await db.SaveChangesAsync(ct);

            var lineNo = 1;
            foreach (var line in normalizedLines)
            {
                db.JournalLines.Add(new JournalLine
                {
                    JournalEntryId = entity.Id,
                    LineNo = lineNo++,
                    AccountId = line.AccountId,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    Description = line.Description
                });
            }
            await db.SaveChangesAsync(ct);

            if (tx is not null) await tx.CommitAsync(ct);
            return (await GetByIdAsync(entity.Id, ct))!;
        }
        catch
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<JournalEntryDto> PostAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var userId = RequireUserId();

        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var entity = await db.JournalEntries
                .Include(j => j.JournalLines)
                .FirstOrDefaultAsync(j => j.Id == id && j.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Journal entry {id} not found.");

            if (entity.Status != JournalEntryStatuses.Draft)
                throw new ValidationAppException([$"Only Draft journals can be posted (current: {entity.Status})."]);

            if (entity.JournalLines.Count < 2)
                throw new ValidationAppException(["Journal must have at least two lines."]);

            EnsureBalanced(entity.JournalLines.Select(l => (l.Debit, l.Credit)));

            entity.Status = JournalEntryStatuses.Posted;
            entity.PostedBy = userId;
            entity.PostedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);

            if (tx is not null) await tx.CommitAsync(ct);
            return (await GetByIdAsync(id, ct))!;
        }
        catch
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<JournalEntryDto> ReverseAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var userId = RequireUserId();
        var now = DateTime.UtcNow;

        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var original = await db.JournalEntries
                .Include(j => j.JournalLines)
                .FirstOrDefaultAsync(j => j.Id == id && j.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Journal entry {id} not found.");

            if (original.Status != JournalEntryStatuses.Posted)
                throw new ValidationAppException([$"Only Posted journals can be reversed (current: {original.Status})."]);

            if (original.JournalLines.Count < 2)
                throw new ValidationAppException(["Journal must have at least two lines."]);

            EnsureBalanced(original.JournalLines.Select(l => (l.Debit, l.Credit)));

            var alreadyReversed = await db.JournalEntries.AnyAsync(
                j => j.ReversalOfEntryId == original.Id && j.TenantId == tenantId, ct);
            if (alreadyReversed)
                throw new ConflictException($"Journal entry {original.EntryNumber} already has a reversal.");

            var entryNumber = await sequences.AllocateNextAsync(
                tenantId, DocumentTypes.Journal, original.BranchId, null, "JE-", ct);

            var reversal = new JournalEntry
            {
                TenantId = tenantId,
                BranchId = original.BranchId,
                EntryNumber = entryNumber,
                EntryDate = now,
                ReferenceType = "JournalReversal",
                ReferenceId = original.Id,
                Description = $"Reversal of {original.EntryNumber}" +
                    (string.IsNullOrWhiteSpace(original.Description) ? "" : $": {original.Description}"),
                Status = JournalEntryStatuses.Posted,
                PostedBy = userId,
                PostedAt = now,
                ReversalOfEntryId = original.Id
            };
            db.JournalEntries.Add(reversal);
            await db.SaveChangesAsync(ct);

            var lineNo = 1;
            foreach (var line in original.JournalLines.OrderBy(l => l.LineNo))
            {
                db.JournalLines.Add(new JournalLine
                {
                    JournalEntryId = reversal.Id,
                    LineNo = lineNo++,
                    AccountId = line.AccountId,
                    Debit = line.Credit,
                    Credit = line.Debit,
                    Description = line.Description
                });
            }

            original.Status = JournalEntryStatuses.Reversed;
            await db.SaveChangesAsync(ct);

            if (tx is not null) await tx.CommitAsync(ct);
            return (await GetByIdAsync(reversal.Id, ct))!;
        }
        catch
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    private async Task<List<(long AccountId, decimal Debit, decimal Credit, string? Description)>> ValidateAndNormalizeLinesAsync(
        long tenantId,
        IReadOnlyList<CreateJournalLineRequest> lines,
        CancellationToken ct)
    {
        if (lines.Count < 2)
            throw new ValidationAppException(["At least two journal lines are required."]);

        var normalized = new List<(long AccountId, decimal Debit, decimal Credit, string? Description)>();
        foreach (var line in lines)
        {
            var debit = Math.Round(line.Debit, 4);
            var credit = Math.Round(line.Credit, 4);

            if (debit < 0 || credit < 0)
                throw new ValidationAppException(["Debit and credit must be non-negative."]);
            if (!((debit > 0 && credit == 0) || (credit > 0 && debit == 0)))
                throw new ValidationAppException(["Each line must have either a debit or a credit (not both, not neither)."]);

            var account = await db.ChartOfAccounts.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == line.AccountId && a.TenantId == tenantId, ct)
                ?? throw new ValidationAppException([$"Account {line.AccountId} not found."]);
            if (!account.IsActive)
                throw new ValidationAppException([$"Account '{account.Code}' is inactive."]);

            normalized.Add((
                account.Id,
                debit,
                credit,
                string.IsNullOrWhiteSpace(line.Description) ? null : line.Description.Trim()));
        }

        EnsureBalanced(normalized.Select(l => (l.Debit, l.Credit)));
        return normalized;
    }

    internal static void EnsureBalanced(IEnumerable<(decimal Debit, decimal Credit)> lines)
    {
        var totalDebit = Math.Round(lines.Sum(l => l.Debit), 4);
        var totalCredit = Math.Round(lines.Sum(l => l.Credit), 4);
        if (totalDebit <= 0 || totalCredit <= 0)
            throw new ValidationAppException(["Journal totals must be greater than zero."]);
        if (totalDebit != totalCredit)
            throw new ValidationAppException([
                $"Journal is not balanced. Debit {totalDebit:0.####} does not equal Credit {totalCredit:0.####}."]);
    }

    private static JournalEntryDto Map(JournalEntry j, bool includeLines)
    {
        var lines = j.JournalLines?.OrderBy(l => l.LineNo).ToList() ?? [];
        return new JournalEntryDto
        {
            Id = j.Id,
            TenantId = j.TenantId,
            BranchId = j.BranchId,
            BranchName = j.Branch?.Name,
            EntryNumber = j.EntryNumber,
            EntryDate = j.EntryDate,
            ReferenceType = j.ReferenceType,
            ReferenceId = j.ReferenceId,
            Description = j.Description,
            Status = j.Status,
            PostedBy = j.PostedBy,
            PostedAt = j.PostedAt,
            ReversalOfEntryId = j.ReversalOfEntryId,
            ReversalOfEntryNumber = j.ReversalOfEntry?.EntryNumber,
            TotalDebit = lines.Sum(l => l.Debit),
            TotalCredit = lines.Sum(l => l.Credit),
            Lines = includeLines
                ? lines.Select(l => new JournalLineDto
                {
                    Id = l.Id,
                    LineNo = l.LineNo,
                    AccountId = l.AccountId,
                    AccountCode = l.Account?.Code,
                    AccountName = l.Account?.Name,
                    Debit = l.Debit,
                    Credit = l.Credit,
                    Description = l.Description
                }).ToList()
                : Array.Empty<JournalLineDto>()
        };
    }
}
