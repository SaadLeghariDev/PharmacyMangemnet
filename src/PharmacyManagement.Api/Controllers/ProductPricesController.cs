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
[Route("api/v1/product-prices")]
public sealed class ProductPricesController(
    IProductPriceService productPrices,
    IValidator<CreateProductPriceRequest> createValidator,
    IValidator<UpdateProductPriceRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.PriceView)]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductPriceDto>>>> Search(
        [FromQuery] ProductPriceQuery query, CancellationToken ct)
    {
        var result = await productPrices.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<ProductPriceDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.PriceView)]
    public async Task<ActionResult<ApiResponse<ProductPriceDto>>> Get(long id, CancellationToken ct)
    {
        var item = await productPrices.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<ProductPriceDto>.Fail("Product price not found"));
        return Ok(ApiResponse<ProductPriceDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.PriceEdit)]
    public async Task<ActionResult<ApiResponse<ProductPriceDto>>> Create(
        [FromBody] CreateProductPriceRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ProductPriceDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await productPrices.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<ProductPriceDto>.Ok(item, "Product price created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.PriceEdit)]
    public async Task<ActionResult<ApiResponse<ProductPriceDto>>> Update(
        long id, [FromBody] UpdateProductPriceRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ProductPriceDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await productPrices.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<ProductPriceDto>.Ok(item, "Product price updated"));
    }
}
