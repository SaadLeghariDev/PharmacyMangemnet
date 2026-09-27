using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Organization;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/tenants")]
public sealed class TenantsController(IOrganizationService org) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TenantDto>>>> List(CancellationToken ct)
    {
        var items = await org.GetTenantsAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<TenantDto>>.Ok(items));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<TenantDto>>> Get(long id, CancellationToken ct)
    {
        var item = await org.GetTenantAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<TenantDto>.Fail("Tenant not found"));
        return Ok(ApiResponse<TenantDto>.Ok(item));
    }
}
