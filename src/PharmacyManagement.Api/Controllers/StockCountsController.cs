using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/stock-counts")]
public sealed class StockCountsController(
    IStockCountService counts,
    IValidator<CreateStockCountRequest> createValidator) : ControllerBase
{
    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.InvAdjust)]
    public async Task<ActionResult<ApiResponse<StockCountDto>>> Get(long id, CancellationToken ct)
    {
        var item = await counts.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<StockCountDto>.Fail("Stock count not found"));
        return Ok(ApiResponse<StockCountDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.InvAdjust)]
    public async Task<ActionResult<ApiResponse<StockCountDto>>> Create([FromBody] CreateStockCountRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<StockCountDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await counts.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<StockCountDto>.Ok(item, "Stock count created"));
    }

    [HttpPost("{id:long}/complete")]
    [Authorize(Policy = PermissionCodes.InvAdjust)]
    public async Task<ActionResult<ApiResponse<StockCountDto>>> Complete(long id, CancellationToken ct)
    {
        var item = await counts.CompleteAsync(id, ct);
        return Ok(ApiResponse<StockCountDto>.Ok(item, "Stock count completed"));
    }
}
