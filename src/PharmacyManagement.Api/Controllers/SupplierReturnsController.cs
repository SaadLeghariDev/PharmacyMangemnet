using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Procurement;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/supplier-returns")]
public sealed class SupplierReturnsController(
    ISupplierReturnService returns,
    IValidator<CreateSupplierReturnRequest> createValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.ProcSupplierReturn)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierReturnDto>>>> Search(
        [FromQuery] SupplierReturnQuery query, CancellationToken ct)
    {
        var result = await returns.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<SupplierReturnDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.ProcSupplierReturn)]
    public async Task<ActionResult<ApiResponse<SupplierReturnDto>>> Get(long id, CancellationToken ct)
    {
        var item = await returns.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<SupplierReturnDto>.Fail("Supplier return not found"));
        return Ok(ApiResponse<SupplierReturnDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.ProcSupplierReturn)]
    public async Task<ActionResult<ApiResponse<SupplierReturnDto>>> Create(
        [FromBody] CreateSupplierReturnRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SupplierReturnDto>.Fail(
                "Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await returns.CreateDraftAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id },
            ApiResponse<SupplierReturnDto>.Ok(item, "Supplier return draft created"));
    }

    [HttpPost("{id:long}/post")]
    [Authorize(Policy = PermissionCodes.ProcSupplierReturn)]
    public async Task<ActionResult<ApiResponse<SupplierReturnDto>>> Post(long id, CancellationToken ct)
    {
        var item = await returns.PostAsync(id, ct);
        return Ok(ApiResponse<SupplierReturnDto>.Ok(item, "Supplier return posted"));
    }

    [HttpPost("{id:long}/cancel")]
    [Authorize(Policy = PermissionCodes.ProcSupplierReturn)]
    public async Task<ActionResult<ApiResponse<SupplierReturnDto>>> Cancel(long id, CancellationToken ct)
    {
        var item = await returns.CancelAsync(id, ct);
        return Ok(ApiResponse<SupplierReturnDto>.Ok(item, "Supplier return cancelled"));
    }
}
