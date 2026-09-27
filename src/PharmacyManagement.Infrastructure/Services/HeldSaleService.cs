using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class HeldSaleService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IHeldSaleService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    private long RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedAppException("User scope required.");

    public async Task<PagedResult<HeldSaleDto>> SearchAsync(HeldSaleQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.HeldSales.AsNoTracking()
            .Include(h => h.Branch)
            .Where(h => h.Branch.TenantId == tenantId);

        if (query.BranchId is long branchId) q = q.Where(h => h.BranchId == branchId);
        if (query.TerminalId is long terminalId) q = q.Where(h => h.TerminalId == terminalId);
        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(h => h.Status == query.Status);
        else
            q = q.Where(h => h.Status == "Held");

        q = q.OrderByDescending(h => h.HeldAt);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<HeldSaleDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<HeldSaleDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.HeldSales.AsNoTracking()
            .Include(h => h.Branch)
            .FirstOrDefaultAsync(h => h.Id == id && h.Branch.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<HeldSaleDto> HoldAsync(HoldSaleRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var userId = RequireUserId();

        var branch = await db.Branches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == tenantId && b.IsActive, ct)
            ?? throw new ValidationAppException(["Branch not found."]);

        if (!await db.Posterminals.AnyAsync(t => t.Id == request.TerminalId && t.BranchId == branch.Id && t.IsActive, ct))
            throw new ValidationAppException(["POS terminal not found for branch."]);

        if (request.CustomerId is long customerId)
        {
            if (!await db.Customers.AnyAsync(c => c.Id == customerId && c.TenantId == tenantId && c.IsActive, ct))
                throw new ValidationAppException(["Customer not found."]);
        }

        var now = DateTime.UtcNow;
        var entity = new HeldSale
        {
            BranchId = request.BranchId,
            TerminalId = request.TerminalId,
            UserId = userId,
            CustomerId = request.CustomerId,
            CartData = request.CartData,
            TotalAmount = Math.Round(request.TotalAmount, 4),
            HeldAt = now,
            ExpiresAt = request.ExpiresAt ?? now.AddHours(4),
            Status = "Held"
        };

        db.HeldSales.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<HeldSaleDto> ResumeAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.HeldSales
            .Include(h => h.Branch)
            .FirstOrDefaultAsync(h => h.Id == id && h.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Held sale {id} not found.");

        if (entity.Status != "Held")
            throw new ValidationAppException([$"Only Held carts can be resumed (current: {entity.Status})."]);

        if (entity.ExpiresAt is DateTime exp && exp < DateTime.UtcNow)
        {
            entity.Status = "Expired";
            await db.SaveChangesAsync(ct);
            throw new ValidationAppException(["Held sale has expired."]);
        }

        entity.Status = "Resumed";
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task DiscardAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.HeldSales
            .Include(h => h.Branch)
            .FirstOrDefaultAsync(h => h.Id == id && h.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Held sale {id} not found.");

        if (entity.Status is not ("Held" or "Resumed"))
            throw new ValidationAppException([$"Held sale cannot be discarded from status '{entity.Status}'."]);

        entity.Status = "Cancelled";
        await db.SaveChangesAsync(ct);
    }

    private static HeldSaleDto Map(HeldSale h) => new()
    {
        Id = h.Id,
        BranchId = h.BranchId,
        TerminalId = h.TerminalId,
        UserId = h.UserId,
        CustomerId = h.CustomerId,
        CartData = h.CartData,
        TotalAmount = h.TotalAmount,
        HeldAt = h.HeldAt,
        ExpiresAt = h.ExpiresAt,
        Status = h.Status
    };
}
