using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class NumberSequenceService(PharmacyManagementDbContext db) : INumberSequenceService
{
    public async Task<string> AllocateNextAsync(
        long tenantId,
        string documentType,
        long? branchId,
        long? terminalId = null,
        string? defaultPrefix = null,
        CancellationToken ct = default)
    {
        await EnsureSequenceExistsAsync(tenantId, documentType, branchId, terminalId, defaultPrefix, ct);

        long next;
        if (db.Database.IsSqlServer())
        {
            next = await AllocateSqlServerAsync(tenantId, documentType, branchId, terminalId, ct);
        }
        else
        {
            next = await AllocateInMemoryAsync(tenantId, documentType, branchId, terminalId, ct);
        }

        var seq = await db.NumberSequences.AsNoTracking()
            .FirstAsync(s =>
                s.TenantId == tenantId &&
                s.DocumentType == documentType &&
                s.BranchId == branchId &&
                s.TerminalId == terminalId, ct);

        var prefix = string.IsNullOrWhiteSpace(seq.Prefix) ? documentType + "-" : seq.Prefix;
        if (!prefix.EndsWith('-') && !prefix.EndsWith('/'))
            prefix += "-";
        return prefix + next.ToString().PadLeft(seq.NumberLength, '0');
    }

    private async Task EnsureSequenceExistsAsync(
        long tenantId,
        string documentType,
        long? branchId,
        long? terminalId,
        string? defaultPrefix,
        CancellationToken ct)
    {
        var exists = await db.NumberSequences.AnyAsync(s =>
            s.TenantId == tenantId &&
            s.DocumentType == documentType &&
            s.BranchId == branchId &&
            s.TerminalId == terminalId, ct);
        if (exists) return;

        db.NumberSequences.Add(new NumberSequence
        {
            TenantId = tenantId,
            BranchId = branchId,
            TerminalId = terminalId,
            DocumentType = documentType,
            Prefix = defaultPrefix ?? documentType + "-",
            CurrentNumber = 0,
            NumberLength = 6,
            ResetPeriod = null
        });
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Concurrent insert of the same sequence key — continue to allocate.
            db.ChangeTracker.Clear();
        }
    }

    private async Task<long> AllocateSqlServerAsync(
        long tenantId,
        string documentType,
        long? branchId,
        long? terminalId,
        CancellationToken ct)
    {
        // Atomic increment with exclusive row lock — never MAX(Id)+1.
        var sql = """
            UPDATE dbo.NumberSequences WITH (UPDLOCK, ROWLOCK)
            SET CurrentNumber = CurrentNumber + 1
            OUTPUT inserted.CurrentNumber
            WHERE TenantId = @tenantId
              AND DocumentType = @documentType
              AND ((@branchId IS NULL AND BranchId IS NULL) OR BranchId = @branchId)
              AND ((@terminalId IS NULL AND TerminalId IS NULL) OR TerminalId = @terminalId);
            """;

        var pTenant = new SqlParameter("@tenantId", tenantId);
        var pDoc = new SqlParameter("@documentType", documentType);
        var pBranch = new SqlParameter("@branchId", (object?)branchId ?? DBNull.Value);
        var pTerminal = new SqlParameter("@terminalId", (object?)terminalId ?? DBNull.Value);

        var numbers = await db.Database
            .SqlQueryRaw<long>(sql, pTenant, pDoc, pBranch, pTerminal)
            .ToListAsync(ct);

        if (numbers.Count == 0)
            throw new ConflictException($"Number sequence '{documentType}' not found for tenant {tenantId}.");

        return numbers[0];
    }

    private async Task<long> AllocateInMemoryAsync(
        long tenantId,
        string documentType,
        long? branchId,
        long? terminalId,
        CancellationToken ct)
    {
        var seq = await db.NumberSequences.FirstOrDefaultAsync(s =>
            s.TenantId == tenantId &&
            s.DocumentType == documentType &&
            s.BranchId == branchId &&
            s.TerminalId == terminalId, ct)
            ?? throw new ConflictException($"Number sequence '{documentType}' not found for tenant {tenantId}.");

        seq.CurrentNumber += 1;
        await db.SaveChangesAsync(ct);
        return seq.CurrentNumber;
    }
}
