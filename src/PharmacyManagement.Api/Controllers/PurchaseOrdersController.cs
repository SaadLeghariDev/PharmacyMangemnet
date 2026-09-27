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
[Route("api/v1/purchase-orders")]
public sealed class PurchaseOrdersController(
    IPurchaseOrderService purchaseOrders,
    IValidator<CreatePurchaseOrderRequest> createValidator,
    IValidator<UpdatePurchaseOrderRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.ProcPo)]
    public async Task<ActionResult<ApiResponse<PagedResult<PurchaseOrderDto>>>> Search([FromQuery] PurchaseOrderQuery query, CancellationToken ct)
    {
        var result = await purchaseOrders.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<PurchaseOrderDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.ProcPo)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Get(long id, CancellationToken ct)
    {
        var item = await purchaseOrders.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<PurchaseOrderDto>.Fail("Purchase order not found"));
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.ProcPo)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Create([FromBody] CreatePurchaseOrderRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<PurchaseOrderDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await purchaseOrders.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<PurchaseOrderDto>.Ok(item, "Purchase order created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.ProcPo)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Update(long id, [FromBody] UpdatePurchaseOrderRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<PurchaseOrderDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await purchaseOrders.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(item, "Purchase order updated"));
    }

    [HttpPost("{id:long}/submit")]
    [Authorize(Policy = PermissionCodes.ProcPo)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Submit(long id, CancellationToken ct)
    {
        var item = await purchaseOrders.SubmitAsync(id, ct);
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(item, "Purchase order submitted"));
    }

    [HttpPost("{id:long}/approve")]
    [Authorize(Policy = PermissionCodes.ProcPo)]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Approve(long id, CancellationToken ct)
    {
        var item = await purchaseOrders.ApproveAsync(id, ct);
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(item, "Purchase order approved"));
    }
}
