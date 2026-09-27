using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Procurement;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class SupplierPaymentService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : ISupplierPaymentService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    private long RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedAppException("User scope required.");

    public async Task<PagedResult<SupplierPaymentDto>> SearchAsync(SupplierPaymentQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.SupplierPayments.AsNoTracking()
            .Include(p => p.Branch)
            .Include(p => p.Supplier)
            .Include(p => p.PaymentMethod)
            .Where(p => p.Branch.TenantId == tenantId && p.Supplier.TenantId == tenantId);

        if (query.SupplierId is long supplierId) q = q.Where(p => p.SupplierId == supplierId);
        if (query.BranchId is long branchId) q = q.Where(p => p.BranchId == branchId);
        if (query.PaymentMethodId is long methodId) q = q.Where(p => p.PaymentMethodId == methodId);
        if (query.FromDate is DateTime from) q = q.Where(p => p.PaymentDate >= from);
        if (query.ToDate is DateTime to) q = q.Where(p => p.PaymentDate <= to);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(p =>
                (p.ReferenceNumber != null && p.ReferenceNumber.Contains(s)) ||
                (p.Remarks != null && p.Remarks.Contains(s)) ||
                p.Supplier.Name.Contains(s) ||
                p.Supplier.Code.Contains(s));
        }

        q = q.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.Id);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<SupplierPaymentDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<SupplierPaymentDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.SupplierPayments.AsNoTracking()
            .Include(p => p.Branch)
            .Include(p => p.Supplier)
            .Include(p => p.PaymentMethod)
            .FirstOrDefaultAsync(p => p.Id == id && p.Branch.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<SupplierPaymentDto> CreateAsync(CreateSupplierPaymentRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var userId = RequireUserId();
        var now = DateTime.UtcNow;

        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var supplier = await db.Suppliers
                .FirstOrDefaultAsync(s => s.Id == request.SupplierId && s.TenantId == tenantId, ct)
                ?? throw new ValidationAppException(["Supplier not found."]);
            if (!supplier.IsActive)
                throw new ValidationAppException(["Supplier is inactive."]);

            var branch = await db.Branches.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == tenantId && b.IsActive, ct)
                ?? throw new ValidationAppException(["Branch not found."]);

            var paymentMethod = await db.PaymentMethods.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.PaymentMethodId && p.IsActive, ct)
                ?? throw new ValidationAppException(["Payment method not found."]);

            var isCash = IsCashPaymentMethod(paymentMethod);
            if (isCash && (request.TerminalId is null or <= 0))
                throw new ValidationAppException(["Terminal is required for cash supplier payments."]);

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

            var amount = Math.Round(request.Amount, 4);
            var entity = new SupplierPayment
            {
                SupplierId = supplier.Id,
                BranchId = branch.Id,
                PaymentMethodId = paymentMethod.Id,
                Amount = amount,
                ReferenceNumber = string.IsNullOrWhiteSpace(request.ReferenceNumber)
                    ? null
                    : request.ReferenceNumber.Trim(),
                PaymentDate = request.PaymentDate,
                Remarks = string.IsNullOrWhiteSpace(request.Remarks) ? null : request.Remarks.Trim(),
                PaidBy = userId
            };
            db.SupplierPayments.Add(entity);
            await db.SaveChangesAsync(ct);

            var seq = await NextLedgerSequenceAsync(supplier.Id, branch.Id, ct);
            db.SupplierLedgers.Add(new SupplierLedger
            {
                SupplierId = supplier.Id,
                BranchId = branch.Id,
                TransactionDate = request.PaymentDate,
                TransactionType = "Payment",
                ReferenceType = "SupplierPayment",
                ReferenceId = entity.Id,
                Debit = 0,
                Credit = amount,
                SequenceNo = seq,
                Remarks = entity.Remarks ?? entity.ReferenceNumber ?? $"Supplier payment {entity.Id}"
            });
            await db.SaveChangesAsync(ct);

            if (openShift is not null)
            {
                db.CashTransactions.Add(new CashTransaction
                {
                    CashShiftId = openShift.Id,
                    TransactionType = "PayOut",
                    ReferenceType = "SupplierPayment",
                    ReferenceId = entity.Id,
                    Amount = amount,
                    Remarks = string.IsNullOrWhiteSpace(entity.Remarks)
                        ? $"Supplier payment #{entity.Id}"
                        : $"Supplier payment #{entity.Id}: {entity.Remarks}",
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

    internal static bool IsCashPaymentMethod(PaymentMethod method) =>
        string.Equals(method.Type, "Cash", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(method.Code, "CASH", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(method.Name, "Cash", StringComparison.OrdinalIgnoreCase);

    private async Task<long> NextLedgerSequenceAsync(long supplierId, long branchId, CancellationToken ct)
    {
        var max = await db.SupplierLedgers
            .Where(l => l.SupplierId == supplierId && l.BranchId == branchId)
            .Select(l => (long?)l.SequenceNo)
            .MaxAsync(ct);
        return (max ?? 0) + 1;
    }

    private static SupplierPaymentDto Map(SupplierPayment p) => new()
    {
        Id = p.Id,
        SupplierId = p.SupplierId,
        SupplierCode = p.Supplier?.Code,
        SupplierName = p.Supplier?.Name,
        BranchId = p.BranchId,
        BranchName = p.Branch?.Name,
        PaymentMethodId = p.PaymentMethodId,
        PaymentMethodCode = p.PaymentMethod?.Code,
        PaymentMethodName = p.PaymentMethod?.Name,
        PaymentMethodType = p.PaymentMethod?.Type,
        Amount = p.Amount,
        ReferenceNumber = p.ReferenceNumber,
        PaymentDate = p.PaymentDate,
        Remarks = p.Remarks,
        PaidBy = p.PaidBy
    };
}
