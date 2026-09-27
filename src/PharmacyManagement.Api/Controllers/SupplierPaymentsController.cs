using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Procurement;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/supplier-payments")]
public sealed class SupplierPaymentsController(
    ISupplierPaymentService payments,
    IValidator<CreateSupplierPaymentRequest> createValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.ProcSupplierPay)]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierPaymentDto>>>> Search(
        [FromQuery] SupplierPaymentQuery query, CancellationToken ct)
    {
        var result = await payments.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<SupplierPaymentDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.ProcSupplierPay)]
    public async Task<ActionResult<ApiResponse<SupplierPaymentDto>>> Get(long id, CancellationToken ct)
    {
        var item = await payments.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<SupplierPaymentDto>.Fail("Supplier payment not found"));
        return Ok(ApiResponse<SupplierPaymentDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.ProcSupplierPay)]
    public async Task<ActionResult<ApiResponse<SupplierPaymentDto>>> Create(
        [FromBody] CreateSupplierPaymentRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SupplierPaymentDto>.Fail(
                "Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await payments.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id },
            ApiResponse<SupplierPaymentDto>.Ok(item, "Supplier payment recorded"));
    }
}
