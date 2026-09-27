using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class PurchaseOrderService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    INumberSequenceService sequences) : IPurchaseOrderService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<PurchaseOrderDto>> SearchAsync(PurchaseOrderQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.PurchaseOrders.AsNoTracking()
            .Include(p => p.PurchaseOrderLines)
            .Include(p => p.Branch)
            .Where(p => p.Branch.TenantId == tenantId);

        if (query.BranchId is long branchId) q = q.Where(p => p.BranchId == branchId);
        if (query.SupplierId is long supplierId) q = q.Where(p => p.SupplierId == supplierId);
        if (!string.IsNullOrWhiteSpace(query.Status)) q = q.Where(p => p.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(p => p.Ponumber.Contains(s) || (p.Remarks != null && p.Remarks.Contains(s)));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "podate" => query.SortDesc ? q.OrderByDescending(p => p.Podate) : q.OrderBy(p => p.Podate),
            "status" => query.SortDesc ? q.OrderByDescending(p => p.Status) : q.OrderBy(p => p.Status),
            _ => q.OrderByDescending(p => p.Id)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<PurchaseOrderDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<PurchaseOrderDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await Query().FirstOrDefaultAsync(p => p.Id == id && p.Branch.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        await EnsureBranchWarehouseSupplierAsync(tenantId, request.BranchId, request.WarehouseId, request.SupplierId, ct);
        await EnsureLinesAsync(tenantId, request.Lines, ct);

        var poNumber = await sequences.AllocateNextAsync(
            tenantId, DocumentTypes.PurchaseOrder, request.BranchId, null, "PO-", ct);

        var entity = new PurchaseOrder
        {
            BranchId = request.BranchId,
            WarehouseId = request.WarehouseId,
            SupplierId = request.SupplierId,
            Ponumber = poNumber,
            Podate = DateTime.UtcNow,
            ExpectedDate = request.ExpectedDate,
            Status = "Draft",
            Remarks = request.Remarks,
            CreatedBy = currentUser.UserId
        };

        foreach (var line in request.Lines)
        {
            entity.PurchaseOrderLines.Add(new PurchaseOrderLine
            {
                ProductId = line.ProductId,
                ProductUnitId = line.ProductUnitId,
                Quantity = line.Quantity,
                FreeQuantity = line.FreeQuantity,
                UnitPrice = line.UnitPrice,
                DiscountAmount = line.DiscountAmount,
                TaxAmount = line.TaxAmount,
                NetAmount = CalculateNet(line.Quantity, line.UnitPrice, line.DiscountAmount, line.TaxAmount)
            });
        }

        db.PurchaseOrders.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<PurchaseOrderDto> UpdateAsync(long id, UpdatePurchaseOrderRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.PurchaseOrders
            .Include(p => p.PurchaseOrderLines)
            .Include(p => p.Branch)
            .FirstOrDefaultAsync(p => p.Id == id && p.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Purchase order {id} not found.");

        if (entity.Status is not "Draft")
            throw new ValidationAppException([$"Purchase order in status '{entity.Status}' cannot be edited."]);

        await EnsureLinesAsync(tenantId, request.Lines, ct);

        entity.ExpectedDate = request.ExpectedDate;
        entity.Remarks = request.Remarks;
        db.PurchaseOrderLines.RemoveRange(entity.PurchaseOrderLines);
        entity.PurchaseOrderLines.Clear();
        foreach (var line in request.Lines)
        {
            entity.PurchaseOrderLines.Add(new PurchaseOrderLine
            {
                ProductId = line.ProductId,
                ProductUnitId = line.ProductUnitId,
                Quantity = line.Quantity,
                FreeQuantity = line.FreeQuantity,
                UnitPrice = line.UnitPrice,
                DiscountAmount = line.DiscountAmount,
                TaxAmount = line.TaxAmount,
                NetAmount = CalculateNet(line.Quantity, line.UnitPrice, line.DiscountAmount, line.TaxAmount)
            });
        }

        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<PurchaseOrderDto> SubmitAsync(long id, CancellationToken ct = default)
    {
        var entity = await LoadForStatusChangeAsync(id, ct);
        if (entity.Status != "Draft")
            throw new ValidationAppException([$"Only Draft POs can be submitted (current: {entity.Status})."]);
        if (entity.PurchaseOrderLines.Count == 0)
            throw new ValidationAppException(["Purchase order has no lines."]);
        entity.Status = "Submitted";
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<PurchaseOrderDto> ApproveAsync(long id, CancellationToken ct = default)
    {
        var entity = await LoadForStatusChangeAsync(id, ct);
        if (entity.Status != "Submitted")
            throw new ValidationAppException([$"Only Submitted POs can be approved (current: {entity.Status})."]);
        entity.Status = "Approved";
        entity.ApprovedBy = currentUser.UserId;
        entity.ApprovedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    private async Task<PurchaseOrder> LoadForStatusChangeAsync(long id, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        return await db.PurchaseOrders
            .Include(p => p.PurchaseOrderLines)
            .Include(p => p.Branch)
            .FirstOrDefaultAsync(p => p.Id == id && p.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Purchase order {id} not found.");
    }

    private IQueryable<PurchaseOrder> Query() =>
        db.PurchaseOrders.AsNoTracking()
            .Include(p => p.PurchaseOrderLines)
            .Include(p => p.Branch);

    private async Task EnsureBranchWarehouseSupplierAsync(
        long tenantId, long branchId, long warehouseId, long supplierId, CancellationToken ct)
    {
        var branch = await db.Branches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == branchId && b.TenantId == tenantId && b.IsActive, ct)
            ?? throw new ValidationAppException(["Branch not found for tenant."]);
        _ = branch;

        if (!await db.Warehouses.AnyAsync(w => w.Id == warehouseId && w.BranchId == branchId && w.IsActive, ct))
            throw new ValidationAppException(["Warehouse not found for branch."]);

        if (!await db.Suppliers.AnyAsync(s => s.Id == supplierId && s.TenantId == tenantId && s.IsActive, ct))
            throw new ValidationAppException(["Supplier not found for tenant."]);
    }

    private async Task EnsureLinesAsync(long tenantId, IEnumerable<PurchaseOrderLineRequest> lines, CancellationToken ct)
    {
        foreach (var line in lines)
        {
            if (!await db.Products.AnyAsync(p => p.Id == line.ProductId && p.TenantId == tenantId && p.IsActive, ct))
                throw new ValidationAppException([$"Product {line.ProductId} not found."]);
            if (!await db.ProductUnits.AnyAsync(u =>
                    u.Id == line.ProductUnitId && u.ProductId == line.ProductId && u.IsActive, ct))
                throw new ValidationAppException([$"Product unit {line.ProductUnitId} invalid for product {line.ProductId}."]);
        }
    }

    private static decimal CalculateNet(decimal qty, decimal unitPrice, decimal discount, decimal tax) =>
        Math.Round(qty * unitPrice - discount + tax, 4);

    private static PurchaseOrderDto Map(PurchaseOrder p) => new()
    {
        Id = p.Id,
        BranchId = p.BranchId,
        WarehouseId = p.WarehouseId,
        SupplierId = p.SupplierId,
        PoNumber = p.Ponumber,
        PoDate = p.Podate,
        ExpectedDate = p.ExpectedDate,
        Status = p.Status,
        Remarks = p.Remarks,
        CreatedBy = p.CreatedBy,
        ApprovedBy = p.ApprovedBy,
        ApprovedAt = p.ApprovedAt,
        Lines = p.PurchaseOrderLines.Select(l => new PurchaseOrderLineDto
        {
            Id = l.Id,
            ProductId = l.ProductId,
            ProductUnitId = l.ProductUnitId,
            Quantity = l.Quantity,
            FreeQuantity = l.FreeQuantity,
            UnitPrice = l.UnitPrice,
            DiscountAmount = l.DiscountAmount,
            TaxAmount = l.TaxAmount,
            NetAmount = l.NetAmount
        }).ToList()
    };
}
