using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Pricing;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/price-lists")]
public sealed class PriceListsController(
    IPriceListService priceLists,
    IValidator<CreatePriceListRequest> createValidator,
    IValidator<UpdatePriceListRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.PriceView)]
    public async Task<ActionResult<ApiResponse<PagedResult<PriceListDto>>>> Search(
        [FromQuery] PriceListQuery query, CancellationToken ct)
    {
        var result = await priceLists.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<PriceListDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.PriceView)]
    public async Task<ActionResult<ApiResponse<PriceListDto>>> Get(long id, CancellationToken ct)
    {
        var item = await priceLists.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<PriceListDto>.Fail("Price list not found"));
        return Ok(ApiResponse<PriceListDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.PriceEdit)]
    public async Task<ActionResult<ApiResponse<PriceListDto>>> Create(
        [FromBody] CreatePriceListRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<PriceListDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await priceLists.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<PriceListDto>.Ok(item, "Price list created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.PriceEdit)]
    public async Task<ActionResult<ApiResponse<PriceListDto>>> Update(
        long id, [FromBody] UpdatePriceListRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<PriceListDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await priceLists.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<PriceListDto>.Ok(item, "Price list updated"));
    }
}
