using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PharmacyManagement.Application.DTOs.Auth;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Options;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Identity;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class AuthService(
    PharmacyManagementDbContext db,
    IOptions<JwtOptions> jwtOptions,
    IOptions<AuthBootstrapOptions> bootstrapOptions,
    IHostEnvironment environment,
    ICurrentUserService currentUser,
    IPasswordHasher<PasswordIdentityUser> passwordHasher) : IAuthService
{
    /// <summary>Known Phase 1 seed placeholder — not a valid Identity/BCrypt hash.</summary>
    public const string SeedPlaceholderHash = "$2b$12$demo.hash.not.for.production";

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var query = db.Users.AsNoTracking()
            .Include(u => u.Roles).ThenInclude(r => r.Permissions)
            .Include(u => u.Branches)
            .Where(u => u.Username == request.Username && u.IsActive);

        if (request.TenantId is long tenantId)
            query = query.Where(u => u.TenantId == tenantId);

        var user = await query.FirstOrDefaultAsync(ct)
            ?? throw new UnauthorizedAppException("Invalid username or password.");

        if (!VerifyPassword(user, request.Password))
            throw new UnauthorizedAppException("Invalid username or password.");

        var roles = user.Roles.Select(r => r.Name).Distinct().ToList();
        var permissions = user.Roles.SelectMany(r => r.Permissions).Select(p => p.Code).Distinct().ToList();
        var branchIds = user.Branches.Select(b => b.Id).Distinct().ToList();

        var expires = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiryMinutes);
        var token = CreateToken(user, roles, permissions, branchIds, expires);

        // Best-effort last login update (tracked query)
        var tracked = await db.Users.FirstAsync(u => u.Id == user.Id, ct);
        tracked.LastLoginAt = DateTime.UtcNow;
        tracked.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            User = MapProfile(user, roles, permissions, branchIds)
        };
    }

    public async Task<UserProfileDto> GetCurrentUserAsync(CancellationToken ct = default)
    {
        if (currentUser.UserId is not long userId)
            throw new UnauthorizedAppException();

        var user = await db.Users.AsNoTracking()
            .Include(u => u.Roles).ThenInclude(r => r.Permissions)
            .Include(u => u.Branches)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, ct)
            ?? throw new UnauthorizedAppException();

        var roles = user.Roles.Select(r => r.Name).Distinct().ToList();
        var permissions = user.Roles.SelectMany(r => r.Permissions).Select(p => p.Code).Distinct().ToList();
        var branchIds = user.Branches.Select(b => b.Id).Distinct().ToList();
        return MapProfile(user, roles, permissions, branchIds);
    }

    private bool VerifyPassword(User user, string password)
    {
        if (string.Equals(user.PasswordHash, SeedPlaceholderHash, StringComparison.Ordinal)
            && environment.IsDevelopment())
        {
            return string.Equals(password, bootstrapOptions.Value.DevelopmentAdminPassword, StringComparison.Ordinal);
        }

        var identityUser = new PasswordIdentityUser { UserName = user.Username };
        var result = passwordHasher.VerifyHashedPassword(identityUser, user.PasswordHash, password);
        if (result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded)
            return true;

        // Also accept BCrypt-style hashes if present (prefix $2a/$2b/$2y) via Identity failure path only —
        // seed placeholder already handled above. No third-party BCrypt package required for 2A.
        return false;
    }

    private string CreateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions, IEnumerable<long> branchIds, DateTime expires)
    {
        var opts = jwtOptions.Value;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(AppClaimTypes.UserId, user.Id.ToString()),
            new(AppClaimTypes.TenantId, user.TenantId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(JwtRegisteredClaimNames.UniqueName, user.Username)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim(AppClaimTypes.Permission, p)));
        claims.AddRange(branchIds.Select(b => new Claim(AppClaimTypes.BranchId, b.ToString())));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: opts.Issuer,
            audience: opts.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserProfileDto MapProfile(User user, IReadOnlyList<string> roles, IReadOnlyList<string> permissions, IReadOnlyList<long> branchIds) =>
        new()
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Roles = roles,
            Permissions = permissions,
            BranchIds = branchIds
        };
}
