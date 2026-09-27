using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Customers;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/customers")]
public sealed class CustomersController(
    ICustomerService customers,
    IValidator<CreateCustomerRequest> createValidator,
    IValidator<UpdateCustomerRequest> updateValidator,
    IValidator<RecordCustomerPaymentRequest> paymentValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.CustView)]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerDto>>>> Search(
        [FromQuery] CustomerQuery query, CancellationToken ct)
    {
        var result = await customers.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<CustomerDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.CustView)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Get(long id, CancellationToken ct)
    {
        var item = await customers.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<CustomerDto>.Fail("Customer not found"));
        return Ok(ApiResponse<CustomerDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.CustEdit)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create(
        [FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<CustomerDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await customers.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<CustomerDto>.Ok(item, "Customer created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.CustEdit)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Update(
        long id, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<CustomerDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await customers.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<CustomerDto>.Ok(item, "Customer updated"));
    }

    [HttpPost("{id:long}/deactivate")]
    [Authorize(Policy = PermissionCodes.CustEdit)]
    public async Task<ActionResult<ApiResponse>> Deactivate(long id, CancellationToken ct)
    {
        await customers.DeactivateAsync(id, ct);
        return Ok(ApiResponse.Ok("Customer deactivated"));
    }

    [HttpGet("{id:long}/ledger")]
    [Authorize(Policy = PermissionCodes.CustView)]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerLedgerEntryDto>>>> Ledger(
        long id, [FromQuery] CustomerLedgerQuery query, CancellationToken ct)
    {
        var result = await customers.GetLedgerAsync(id, query, ct);
        return Ok(ApiResponse<PagedResult<CustomerLedgerEntryDto>>.Ok(result));
    }

    [HttpPost("{id:long}/payments")]
    [Authorize(Policy = PermissionCodes.CustEdit)]
    public async Task<ActionResult<ApiResponse<CustomerPaymentDto>>> RecordPayment(
        long id, [FromBody] RecordCustomerPaymentRequest request, CancellationToken ct)
    {
        var validation = await paymentValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<CustomerPaymentDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await customers.RecordPaymentAsync(id, request, ct);
        return Ok(ApiResponse<CustomerPaymentDto>.Ok(item, "Customer payment recorded"));
    }
}
