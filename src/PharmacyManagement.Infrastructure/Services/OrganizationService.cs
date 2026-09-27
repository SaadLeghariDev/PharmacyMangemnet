using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Organization;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class OrganizationService(PharmacyManagementDbContext db, ICurrentUserService currentUser) : IOrganizationService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<IReadOnlyList<TenantDto>> GetTenantsAsync(CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var tenants = await db.Tenants.AsNoTracking()
            .Where(t => t.Id == tenantId)
            .ToListAsync(ct);
        return tenants.Select(MapTenant).ToList();
    }

    public async Task<TenantDto?> GetTenantAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        if (id != tenantId) return null;
        var t = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return t is null ? null : MapTenant(t);
    }

    public async Task<PagedResult<BranchDto>> GetBranchesAsync(PaginationQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.Branches.AsNoTracking().Where(b => b.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(b => b.Name.Contains(s) || b.Code.Contains(s));
        }
        q = query.SortBy?.ToLowerInvariant() switch
        {
            "code" => query.SortDesc ? q.OrderByDescending(b => b.Code) : q.OrderBy(b => b.Code),
            "name" => query.SortDesc ? q.OrderByDescending(b => b.Name) : q.OrderBy(b => b.Name),
            _ => q.OrderBy(b => b.Code)
        };
        return await PageMapAsync(q, query, MapBranch, ct);
    }

    public async Task<BranchDto?> GetBranchAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var b = await db.Branches.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
        return b is null ? null : MapBranch(b);
    }

    public async Task<BranchDto> CreateBranchAsync(CreateBranchRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        if (await db.Branches.AnyAsync(b => b.TenantId == tenantId && b.Code == request.Code, ct))
            throw new ConflictException($"Branch code '{request.Code}' already exists.");

        var entity = new Branch
        {
            TenantId = tenantId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            BranchType = request.BranchType,
            City = request.City,
            Province = request.Province,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[8]
        };
        db.Branches.Add(entity);
        await db.SaveChangesAsync(ct);
        return MapBranch(entity);
    }

    public async Task<BranchDto> UpdateBranchAsync(long id, UpdateBranchRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Branches.FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Branch {id} not found.");
        entity.Name = request.Name.Trim();
        entity.BranchType = request.BranchType;
        entity.City = request.City;
        entity.Province = request.Province;
        entity.Phone = request.Phone;
        entity.Email = request.Email;
        entity.Address = request.Address;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return MapBranch(entity);
    }

    public async Task DeactivateBranchAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Branches.FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Branch {id} not found.");
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<WarehouseDto>> GetWarehousesAsync(long? branchId, PaginationQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.Warehouses.AsNoTracking()
            .Where(w => w.Branch.TenantId == tenantId);
        if (branchId is long bid) q = q.Where(w => w.BranchId == bid);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(w => w.Name.Contains(s) || w.Code.Contains(s));
        }
        q = q.OrderBy(w => w.Code);
        return await PageMapAsync(q, query, MapWarehouse, ct);
    }

    public async Task<WarehouseDto?> GetWarehouseAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var w = await db.Warehouses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.Branch.TenantId == tenantId, ct);
        return w is null ? null : MapWarehouse(w);
    }

    public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var branch = await db.Branches.FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == tenantId, ct)
            ?? throw new ValidationAppException([$"Branch {request.BranchId} not found for tenant."]);
        if (await db.Warehouses.AnyAsync(w => w.BranchId == branch.Id && w.Code == request.Code, ct))
            throw new ConflictException($"Warehouse code '{request.Code}' already exists for branch.");

        var entity = new Warehouse
        {
            BranchId = branch.Id,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            WarehouseType = request.WarehouseType,
            IsMain = request.IsMain,
            TemperatureControlled = request.TemperatureControlled,
            IsActive = true
        };
        db.Warehouses.Add(entity);
        await db.SaveChangesAsync(ct);
        return MapWarehouse(entity);
    }

    public async Task<WarehouseDto> UpdateWarehouseAsync(long id, UpdateWarehouseRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == id && w.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Warehouse {id} not found.");
        entity.Name = request.Name.Trim();
        entity.WarehouseType = request.WarehouseType;
        entity.IsMain = request.IsMain;
        entity.TemperatureControlled = request.TemperatureControlled;
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return MapWarehouse(entity);
    }

    public async Task DeactivateWarehouseAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == id && w.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Warehouse {id} not found.");
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<WarehouseLocationDto>> GetWarehouseLocationsAsync(long? warehouseId, PaginationQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.WarehouseLocations.AsNoTracking()
            .Where(l => l.Warehouse.Branch.TenantId == tenantId);
        if (warehouseId is long wid) q = q.Where(l => l.WarehouseId == wid);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(l => l.Name.Contains(s) || l.Code.Contains(s));
        }
        q = q.OrderBy(l => l.Code);
        return await PageMapAsync(q, query, MapLocation, ct);
    }

    public async Task<WarehouseLocationDto?> GetWarehouseLocationAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var l = await db.WarehouseLocations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.Warehouse.Branch.TenantId == tenantId, ct);
        return l is null ? null : MapLocation(l);
    }

    public async Task<WarehouseLocationDto> CreateWarehouseLocationAsync(CreateWarehouseLocationRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var wh = await db.Warehouses.FirstOrDefaultAsync(w => w.Id == request.WarehouseId && w.Branch.TenantId == tenantId, ct)
            ?? throw new ValidationAppException([$"Warehouse {request.WarehouseId} not found for tenant."]);
        if (await db.WarehouseLocations.AnyAsync(l => l.WarehouseId == wh.Id && l.Code == request.Code, ct))
            throw new ConflictException($"Location code '{request.Code}' already exists.");

        var entity = new WarehouseLocation
        {
            WarehouseId = wh.Id,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            RackNo = request.RackNo,
            ShelfNo = request.ShelfNo,
            BinNo = request.BinNo,
            LocationType = request.LocationType,
            IsActive = true
        };
        db.WarehouseLocations.Add(entity);
        await db.SaveChangesAsync(ct);
        return MapLocation(entity);
    }

    public async Task<WarehouseLocationDto> UpdateWarehouseLocationAsync(long id, UpdateWarehouseLocationRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.WarehouseLocations.FirstOrDefaultAsync(l => l.Id == id && l.Warehouse.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Warehouse location {id} not found.");
        entity.Name = request.Name.Trim();
        entity.RackNo = request.RackNo;
        entity.ShelfNo = request.ShelfNo;
        entity.BinNo = request.BinNo;
        entity.LocationType = request.LocationType;
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return MapLocation(entity);
    }

    public async Task DeactivateWarehouseLocationAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.WarehouseLocations.FirstOrDefaultAsync(l => l.Id == id && l.Warehouse.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Warehouse location {id} not found.");
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<CounterDto>> GetCountersAsync(long? branchId, PaginationQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.Counters.AsNoTracking().Where(c => c.Branch.TenantId == tenantId);
        if (branchId is long bid) q = q.Where(c => c.BranchId == bid);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(c => c.Name.Contains(s) || c.Code.Contains(s));
        }
        q = q.OrderBy(c => c.Code);
        return await PageMapAsync(q, query, MapCounter, ct);
    }

    public async Task<CounterDto?> GetCounterAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var c = await db.Counters.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Branch.TenantId == tenantId, ct);
        return c is null ? null : MapCounter(c);
    }

    public async Task<CounterDto> CreateCounterAsync(CreateCounterRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var branch = await db.Branches.FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == tenantId, ct)
            ?? throw new ValidationAppException([$"Branch {request.BranchId} not found for tenant."]);
        if (await db.Counters.AnyAsync(c => c.BranchId == branch.Id && c.Code == request.Code, ct))
            throw new ConflictException($"Counter code '{request.Code}' already exists.");

        var entity = new Counter
        {
            BranchId = branch.Id,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            CounterType = request.CounterType,
            IsActive = true
        };
        db.Counters.Add(entity);
        await db.SaveChangesAsync(ct);
        return MapCounter(entity);
    }

    public async Task<CounterDto> UpdateCounterAsync(long id, UpdateCounterRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Counters.FirstOrDefaultAsync(c => c.Id == id && c.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Counter {id} not found.");
        entity.Name = request.Name.Trim();
        entity.CounterType = request.CounterType;
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return MapCounter(entity);
    }

    public async Task DeactivateCounterAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Counters.FirstOrDefaultAsync(c => c.Id == id && c.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Counter {id} not found.");
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<PosTerminalDto>> GetPosTerminalsAsync(long? branchId, PaginationQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.Posterminals.AsNoTracking().Where(t => t.Branch.TenantId == tenantId);
        if (branchId is long bid) q = q.Where(t => t.BranchId == bid);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(t => t.TerminalCode.Contains(s) || (t.ComputerName != null && t.ComputerName.Contains(s)));
        }
        q = q.OrderBy(t => t.TerminalCode);
        return await PageMapAsync(q, query, MapTerminal, ct);
    }

    public async Task<PosTerminalDto?> GetPosTerminalAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var t = await db.Posterminals.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Branch.TenantId == tenantId, ct);
        return t is null ? null : MapTerminal(t);
    }

    public async Task<PosTerminalDto> CreatePosTerminalAsync(CreatePosTerminalRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var branch = await db.Branches.FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == tenantId, ct)
            ?? throw new ValidationAppException([$"Branch {request.BranchId} not found for tenant."]);
        var counter = await db.Counters.FirstOrDefaultAsync(c => c.Id == request.CounterId && c.BranchId == branch.Id, ct)
            ?? throw new ValidationAppException([$"Counter {request.CounterId} not found for branch."]);
        if (await db.Posterminals.AnyAsync(t => t.BranchId == branch.Id && t.TerminalCode == request.TerminalCode, ct))
            throw new ConflictException($"Terminal code '{request.TerminalCode}' already exists.");

        var entity = new Posterminal
        {
            BranchId = branch.Id,
            CounterId = counter.Id,
            TerminalCode = request.TerminalCode.Trim(),
            ComputerName = request.ComputerName,
            MacAddress = request.MacAddress,
            Ipaddress = request.IpAddress,
            SerialNumber = request.SerialNumber,
            IsPrimary = request.IsPrimary,
            IsOnline = false,
            IsActive = true
        };
        db.Posterminals.Add(entity);
        await db.SaveChangesAsync(ct);
        return MapTerminal(entity);
    }

    public async Task<PosTerminalDto> UpdatePosTerminalAsync(long id, UpdatePosTerminalRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Posterminals.FirstOrDefaultAsync(t => t.Id == id && t.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"POS terminal {id} not found.");
        var counter = await db.Counters.FirstOrDefaultAsync(c => c.Id == request.CounterId && c.BranchId == entity.BranchId, ct)
            ?? throw new ValidationAppException([$"Counter {request.CounterId} not found for branch."]);
        entity.CounterId = counter.Id;
        entity.ComputerName = request.ComputerName;
        entity.MacAddress = request.MacAddress;
        entity.Ipaddress = request.IpAddress;
        entity.SerialNumber = request.SerialNumber;
        entity.IsPrimary = request.IsPrimary;
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return MapTerminal(entity);
    }

    public async Task DeactivatePosTerminalAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Posterminals.FirstOrDefaultAsync(t => t.Id == id && t.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"POS terminal {id} not found.");
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    private static async Task<PagedResult<TDto>> PageMapAsync<TEntity, TDto>(
        IQueryable<TEntity> query,
        PaginationQuery page,
        Func<TEntity, TDto> map,
        CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page.Page - 1) * page.PageSize).Take(page.PageSize).ToListAsync(ct);
        return new PagedResult<TDto>
        {
            Items = items.Select(map).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = total
        };
    }

    private static TenantDto MapTenant(Tenant t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        LegalName = t.LegalName,
        Ntn = t.Ntn,
        Strn = t.Strn,
        LicenseNo = t.LicenseNo,
        Phone = t.Phone,
        Email = t.Email,
        Address = t.Address,
        IsActive = t.IsActive
    };

    private static BranchDto MapBranch(Branch b) => new()
    {
        Id = b.Id,
        TenantId = b.TenantId,
        Code = b.Code,
        Name = b.Name,
        BranchType = b.BranchType,
        City = b.City,
        Province = b.Province,
        Phone = b.Phone,
        Email = b.Email,
        Address = b.Address,
        IsActive = b.IsActive
    };

    private static WarehouseDto MapWarehouse(Warehouse w) => new()
    {
        Id = w.Id,
        BranchId = w.BranchId,
        Code = w.Code,
        Name = w.Name,
        WarehouseType = w.WarehouseType,
        IsMain = w.IsMain,
        TemperatureControlled = w.TemperatureControlled,
        IsActive = w.IsActive
    };

    private static WarehouseLocationDto MapLocation(WarehouseLocation l) => new()
    {
        Id = l.Id,
        WarehouseId = l.WarehouseId,
        Code = l.Code,
        Name = l.Name,
        RackNo = l.RackNo,
        ShelfNo = l.ShelfNo,
        BinNo = l.BinNo,
        LocationType = l.LocationType,
        IsActive = l.IsActive
    };

    private static CounterDto MapCounter(Counter c) => new()
    {
        Id = c.Id,
        BranchId = c.BranchId,
        Code = c.Code,
        Name = c.Name,
        CounterType = c.CounterType,
        IsActive = c.IsActive
    };

    private static PosTerminalDto MapTerminal(Posterminal t) => new()
    {
        Id = t.Id,
        BranchId = t.BranchId,
        CounterId = t.CounterId,
        TerminalCode = t.TerminalCode,
        ComputerName = t.ComputerName,
        MacAddress = t.MacAddress,
        IpAddress = t.Ipaddress,
        SerialNumber = t.SerialNumber,
        IsPrimary = t.IsPrimary,
        IsOnline = t.IsOnline,
        IsActive = t.IsActive
    };
}
