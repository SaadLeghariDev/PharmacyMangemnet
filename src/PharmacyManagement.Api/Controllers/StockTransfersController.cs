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
[Route("api/v1/stock-transfers")]
public sealed class StockTransfersController(
    IStockTransferService transfers,
    IValidator<CreateStockTransferRequest> createValidator) : ControllerBase
{
    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.InvTransfer)]
    public async Task<ActionResult<ApiResponse<StockTransferDto>>> Get(long id, CancellationToken ct)
    {
        var item = await transfers.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<StockTransferDto>.Fail("Stock transfer not found"));
        return Ok(ApiResponse<StockTransferDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.InvTransfer)]
    public async Task<ActionResult<ApiResponse<StockTransferDto>>> Create([FromBody] CreateStockTransferRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<StockTransferDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await transfers.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<StockTransferDto>.Ok(item, "Stock transfer created"));
    }

    [HttpPost("{id:long}/complete")]
    [Authorize(Policy = PermissionCodes.InvTransfer)]
    public async Task<ActionResult<ApiResponse<StockTransferDto>>> Complete(long id, CancellationToken ct)
    {
        var item = await transfers.CompleteAsync(id, ct);
        return Ok(ApiResponse<StockTransferDto>.Ok(item, "Stock transfer completed"));
    }
}
