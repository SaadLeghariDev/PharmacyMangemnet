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
[Route("api/v1/reason-codes")]
public sealed class ReasonCodesController(
    IReasonCodeService reasonCodes,
    IValidator<CreateReasonCodeRequest> createValidator,
    IValidator<UpdateReasonCodeRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<PagedResult<ReasonCodeDto>>>> Search(
        [FromQuery] ReasonCodeQuery query, CancellationToken ct)
    {
        var result = await reasonCodes.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<ReasonCodeDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<ReasonCodeDto>>> Get(long id, CancellationToken ct)
    {
        var item = await reasonCodes.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<ReasonCodeDto>.Fail("Reason code not found"));
        return Ok(ApiResponse<ReasonCodeDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<ReasonCodeDto>>> Create(
        [FromBody] CreateReasonCodeRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ReasonCodeDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await reasonCodes.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<ReasonCodeDto>.Ok(item, "Reason code created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<ReasonCodeDto>>> Update(
        long id, [FromBody] UpdateReasonCodeRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ReasonCodeDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await reasonCodes.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<ReasonCodeDto>.Ok(item, "Reason code updated"));
    }
}
