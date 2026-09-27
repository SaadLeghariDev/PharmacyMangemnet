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
[Route("api/v1/sale-returns")]
public sealed class SaleReturnsController(
    ISaleReturnService saleReturns,
    IValidator<CreateSaleReturnRequest> createValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.PosReturn)]
    public async Task<ActionResult<ApiResponse<PagedResult<SaleReturnDto>>>> Search([FromQuery] SaleReturnQuery query, CancellationToken ct)
    {
        var result = await saleReturns.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<SaleReturnDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.PosReturn)]
    public async Task<ActionResult<ApiResponse<SaleReturnDto>>> Get(long id, CancellationToken ct)
    {
        var item = await saleReturns.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<SaleReturnDto>.Fail("Sale return not found"));
        return Ok(ApiResponse<SaleReturnDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.PosReturn)]
    public async Task<ActionResult<ApiResponse<SaleReturnDto>>> Create([FromBody] CreateSaleReturnRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SaleReturnDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await saleReturns.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<SaleReturnDto>.Ok(item, "Sale return created"));
    }

    [HttpPost("{id:long}/post")]
    [Authorize(Policy = PermissionCodes.PosReturn)]
    public async Task<ActionResult<ApiResponse<SaleReturnDto>>> Post(long id, CancellationToken ct)
    {
        var item = await saleReturns.PostAsync(id, ct);
        return Ok(ApiResponse<SaleReturnDto>.Ok(item, "Sale return posted"));
    }
}
