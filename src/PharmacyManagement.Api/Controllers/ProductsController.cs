using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Products;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/products")]
public sealed class ProductsController(
    IProductService products,
    IValidator<CreateProductRequest> createValidator,
    IValidator<UpdateProductRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.ProdView)]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> Search([FromQuery] ProductQuery query, CancellationToken ct)
    {
        var result = await products.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.ProdView)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Get(long id, CancellationToken ct)
    {
        var item = await products.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<ProductDto>.Fail("Product not found"));
        return Ok(ApiResponse<ProductDto>.Ok(item));
    }

    [HttpGet("by-sku/{sku}")]
    [Authorize(Policy = PermissionCodes.ProdView)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetBySku(string sku, CancellationToken ct)
    {
        var item = await products.GetBySkuAsync(sku, ct);
        if (item is null) return NotFound(ApiResponse<ProductDto>.Fail("Product not found"));
        return Ok(ApiResponse<ProductDto>.Ok(item));
    }

    [HttpGet("by-barcode/{barcode}")]
    [Authorize(Policy = PermissionCodes.ProdView)]
    public async Task<ActionResult<ApiResponse<BarcodeLookupDto>>> GetByBarcode(string barcode, CancellationToken ct)
    {
        var item = await products.GetByBarcodeAsync(barcode, ct);
        if (item is null) return NotFound(ApiResponse<BarcodeLookupDto>.Fail("Barcode not found"));
        return Ok(ApiResponse<BarcodeLookupDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.ProdEdit)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ProductDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await products.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<ProductDto>.Ok(item, "Product created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.ProdEdit)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(long id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ProductDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await products.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<ProductDto>.Ok(item, "Product updated"));
    }

    [HttpPost("{id:long}/deactivate")]
    [Authorize(Policy = PermissionCodes.ProdEdit)]
    public async Task<ActionResult<ApiResponse>> Deactivate(long id, CancellationToken ct)
    {
        await products.DeactivateAsync(id, ct);
        return Ok(ApiResponse.Ok("Product deactivated"));
    }
}
