using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Organization;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/branches")]
public sealed class BranchesController(
    IOrganizationService org,
    IValidator<CreateBranchRequest> createValidator,
    IValidator<UpdateBranchRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<PagedResult<BranchDto>>>> List([FromQuery] PaginationQuery query, CancellationToken ct)
    {
        var result = await org.GetBranchesAsync(query, ct);
        return Ok(ApiResponse<PagedResult<BranchDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<BranchDto>>> Get(long id, CancellationToken ct)
    {
        var item = await org.GetBranchAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<BranchDto>.Fail("Branch not found"));
        return Ok(ApiResponse<BranchDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<BranchDto>>> Create([FromBody] CreateBranchRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<BranchDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await org.CreateBranchAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<BranchDto>.Ok(item, "Branch created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<BranchDto>>> Update(long id, [FromBody] UpdateBranchRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<BranchDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await org.UpdateBranchAsync(id, request, ct);
        return Ok(ApiResponse<BranchDto>.Ok(item, "Branch updated"));
    }

    [HttpPost("{id:long}/deactivate")]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse>> Deactivate(long id, CancellationToken ct)
    {
        await org.DeactivateBranchAsync(id, ct);
        return Ok(ApiResponse.Ok("Branch deactivated"));
    }
}
