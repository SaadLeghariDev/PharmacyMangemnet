using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/suppliers")]
public sealed class SuppliersController(
    ISupplierService suppliers,
    IValidator<CreateSupplierRequest> createValidator,
    IValidator<UpdateSupplierRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.ProcPo)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierDto>>>> Search([FromQuery] PaginationQuery query, CancellationToken ct)
    {
        var result = await suppliers.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<SupplierDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.ProcPo)]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Get(long id, CancellationToken ct)
    {
        var item = await suppliers.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<SupplierDto>.Fail("Supplier not found"));
        return Ok(ApiResponse<SupplierDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.ProcPo)]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Create([FromBody] CreateSupplierRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SupplierDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await suppliers.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<SupplierDto>.Ok(item, "Supplier created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.ProcPo)]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Update(long id, [FromBody] UpdateSupplierRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SupplierDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await suppliers.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<SupplierDto>.Ok(item, "Supplier updated"));
    }

    [HttpPost("{id:long}/deactivate")]
    [Authorize(Policy = PermissionCodes.ProcPo)]
    public async Task<ActionResult<ApiResponse>> Deactivate(long id, CancellationToken ct)
    {
        await suppliers.DeactivateAsync(id, ct);
        return Ok(ApiResponse.Ok("Supplier deactivated"));
    }
}
