using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Admin;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/permissions")]
public sealed class PermissionsController(IRoleAdminService roles) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.SecRoles)]
    public async Task<ActionResult<ApiResponse<PagedResult<PermissionDto>>>> Search(
        [FromQuery] PermissionQuery query, CancellationToken ct)
    {
        var result = await roles.SearchPermissionsAsync(query, ct);
        return Ok(ApiResponse<PagedResult<PermissionDto>>.Ok(result));
    }
}
