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
[Route("api/v1/warehouse-locations")]
public sealed class WarehouseLocationsController(
    IOrganizationService org,
    IValidator<CreateWarehouseLocationRequest> createValidator,
    IValidator<UpdateWarehouseLocationRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<PagedResult<WarehouseLocationDto>>>> List([FromQuery] long? warehouseId, [FromQuery] PaginationQuery query, CancellationToken ct)
    {
        var result = await org.GetWarehouseLocationsAsync(warehouseId, query, ct);
        return Ok(ApiResponse<PagedResult<WarehouseLocationDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<WarehouseLocationDto>>> Get(long id, CancellationToken ct)
    {
        var item = await org.GetWarehouseLocationAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<WarehouseLocationDto>.Fail("Warehouse location not found"));
        return Ok(ApiResponse<WarehouseLocationDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<WarehouseLocationDto>>> Create([FromBody] CreateWarehouseLocationRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<WarehouseLocationDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await org.CreateWarehouseLocationAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<WarehouseLocationDto>.Ok(item, "Location created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<WarehouseLocationDto>>> Update(long id, [FromBody] UpdateWarehouseLocationRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<WarehouseLocationDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await org.UpdateWarehouseLocationAsync(id, request, ct);
        return Ok(ApiResponse<WarehouseLocationDto>.Ok(item, "Location updated"));
    }

    [HttpPost("{id:long}/deactivate")]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse>> Deactivate(long id, CancellationToken ct)
    {
        await org.DeactivateWarehouseLocationAsync(id, ct);
        return Ok(ApiResponse.Ok("Location deactivated"));
    }
}
