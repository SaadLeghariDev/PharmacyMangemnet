using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Controlled;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class ControlledDrugService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    INumberSequenceService sequences) : IControlledDrugService
{
    private static readonly HashSet<string> ManualTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Receipt", "Adjustment", "Destruction", "TransferIn", "TransferOut"
    };

    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    private long? CurrentUserId() => currentUser.UserId;

    public async Task<PagedResult<ControlledRegisterDto>> SearchAsync(
        ControlledRegisterQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.ControlledDrugRegisters.AsNoTracking()
            .Include(r => r.Product)
            .Include(r => r.Branch)
            .Where(r => r.Branch.TenantId == tenantId);

        if (query.BranchId is long branchId) q = q.Where(r => r.BranchId == branchId);
        if (query.ProductId is long productId) q = q.Where(r => r.ProductId == productId);
        if (query.IsActive is bool active) q = q.Where(r => r.IsActive == active);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(r =>
                r.RegisterNumber.Contains(s) ||
                r.Product.Sku.Contains(s) ||
                r.Product.Name.Contains(s));
        }

        q = q.OrderBy(r => r.RegisterNumber);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<ControlledRegisterDto>
        {
            Items = items.Select(r => Map(r)).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<ControlledRegisterDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.ControlledDrugRegisters.AsNoTracking()
            .Include(r => r.Product)
            .Include(r => r.Branch)
            .FirstOrDefaultAsync(r => r.Id == id && r.Branch.TenantId == tenantId, ct);
        if (entity is null) return null;

        var recent = await db.ControlledDrugTransactions.AsNoTracking()
            .Where(t => t.RegisterId == id)
            .OrderByDescending(t => t.Id)
            .Take(20)
            .ToListAsync(ct);

        return Map(entity, recent);
    }

    public async Task<ControlledRegisterDto> OpenAsync(OpenControlledRegisterRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();

        var branch = await db.Branches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == tenantId && b.IsActive, ct)
            ?? throw new ValidationAppException(["Branch not found."]);

        var product = await db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.TenantId == tenantId && p.IsActive, ct)
            ?? throw new ValidationAppException(["Product not found."]);

        if (!product.IsControlled)
            throw new ValidationAppException(["Product is not marked IsControlled."]);

        if (await db.ControlledDrugRegisters.AnyAsync(r =>
                r.BranchId == branch.Id && r.ProductId == product.Id && r.IsActive, ct))
            throw new ConflictException("An active controlled register already exists for this branch/product.");

        string number;
        if (!string.IsNullOrWhiteSpace(request.RegisterNumber))
        {
            number = request.RegisterNumber.Trim();
            if (await db.ControlledDrugRegisters.AnyAsync(r =>
                    r.BranchId == branch.Id && r.RegisterNumber == number, ct))
                throw new ConflictException($"Register number '{number}' already exists for branch.");
        }
        else
        {
            number = await sequences.AllocateNextAsync(
                tenantId, DocumentTypes.ControlledRegister, branch.Id, null, "CDR-", ct);
        }

        var opening = Math.Round(request.OpeningBalance, 6);
        var entity = new ControlledDrugRegister
        {
            BranchId = branch.Id,
            ProductId = product.Id,
            RegisterNumber = number,
            OpeningBalance = opening,
            CurrentBalance = opening,
            IsActive = true
        };
        db.ControlledDrugRegisters.Add(entity);

        if (opening > 0)
        {
            db.ControlledDrugTransactions.Add(new ControlledDrugTransaction
            {
                Register = entity,
                TransactionType = "Receipt",
                ReferenceType = "Opening",
                Quantity = opening,
                BalanceBefore = 0,
                BalanceAfter = opening,
                PerformedBy = CurrentUserId(),
                TransactionDate = DateTime.UtcNow,
                Remarks = "Opening balance"
            });
        }

        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<PagedResult<ControlledTransactionDto>> GetTransactionsAsync(
        long registerId, ControlledTransactionQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        if (!await db.ControlledDrugRegisters.AnyAsync(r =>
                r.Id == registerId && r.Branch.TenantId == tenantId, ct))
            throw new NotFoundException($"Controlled register {registerId} not found.");

        var q = db.ControlledDrugTransactions.AsNoTracking()
            .Where(t => t.RegisterId == registerId)
            .OrderByDescending(t => t.Id);

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<ControlledTransactionDto>
        {
            Items = items.Select(MapTxn).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<ControlledTransactionDto> PostTransactionAsync(
        long registerId, PostControlledTransactionRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        if (!ManualTypes.Contains(request.TransactionType))
            throw new ValidationAppException(["Dispense/Return must be posted via prescription dispense or sale void."]);

        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var register = await db.ControlledDrugRegisters
                .Include(r => r.Branch)
                .FirstOrDefaultAsync(r => r.Id == registerId && r.Branch.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Controlled register {registerId} not found.");

            if (!register.IsActive)
                throw new ValidationAppException(["Register is inactive."]);

            var qty = Math.Round(request.Quantity, 6);
            var before = register.CurrentBalance;
            var after = ApplyBalance(request.TransactionType, before, qty);

            if (after < 0)
                throw new ValidationAppException([
                    $"Transaction would make balance negative (before {before}, qty {qty})."]);

            register.CurrentBalance = after;
            var entity = new ControlledDrugTransaction
            {
                RegisterId = register.Id,
                TransactionType = NormalizeType(request.TransactionType),
                Quantity = qty,
                BalanceBefore = before,
                BalanceAfter = after,
                PerformedBy = CurrentUserId(),
                WitnessedBy = request.WitnessedBy,
                TransactionDate = DateTime.UtcNow,
                Remarks = request.Remarks
            };
            db.ControlledDrugTransactions.Add(entity);
            await db.SaveChangesAsync(ct);
            if (tx is not null) await tx.CommitAsync(ct);
            return MapTxn(entity);
        }
        catch
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    private static decimal ApplyBalance(string type, decimal before, decimal qty) =>
        type.ToLowerInvariant() switch
        {
            "receipt" or "transferin" => Math.Round(before + qty, 6),
            "destruction" or "transferout" => Math.Round(before - qty, 6),
            "adjustment" => Math.Round(before + qty, 6), // positive qty increases; use Destruction for decreases
            _ => throw new ValidationAppException(["Unsupported transaction type."])
        };

    private static string NormalizeType(string type) =>
        type.ToLowerInvariant() switch
        {
            "receipt" => "Receipt",
            "adjustment" => "Adjustment",
            "destruction" => "Destruction",
            "transferin" => "TransferIn",
            "transferout" => "TransferOut",
            _ => type
        };

    private static ControlledRegisterDto Map(
        ControlledDrugRegister r,
        IEnumerable<ControlledDrugTransaction>? recent = null) => new()
    {
        Id = r.Id,
        BranchId = r.BranchId,
        ProductId = r.ProductId,
        Sku = r.Product?.Sku,
        ProductName = r.Product?.Name,
        RegisterNumber = r.RegisterNumber,
        OpeningBalance = r.OpeningBalance,
        CurrentBalance = r.CurrentBalance,
        IsActive = r.IsActive,
        RecentTransactions = recent?.Select(MapTxn).ToList() ?? []
    };

    private static ControlledTransactionDto MapTxn(ControlledDrugTransaction t) => new()
    {
        Id = t.Id,
        RegisterId = t.RegisterId,
        TransactionType = t.TransactionType,
        ReferenceType = t.ReferenceType,
        ReferenceId = t.ReferenceId,
        Quantity = t.Quantity,
        BalanceBefore = t.BalanceBefore,
        BalanceAfter = t.BalanceAfter,
        PrescriptionId = t.PrescriptionId,
        DoctorId = t.DoctorId,
        PerformedBy = t.PerformedBy,
        WitnessedBy = t.WitnessedBy,
        TransactionDate = t.TransactionDate,
        Remarks = t.Remarks
    };
}
