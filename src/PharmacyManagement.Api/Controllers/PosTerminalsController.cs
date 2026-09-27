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
[Route("api/v1/pos-terminals")]
public sealed class PosTerminalsController(
    IOrganizationService org,
    IValidator<CreatePosTerminalRequest> createValidator,
    IValidator<UpdatePosTerminalRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<PagedResult<PosTerminalDto>>>> List([FromQuery] long? branchId, [FromQuery] PaginationQuery query, CancellationToken ct)
    {
        var result = await org.GetPosTerminalsAsync(branchId, query, ct);
        return Ok(ApiResponse<PagedResult<PosTerminalDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<PosTerminalDto>>> Get(long id, CancellationToken ct)
    {
        var item = await org.GetPosTerminalAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<PosTerminalDto>.Fail("POS terminal not found"));
        return Ok(ApiResponse<PosTerminalDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<PosTerminalDto>>> Create([FromBody] CreatePosTerminalRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<PosTerminalDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await org.CreatePosTerminalAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<PosTerminalDto>.Ok(item, "POS terminal created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<PosTerminalDto>>> Update(long id, [FromBody] UpdatePosTerminalRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<PosTerminalDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await org.UpdatePosTerminalAsync(id, request, ct);
        return Ok(ApiResponse<PosTerminalDto>.Ok(item, "POS terminal updated"));
    }

    [HttpPost("{id:long}/deactivate")]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse>> Deactivate(long id, CancellationToken ct)
    {
        await org.DeactivatePosTerminalAsync(id, ct);
        return Ok(ApiResponse.Ok("POS terminal deactivated"));
    }
}
