using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/held-sales")]
public sealed class HeldSalesController(
    IHeldSaleService heldSales,
    IValidator<HoldSaleRequest> holdValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.PosHold)]
    public async Task<ActionResult<ApiResponse<PagedResult<HeldSaleDto>>>> Search([FromQuery] HeldSaleQuery query, CancellationToken ct)
    {
        var result = await heldSales.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<HeldSaleDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.PosHold)]
    public async Task<ActionResult<ApiResponse<HeldSaleDto>>> Get(long id, CancellationToken ct)
    {
        var item = await heldSales.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<HeldSaleDto>.Fail("Held sale not found"));
        return Ok(ApiResponse<HeldSaleDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.PosHold)]
    public async Task<ActionResult<ApiResponse<HeldSaleDto>>> Hold([FromBody] HoldSaleRequest request, CancellationToken ct)
    {
        var validation = await holdValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<HeldSaleDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await heldSales.HoldAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<HeldSaleDto>.Ok(item, "Sale held"));
    }

    [HttpPost("{id:long}/resume")]
    [Authorize(Policy = PermissionCodes.PosHold)]
    public async Task<ActionResult<ApiResponse<HeldSaleDto>>> Resume(long id, CancellationToken ct)
    {
        var item = await heldSales.ResumeAsync(id, ct);
        return Ok(ApiResponse<HeldSaleDto>.Ok(item, "Held sale resumed"));
    }

    [HttpPost("{id:long}/discard")]
    [Authorize(Policy = PermissionCodes.PosHold)]
    public async Task<ActionResult<ApiResponse>> Discard(long id, CancellationToken ct)
    {
        await heldSales.DiscardAsync(id, ct);
        return Ok(ApiResponse.Ok("Held sale discarded"));
    }
}
