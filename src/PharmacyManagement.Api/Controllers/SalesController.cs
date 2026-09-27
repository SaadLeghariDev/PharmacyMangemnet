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
[Route("api/v1/sales")]
public sealed class SalesController(
    ISaleService sales,
    IValidator<CreateSaleRequest> createValidator,
    IValidator<RecordSalePaymentRequest> paymentValidator,
    IValidator<VoidSaleRequest> voidValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.PosSale)]
    public async Task<ActionResult<ApiResponse<PagedResult<SaleDto>>>> Search([FromQuery] SaleQuery query, CancellationToken ct)
    {
        var result = await sales.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<SaleDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.PosSale)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> Get(long id, CancellationToken ct)
    {
        var item = await sales.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<SaleDto>.Fail("Sale not found"));
        return Ok(ApiResponse<SaleDto>.Ok(item));
    }

    [HttpGet("{id:long}/receipt")]
    [Authorize(Policy = PermissionCodes.PosSale)]
    public async Task<ActionResult<ApiResponse<SaleReceiptDto>>> Receipt(long id, CancellationToken ct)
    {
        var item = await sales.GetReceiptAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<SaleReceiptDto>.Fail("Sale not found"));
        return Ok(ApiResponse<SaleReceiptDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.PosSale)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> Create([FromBody] CreateSaleRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SaleDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await sales.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<SaleDto>.Ok(item, "Sale completed"));
    }

    [HttpPost("{id:long}/payments")]
    [Authorize(Policy = PermissionCodes.PosSale)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> RecordPayment(long id, [FromBody] RecordSalePaymentRequest request, CancellationToken ct)
    {
        var validation = await paymentValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SaleDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await sales.RecordPaymentAsync(id, request, ct);
        return Ok(ApiResponse<SaleDto>.Ok(item, "Payment recorded"));
    }

    [HttpPost("{id:long}/void")]
    [Authorize(Policy = PermissionCodes.PosVoid)]
    public async Task<ActionResult<ApiResponse<SaleDto>>> Void(long id, [FromBody] VoidSaleRequest? request, CancellationToken ct)
    {
        request ??= new VoidSaleRequest();
        var validation = await voidValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SaleDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await sales.VoidAsync(id, request, ct);
        return Ok(ApiResponse<SaleDto>.Ok(item, "Sale voided"));
    }
}
