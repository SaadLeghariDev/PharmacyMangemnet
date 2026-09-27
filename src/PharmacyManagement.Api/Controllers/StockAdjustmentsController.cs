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
[Route("api/v1/stock-adjustments")]
public sealed class StockAdjustmentsController(
    IStockAdjustmentService adjustments,
    IValidator<CreateStockAdjustmentRequest> createValidator) : ControllerBase
{
    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.InvAdjust)]
    public async Task<ActionResult<ApiResponse<StockAdjustmentDto>>> Get(long id, CancellationToken ct)
    {
        var item = await adjustments.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<StockAdjustmentDto>.Fail("Stock adjustment not found"));
        return Ok(ApiResponse<StockAdjustmentDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.InvAdjust)]
    public async Task<ActionResult<ApiResponse<StockAdjustmentDto>>> Create([FromBody] CreateStockAdjustmentRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<StockAdjustmentDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await adjustments.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<StockAdjustmentDto>.Ok(item, "Stock adjustment created"));
    }

    [HttpPost("{id:long}/approve")]
    [Authorize(Policy = PermissionCodes.InvAdjust)]
    public async Task<ActionResult<ApiResponse<StockAdjustmentDto>>> Approve(long id, CancellationToken ct)
    {
        var item = await adjustments.ApproveAsync(id, ct);
        return Ok(ApiResponse<StockAdjustmentDto>.Ok(item, "Stock adjustment approved"));
    }

    [HttpPost("{id:long}/post")]
    [Authorize(Policy = PermissionCodes.InvAdjust)]
    public async Task<ActionResult<ApiResponse<StockAdjustmentDto>>> Post(long id, CancellationToken ct)
    {
        var item = await adjustments.PostAsync(id, ct);
        return Ok(ApiResponse<StockAdjustmentDto>.Ok(item, "Stock adjustment posted"));
    }
}
