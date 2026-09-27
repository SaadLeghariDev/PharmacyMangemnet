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
[Route("api/v1/warehouses")]
public sealed class WarehousesController(
    IOrganizationService org,
    IValidator<CreateWarehouseRequest> createValidator,
    IValidator<UpdateWarehouseRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<PagedResult<WarehouseDto>>>> List([FromQuery] long? branchId, [FromQuery] PaginationQuery query, CancellationToken ct)
    {
        var result = await org.GetWarehousesAsync(branchId, query, ct);
        return Ok(ApiResponse<PagedResult<WarehouseDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<WarehouseDto>>> Get(long id, CancellationToken ct)
    {
        var item = await org.GetWarehouseAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<WarehouseDto>.Fail("Warehouse not found"));
        return Ok(ApiResponse<WarehouseDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<WarehouseDto>>> Create([FromBody] CreateWarehouseRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<WarehouseDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await org.CreateWarehouseAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<WarehouseDto>.Ok(item, "Warehouse created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<WarehouseDto>>> Update(long id, [FromBody] UpdateWarehouseRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<WarehouseDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await org.UpdateWarehouseAsync(id, request, ct);
        return Ok(ApiResponse<WarehouseDto>.Ok(item, "Warehouse updated"));
    }

    [HttpPost("{id:long}/deactivate")]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse>> Deactivate(long id, CancellationToken ct)
    {
        await org.DeactivateWarehouseAsync(id, ct);
        return Ok(ApiResponse.Ok("Warehouse deactivated"));
    }
}
