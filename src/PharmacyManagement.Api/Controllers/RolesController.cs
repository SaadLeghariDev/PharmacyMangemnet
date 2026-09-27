using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Admin;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/roles")]
public sealed class RolesController(
    IRoleAdminService roles,
    IValidator<CreateRoleRequest> createValidator,
    IValidator<UpdateRoleRequest> updateValidator,
    IValidator<AssignRolePermissionsRequest> permissionsValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.SecRoles)]
    public async Task<ActionResult<ApiResponse<PagedResult<RoleDto>>>> Search(
        [FromQuery] RoleQuery query, CancellationToken ct)
    {
        var result = await roles.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<RoleDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.SecRoles)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Get(long id, CancellationToken ct)
    {
        var item = await roles.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<RoleDto>.Fail("Role not found"));
        return Ok(ApiResponse<RoleDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.SecRoles)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create(
        [FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<RoleDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await roles.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<RoleDto>.Ok(item, "Role created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.SecRoles)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(
        long id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<RoleDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await roles.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<RoleDto>.Ok(item, "Role updated"));
    }

    [HttpPut("{id:long}/permissions")]
    [Authorize(Policy = PermissionCodes.SecRoles)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> AssignPermissions(
        long id, [FromBody] AssignRolePermissionsRequest request, CancellationToken ct)
    {
        var validation = await permissionsValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<RoleDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await roles.AssignPermissionsAsync(id, request, ct);
        return Ok(ApiResponse<RoleDto>.Ok(item, "Role permissions updated"));
    }
}
