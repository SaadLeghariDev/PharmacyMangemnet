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
[Route("api/v1/users")]
public sealed class UsersController(
    IUserAdminService users,
    IValidator<CreateUserRequest> createValidator,
    IValidator<UpdateUserRequest> updateValidator,
    IValidator<AssignUserRolesRequest> rolesValidator,
    IValidator<AssignUserBranchesRequest> branchesValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.SecUsers)]
    public async Task<ActionResult<ApiResponse<PagedResult<UserAdminDto>>>> Search(
        [FromQuery] UserAdminQuery query, CancellationToken ct)
    {
        var result = await users.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<UserAdminDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.SecUsers)]
    public async Task<ActionResult<ApiResponse<UserAdminDto>>> Get(long id, CancellationToken ct)
    {
        var item = await users.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<UserAdminDto>.Fail("User not found"));
        return Ok(ApiResponse<UserAdminDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.SecUsers)]
    public async Task<ActionResult<ApiResponse<UserAdminDto>>> Create(
        [FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<UserAdminDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await users.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<UserAdminDto>.Ok(item, "User created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.SecUsers)]
    public async Task<ActionResult<ApiResponse<UserAdminDto>>> Update(
        long id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<UserAdminDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await users.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<UserAdminDto>.Ok(item, "User updated"));
    }

    [HttpPost("{id:long}/deactivate")]
    [Authorize(Policy = PermissionCodes.SecUsers)]
    public async Task<ActionResult<ApiResponse>> Deactivate(long id, CancellationToken ct)
    {
        await users.DeactivateAsync(id, ct);
        return Ok(ApiResponse.Ok("User deactivated"));
    }

    [HttpPut("{id:long}/roles")]
    [Authorize(Policy = PermissionCodes.SecUsers)]
    public async Task<ActionResult<ApiResponse<UserAdminDto>>> AssignRoles(
        long id, [FromBody] AssignUserRolesRequest request, CancellationToken ct)
    {
        var validation = await rolesValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<UserAdminDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await users.AssignRolesAsync(id, request, ct);
        return Ok(ApiResponse<UserAdminDto>.Ok(item, "User roles updated"));
    }

    [HttpPut("{id:long}/branches")]
    [Authorize(Policy = PermissionCodes.SecUsers)]
    public async Task<ActionResult<ApiResponse<UserAdminDto>>> AssignBranches(
        long id, [FromBody] AssignUserBranchesRequest request, CancellationToken ct)
    {
        var validation = await branchesValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<UserAdminDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await users.AssignBranchesAsync(id, request, ct);
        return Ok(ApiResponse<UserAdminDto>.Ok(item, "User branches updated"));
    }
}
