using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public long? UserId => ParseLong(User?.FindFirstValue(AppClaimTypes.UserId) ?? User?.FindFirstValue(ClaimTypes.NameIdentifier));

    public long? TenantId => ParseLong(User?.FindFirstValue(AppClaimTypes.TenantId));

    public IReadOnlyCollection<string> Permissions =>
        User?.FindAll(AppClaimTypes.Permission).Select(c => c.Value).ToArray() ?? Array.Empty<string>();

    public IReadOnlyCollection<long> BranchIds =>
        User?.FindAll(AppClaimTypes.BranchId).Select(c => long.Parse(c.Value)).ToArray() ?? Array.Empty<long>();

    private static long? ParseLong(string? value) =>
        long.TryParse(value, out var id) ? id : null;
}
