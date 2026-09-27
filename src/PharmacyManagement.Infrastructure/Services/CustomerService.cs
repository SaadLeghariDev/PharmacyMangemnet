using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Customers;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class CustomerService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    INumberSequenceService sequences) : ICustomerService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    private long? CurrentUserId() => currentUser.UserId;

    public async Task<PagedResult<CustomerDto>> SearchAsync(CustomerQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.Customers.AsNoTracking().Where(c => c.TenantId == tenantId);

        if (query.IsActive is bool active) q = q.Where(c => c.IsActive == active);
        if (query.IsPatient is bool patient) q = q.Where(c => c.IsPatient == patient);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(c =>
                c.Name.Contains(s) ||
                c.CustomerCode.Contains(s) ||
                (c.Phone != null && c.Phone.Contains(s)) ||
                (c.Cnic != null && c.Cnic.Contains(s)));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.SortDesc ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name),
            "code" => query.SortDesc ? q.OrderByDescending(c => c.CustomerCode) : q.OrderBy(c => c.CustomerCode),
            _ => q.OrderBy(c => c.CustomerCode)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        var balances = await LoadBalancesAsync(items.Select(c => c.Id).ToList(), ct);

        return new PagedResult<CustomerDto>
        {
            Items = items.Select(c => Map(c, balances.GetValueOrDefault(c.Id))).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<CustomerDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, ct);
        if (entity is null) return null;
        var balance = await GetBalanceAsync(entity.Id, ct);
        return Map(entity, balance);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var now = DateTime.UtcNow;

        string code;
        if (!string.IsNullOrWhiteSpace(request.CustomerCode))
        {
            code = request.CustomerCode.Trim();
            if (await db.Customers.AnyAsync(c => c.TenantId == tenantId && c.CustomerCode == code, ct))
                throw new ConflictException($"Customer code '{code}' already exists.");
        }
        else
        {
            code = await sequences.AllocateNextAsync(
                tenantId, DocumentTypes.Customer, null, null, "C-", ct);
        }

        var entity = new Customer
        {
            TenantId = tenantId,
            CustomerCode = code,
            Name = request.Name.Trim(),
            Cnic = request.Cnic,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            CreditLimit = Math.Round(request.CreditLimit, 4),
            IsPatient = request.IsPatient,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Customers.Add(entity);
        await db.SaveChangesAsync(ct);
        return Map(entity, 0);
    }

    public async Task<CustomerDto> UpdateAsync(long id, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Customers.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Customer {id} not found.");

        entity.Name = request.Name.Trim();
        entity.Cnic = request.Cnic;
        entity.Phone = request.Phone;
        entity.Email = request.Email;
        entity.Address = request.Address;
        entity.DateOfBirth = request.DateOfBirth;
        entity.Gender = request.Gender;
        entity.CreditLimit = Math.Round(request.CreditLimit, 4);
        entity.IsPatient = request.IsPatient;
        entity.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        var balance = await GetBalanceAsync(entity.Id, ct);
        return Map(entity, balance);
    }

    public async Task DeactivateAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Customers.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Customer {id} not found.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<CustomerLedgerEntryDto>> GetLedgerAsync(
        long customerId, CustomerLedgerQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        if (!await db.Customers.AnyAsync(c => c.Id == customerId && c.TenantId == tenantId, ct))
            throw new NotFoundException($"Customer {customerId} not found.");

        var q = db.CustomerLedgers.AsNoTracking().Where(l => l.CustomerId == customerId);
        if (query.BranchId is long branchId) q = q.Where(l => l.BranchId == branchId);
        if (query.FromDate is DateTime from) q = q.Where(l => l.TransactionDate >= from);
        if (query.ToDate is DateTime to) q = q.Where(l => l.TransactionDate <= to);

        q = q.OrderByDescending(l => l.SequenceNo).ThenByDescending(l => l.Id);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return new PagedResult<CustomerLedgerEntryDto>
        {
            Items = items.Select(l => new CustomerLedgerEntryDto
            {
                Id = l.Id,
                CustomerId = l.CustomerId,
                BranchId = l.BranchId,
                TransactionDate = l.TransactionDate,
                TransactionType = l.TransactionType,
                ReferenceType = l.ReferenceType,
                ReferenceId = l.ReferenceId,
                Debit = l.Debit,
                Credit = l.Credit,
                SequenceNo = l.SequenceNo,
                Remarks = l.Remarks
            }).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<CustomerPaymentDto> RecordPaymentAsync(
        long customerId, RecordCustomerPaymentRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var userId = CurrentUserId();
        var now = request.PaymentDate ?? DateTime.UtcNow;

        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var customer = await db.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId && c.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Customer {customerId} not found.");

            if (!customer.IsActive)
                throw new ValidationAppException(["Customer is inactive."]);

            if (!await db.Branches.AnyAsync(b => b.Id == request.BranchId && b.TenantId == tenantId && b.IsActive, ct))
                throw new ValidationAppException(["Branch not found."]);

            var method = await db.PaymentMethods
                .FirstOrDefaultAsync(m => m.Id == request.PaymentMethodId && m.IsActive, ct)
                ?? throw new ValidationAppException(["Payment method not found."]);

            if (string.Equals(method.Type, "Credit", StringComparison.OrdinalIgnoreCase))
                throw new ValidationAppException(["Cannot record AR payment with Credit payment method."]);

            var amount = Math.Round(request.Amount, 4);
            var payment = new CustomerPayment
            {
                CustomerId = customer.Id,
                BranchId = request.BranchId,
                PaymentMethodId = method.Id,
                Amount = amount,
                ReferenceNumber = request.ReferenceNumber,
                PaymentDate = now,
                Remarks = request.Remarks,
                ReceivedBy = userId
            };
            db.CustomerPayments.Add(payment);
            await db.SaveChangesAsync(ct);

            var seq = await NextLedgerSequenceAsync(customer.Id, ct);
            db.CustomerLedgers.Add(new CustomerLedger
            {
                CustomerId = customer.Id,
                BranchId = request.BranchId,
                TransactionDate = now,
                TransactionType = "Payment",
                ReferenceType = "CustomerPayment",
                ReferenceId = payment.Id,
                Debit = 0,
                Credit = amount,
                SequenceNo = seq,
                Remarks = request.Remarks ?? $"AR payment {payment.Id}"
            });

            await db.SaveChangesAsync(ct);
            if (tx is not null) await tx.CommitAsync(ct);

            return new CustomerPaymentDto
            {
                Id = payment.Id,
                CustomerId = payment.CustomerId,
                BranchId = payment.BranchId,
                PaymentMethodId = payment.PaymentMethodId,
                PaymentMethodCode = method.Code,
                PaymentMethodName = method.Name,
                Amount = payment.Amount,
                ReferenceNumber = payment.ReferenceNumber,
                PaymentDate = payment.PaymentDate,
                Remarks = payment.Remarks,
                ReceivedBy = payment.ReceivedBy
            };
        }
        catch
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    private async Task<decimal> GetBalanceAsync(long customerId, CancellationToken ct)
    {
        var debit = await db.CustomerLedgers.Where(l => l.CustomerId == customerId).SumAsync(l => (decimal?)l.Debit, ct) ?? 0;
        var credit = await db.CustomerLedgers.Where(l => l.CustomerId == customerId).SumAsync(l => (decimal?)l.Credit, ct) ?? 0;
        return Math.Round(debit - credit, 4);
    }

    private async Task<Dictionary<long, decimal>> LoadBalancesAsync(IReadOnlyList<long> customerIds, CancellationToken ct)
    {
        if (customerIds.Count == 0) return new Dictionary<long, decimal>();
        var rows = await db.CustomerLedgers.AsNoTracking()
            .Where(l => customerIds.Contains(l.CustomerId))
            .GroupBy(l => l.CustomerId)
            .Select(g => new { CustomerId = g.Key, Balance = g.Sum(x => x.Debit - x.Credit) })
            .ToListAsync(ct);
        return rows.ToDictionary(r => r.CustomerId, r => Math.Round(r.Balance, 4));
    }

    private async Task<long> NextLedgerSequenceAsync(long customerId, CancellationToken ct)
    {
        var max = await db.CustomerLedgers
            .Where(l => l.CustomerId == customerId)
            .Select(l => (long?)l.SequenceNo)
            .MaxAsync(ct);
        return (max ?? 0) + 1;
    }

    private static CustomerDto Map(Customer c, decimal balance) => new()
    {
        Id = c.Id,
        TenantId = c.TenantId,
        CustomerCode = c.CustomerCode,
        Name = c.Name,
        Cnic = c.Cnic,
        Phone = c.Phone,
        Email = c.Email,
        Address = c.Address,
        DateOfBirth = c.DateOfBirth,
        Gender = c.Gender,
        CreditLimit = c.CreditLimit,
        IsPatient = c.IsPatient,
        IsActive = c.IsActive,
        Balance = balance,
        AvailableCredit = Math.Round(Math.Max(0, c.CreditLimit - balance), 4),
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}
