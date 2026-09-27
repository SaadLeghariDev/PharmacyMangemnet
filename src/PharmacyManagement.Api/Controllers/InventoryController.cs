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
[Route("api/v1/inventory")]
public sealed class InventoryController(
    IInventoryQueryService inventory,
    IValidator<FefoQuery> fefoValidator) : ControllerBase
{
    [HttpGet("stock")]
    [Authorize(Policy = PermissionCodes.InvView)]
    public async Task<ActionResult<ApiResponse<PagedResult<StockBalanceDto>>>> GetStock([FromQuery] StockQuery query, CancellationToken ct)
    {
        var result = await inventory.GetStockAsync(query, ct);
        return Ok(ApiResponse<PagedResult<StockBalanceDto>>.Ok(result));
    }

    [HttpGet("fefo")]
    [Authorize(Policy = PermissionCodes.InvView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FefoCandidateDto>>>> GetFefo([FromQuery] FefoQuery query, CancellationToken ct)
    {
        var validation = await fefoValidator.ValidateAsync(query, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<IReadOnlyList<FefoCandidateDto>>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var result = await inventory.GetFefoCandidatesAsync(query, ct);
        return Ok(ApiResponse<IReadOnlyList<FefoCandidateDto>>.Ok(result));
    }

    [HttpGet("near-expiry")]
    [Authorize(Policy = PermissionCodes.InvView)]
    public async Task<ActionResult<ApiResponse<PagedResult<StockBalanceDto>>>> GetNearExpiry([FromQuery] NearExpiryQuery query, CancellationToken ct)
    {
        var result = await inventory.GetNearExpiryAsync(query, ct);
        return Ok(ApiResponse<PagedResult<StockBalanceDto>>.Ok(result));
    }
}
