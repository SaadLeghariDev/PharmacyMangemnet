using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/goods-receipts")]
public sealed class GoodsReceiptsController(
    IGoodsReceiptService goodsReceipts,
    IValidator<CreateGoodsReceiptRequest> createValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.ProcGrn)]
    public async Task<ActionResult<ApiResponse<PagedResult<GoodsReceiptDto>>>> Search([FromQuery] GoodsReceiptQuery query, CancellationToken ct)
    {
        var result = await goodsReceipts.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<GoodsReceiptDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.ProcGrn)]
    public async Task<ActionResult<ApiResponse<GoodsReceiptDto>>> Get(long id, CancellationToken ct)
    {
        var item = await goodsReceipts.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<GoodsReceiptDto>.Fail("Goods receipt not found"));
        return Ok(ApiResponse<GoodsReceiptDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.ProcGrn)]
    public async Task<ActionResult<ApiResponse<GoodsReceiptDto>>> CreateDraft([FromBody] CreateGoodsReceiptRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<GoodsReceiptDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await goodsReceipts.CreateDraftAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<GoodsReceiptDto>.Ok(item, "Goods receipt draft created"));
    }

    [HttpPost("{id:long}/post")]
    [Authorize(Policy = PermissionCodes.ProcGrn)]
    public async Task<ActionResult<ApiResponse<GoodsReceiptDto>>> Post(long id, CancellationToken ct)
    {
        var item = await goodsReceipts.PostAsync(id, ct);
        return Ok(ApiResponse<GoodsReceiptDto>.Ok(item, "Goods receipt posted"));
    }
}
