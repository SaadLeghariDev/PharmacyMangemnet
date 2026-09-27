using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Admin;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Identity;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class UserAdminService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    IPasswordHasher<PasswordIdentityUser> passwordHasher) : IUserAdminService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<UserAdminDto>> SearchAsync(UserAdminQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.Users.AsNoTracking()
            .Include(u => u.Roles)
            .Include(u => u.Branches)
            .Where(u => u.TenantId == tenantId);

        if (query.IsActive is bool active) q = q.Where(u => u.IsActive == active);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(u =>
                u.Username.Contains(s) ||
                u.FullName.Contains(s) ||
                (u.Email != null && u.Email.Contains(s)) ||
                (u.EmployeeCode != null && u.EmployeeCode.Contains(s)));
        }

        q = q.OrderBy(u => u.Username);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<UserAdminDto>
        {
            Items = rows.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<UserAdminDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var user = await db.Users.AsNoTracking()
            .Include(u => u.Roles)
            .Include(u => u.Branches)
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct);
        return user is null ? null : Map(user);
    }

    public async Task<UserAdminDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var username = request.Username.Trim();
        if (await db.Users.AnyAsync(u => u.TenantId == tenantId && u.Username == username, ct))
            throw new ConflictException($"Username '{username}' already exists.");

        var now = DateTime.UtcNow;
        var identityUser = new PasswordIdentityUser { UserName = username };
        var entity = new User
        {
            TenantId = tenantId,
            Username = username,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            PasswordHash = passwordHasher.HashPassword(identityUser, request.Password),
            FullName = request.FullName.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            EmployeeCode = string.IsNullOrWhiteSpace(request.EmployeeCode) ? null : request.EmployeeCode.Trim(),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
            RowVersion = new byte[8]
        };

        if (request.RoleIds is { Count: > 0 })
        {
            var roles = await LoadRolesAsync(tenantId, request.RoleIds, ct);
            foreach (var role in roles) entity.Roles.Add(role);
        }

        if (request.BranchIds is { Count: > 0 })
        {
            var branches = await LoadBranchesAsync(tenantId, request.BranchIds, ct);
            foreach (var branch in branches) entity.Branches.Add(branch);
        }

        db.Users.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<UserAdminDto> UpdateAsync(long id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"User {id} not found.");

        entity.FullName = request.FullName.Trim();
        entity.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        entity.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        entity.EmployeeCode = string.IsNullOrWhiteSpace(request.EmployeeCode) ? null : request.EmployeeCode.Trim();
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var identityUser = new PasswordIdentityUser { UserName = entity.Username };
            entity.PasswordHash = passwordHasher.HashPassword(identityUser, request.Password);
        }

        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task DeactivateAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"User {id} not found.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<UserAdminDto> AssignRolesAsync(long id, AssignUserRolesRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"User {id} not found.");

        var roles = await LoadRolesAsync(tenantId, request.RoleIds ?? Array.Empty<long>(), ct);
        entity.Roles.Clear();
        foreach (var role in roles) entity.Roles.Add(role);
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<UserAdminDto> AssignBranchesAsync(long id, AssignUserBranchesRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Users
            .Include(u => u.Branches)
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"User {id} not found.");

        var branches = await LoadBranchesAsync(tenantId, request.BranchIds ?? Array.Empty<long>(), ct);
        entity.Branches.Clear();
        foreach (var branch in branches) entity.Branches.Add(branch);
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    private async Task<List<Role>> LoadRolesAsync(long tenantId, IReadOnlyList<long> roleIds, CancellationToken ct)
    {
        var distinct = roleIds.Distinct().ToList();
        if (distinct.Count == 0) return [];
        var roles = await db.Roles.Where(r => r.TenantId == tenantId && distinct.Contains(r.Id)).ToListAsync(ct);
        if (roles.Count != distinct.Count)
            throw new ValidationAppException(["One or more roles were not found for this tenant."]);
        return roles;
    }

    private async Task<List<Branch>> LoadBranchesAsync(long tenantId, IReadOnlyList<long> branchIds, CancellationToken ct)
    {
        var distinct = branchIds.Distinct().ToList();
        if (distinct.Count == 0) return [];
        var branches = await db.Branches.Where(b => b.TenantId == tenantId && distinct.Contains(b.Id)).ToListAsync(ct);
        if (branches.Count != distinct.Count)
            throw new ValidationAppException(["One or more branches were not found for this tenant."]);
        return branches;
    }

    private static UserAdminDto Map(User u) => new()
    {
        Id = u.Id,
        TenantId = u.TenantId,
        Username = u.Username,
        Email = u.Email,
        FullName = u.FullName,
        Phone = u.Phone,
        EmployeeCode = u.EmployeeCode,
        IsActive = u.IsActive,
        LastLoginAt = u.LastLoginAt,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt,
        RoleIds = u.Roles.Select(r => r.Id).OrderBy(x => x).ToList(),
        RoleNames = u.Roles.Select(r => r.Name).OrderBy(x => x).ToList(),
        BranchIds = u.Branches.Select(b => b.Id).OrderBy(x => x).ToList()
    };
}
