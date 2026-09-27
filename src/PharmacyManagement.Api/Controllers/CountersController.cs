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
[Route("api/v1/counters")]
public sealed class CountersController(
    IOrganizationService org,
    IValidator<CreateCounterRequest> createValidator,
    IValidator<UpdateCounterRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<PagedResult<CounterDto>>>> List([FromQuery] long? branchId, [FromQuery] PaginationQuery query, CancellationToken ct)
    {
        var result = await org.GetCountersAsync(branchId, query, ct);
        return Ok(ApiResponse<PagedResult<CounterDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<CounterDto>>> Get(long id, CancellationToken ct)
    {
        var item = await org.GetCounterAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<CounterDto>.Fail("Counter not found"));
        return Ok(ApiResponse<CounterDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<CounterDto>>> Create([FromBody] CreateCounterRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<CounterDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await org.CreateCounterAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<CounterDto>.Ok(item, "Counter created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<CounterDto>>> Update(long id, [FromBody] UpdateCounterRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<CounterDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await org.UpdateCounterAsync(id, request, ct);
        return Ok(ApiResponse<CounterDto>.Ok(item, "Counter updated"));
    }

    [HttpPost("{id:long}/deactivate")]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse>> Deactivate(long id, CancellationToken ct)
    {
        await org.DeactivateCounterAsync(id, ct);
        return Ok(ApiResponse.Ok("Counter deactivated"));
    }
}
