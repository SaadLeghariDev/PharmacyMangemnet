using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Admin;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class RoleAdminService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IRoleAdminService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<RoleDto>> SearchAsync(RoleQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.Roles.AsNoTracking()
            .Include(r => r.Permissions)
            .Where(r => r.TenantId == tenantId);

        if (query.IsSystemRole is bool system) q = q.Where(r => r.IsSystemRole == system);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(r => r.Name.Contains(s) || (r.Description != null && r.Description.Contains(s)));
        }

        q = q.OrderBy(r => r.Name);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<RoleDto>
        {
            Items = rows.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<RoleDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var role = await db.Roles.AsNoTracking()
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, ct);
        return role is null ? null : Map(role);
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var name = request.Name.Trim();
        if (await db.Roles.AnyAsync(r => r.TenantId == tenantId && r.Name == name, ct))
            throw new ConflictException($"Role '{name}' already exists.");

        var entity = new Role
        {
            TenantId = tenantId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsSystemRole = false
        };

        if (request.PermissionIds is { Count: > 0 })
        {
            var perms = await LoadPermissionsAsync(request.PermissionIds, ct);
            foreach (var p in perms) entity.Permissions.Add(p);
        }

        db.Roles.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<RoleDto> UpdateAsync(long id, UpdateRoleRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Roles.FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Role {id} not found.");

        var name = request.Name.Trim();
        if (await db.Roles.AnyAsync(r => r.TenantId == tenantId && r.Name == name && r.Id != id, ct))
            throw new ConflictException($"Role '{name}' already exists.");

        entity.Name = name;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<RoleDto> AssignPermissionsAsync(long id, AssignRolePermissionsRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Role {id} not found.");

        var perms = await LoadPermissionsAsync(request.PermissionIds ?? Array.Empty<long>(), ct);
        entity.Permissions.Clear();
        foreach (var p in perms) entity.Permissions.Add(p);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<PagedResult<PermissionDto>> SearchPermissionsAsync(PermissionQuery query, CancellationToken ct = default)
    {
        _ = RequireTenantId();
        var q = db.Permissions.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Module))
        {
            var module = query.Module.Trim();
            q = q.Where(p => p.Module == module);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(p => p.Code.Contains(s) || p.Name.Contains(s) || p.Module.Contains(s));
        }

        q = q.OrderBy(p => p.Module).ThenBy(p => p.Code);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<PermissionDto>
        {
            Items = rows.Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Module = p.Module,
                Description = p.Description
            }).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    private async Task<List<Permission>> LoadPermissionsAsync(IReadOnlyList<long> permissionIds, CancellationToken ct)
    {
        var distinct = permissionIds.Distinct().ToList();
        if (distinct.Count == 0) return [];
        var perms = await db.Permissions.Where(p => distinct.Contains(p.Id)).ToListAsync(ct);
        if (perms.Count != distinct.Count)
            throw new ValidationAppException(["One or more permissions were not found."]);
        return perms;
    }

    private static RoleDto Map(Role r) => new()
    {
        Id = r.Id,
        TenantId = r.TenantId,
        Name = r.Name,
        Description = r.Description,
        IsSystemRole = r.IsSystemRole,
        PermissionIds = r.Permissions.Select(p => p.Id).OrderBy(x => x).ToList(),
        PermissionCodes = r.Permissions.Select(p => p.Code).OrderBy(x => x).ToList()
    };
}
