namespace PharmacyManagement.Application.DTOs.Admin;

public sealed class UserAdminDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public IReadOnlyList<long> RoleIds { get; set; } = Array.Empty<long>();
    public IReadOnlyList<string> RoleNames { get; set; } = Array.Empty<string>();
    public IReadOnlyList<long> BranchIds { get; set; } = Array.Empty<long>();
}

public sealed class UserAdminQuery : Common.PaginationQuery
{
    public bool? IsActive { get; set; }
}

public sealed class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? EmployeeCode { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<long>? RoleIds { get; set; }
    public IReadOnlyList<long>? BranchIds { get; set; }
}

public sealed class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? EmployeeCode { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Password { get; set; }
}

public sealed class AssignUserRolesRequest
{
    public IReadOnlyList<long> RoleIds { get; set; } = Array.Empty<long>();
}

public sealed class AssignUserBranchesRequest
{
    public IReadOnlyList<long> BranchIds { get; set; } = Array.Empty<long>();
}

public sealed class RoleDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public IReadOnlyList<long> PermissionIds { get; set; } = Array.Empty<long>();
    public IReadOnlyList<string> PermissionCodes { get; set; } = Array.Empty<string>();
}

public sealed class RoleQuery : Common.PaginationQuery
{
    public bool? IsSystemRole { get; set; }
}

public sealed class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyList<long>? PermissionIds { get; set; }
}

public sealed class UpdateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class AssignRolePermissionsRequest
{
    public IReadOnlyList<long> PermissionIds { get; set; } = Array.Empty<long>();
}

public sealed class PermissionDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class PermissionQuery : Common.PaginationQuery
{
    public string? Module { get; set; }
}

public sealed class TenantSettingDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string? SettingValue { get; set; }
    public bool IsEncrypted { get; set; }
}

public sealed class BranchSettingDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string? SettingValue { get; set; }
    public bool IsEncrypted { get; set; }
}

public sealed class UpsertSettingRequest
{
    public string SettingKey { get; set; } = string.Empty;
    public string? SettingValue { get; set; }
    public bool IsEncrypted { get; set; }
}

public sealed class SettingQuery : Common.PaginationQuery
{
    public string? Key { get; set; }
}

public sealed class BranchSettingQuery : Common.PaginationQuery
{
    public long? BranchId { get; set; }
    public string? Key { get; set; }
}

public sealed class ReasonCodeDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string ReasonType { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ReasonCodeQuery : Common.PaginationQuery
{
    public string? ReasonType { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class CreateReasonCodeRequest
{
    public string ReasonType { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateReasonCodeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AuditLogDto
{
    public long Id { get; set; }
    public long? TenantId { get; set; }
    public long? BranchId { get; set; }
    public long? UserId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public long? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public long? TerminalId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class AuditLogQuery : Common.PaginationQuery
{
    public string? EntityName { get; set; }
    public string? Action { get; set; }
    public long? UserId { get; set; }
    public long? BranchId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}
