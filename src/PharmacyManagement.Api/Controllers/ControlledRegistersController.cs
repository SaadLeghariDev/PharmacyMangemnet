using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Controlled;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/controlled-registers")]
public sealed class ControlledRegistersController(
    IControlledDrugService controlled,
    IValidator<OpenControlledRegisterRequest> openValidator,
    IValidator<PostControlledTransactionRequest> txnValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.CtrlManage)]
    public async Task<ActionResult<ApiResponse<PagedResult<ControlledRegisterDto>>>> Search(
        [FromQuery] ControlledRegisterQuery query, CancellationToken ct)
    {
        var result = await controlled.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<ControlledRegisterDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.CtrlManage)]
    public async Task<ActionResult<ApiResponse<ControlledRegisterDto>>> Get(long id, CancellationToken ct)
    {
        var item = await controlled.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<ControlledRegisterDto>.Fail("Controlled register not found"));
        return Ok(ApiResponse<ControlledRegisterDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.CtrlManage)]
    public async Task<ActionResult<ApiResponse<ControlledRegisterDto>>> Open(
        [FromBody] OpenControlledRegisterRequest request, CancellationToken ct)
    {
        var validation = await openValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ControlledRegisterDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await controlled.OpenAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<ControlledRegisterDto>.Ok(item, "Register opened"));
    }

    [HttpGet("{id:long}/transactions")]
    [Authorize(Policy = PermissionCodes.CtrlManage)]
    public async Task<ActionResult<ApiResponse<PagedResult<ControlledTransactionDto>>>> Transactions(
        long id, [FromQuery] ControlledTransactionQuery query, CancellationToken ct)
    {
        var result = await controlled.GetTransactionsAsync(id, query, ct);
        return Ok(ApiResponse<PagedResult<ControlledTransactionDto>>.Ok(result));
    }

    [HttpPost("{id:long}/transactions")]
    [Authorize(Policy = PermissionCodes.CtrlManage)]
    public async Task<ActionResult<ApiResponse<ControlledTransactionDto>>> PostTransaction(
        long id, [FromBody] PostControlledTransactionRequest request, CancellationToken ct)
    {
        var validation = await txnValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ControlledTransactionDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await controlled.PostTransactionAsync(id, request, ct);
        return Ok(ApiResponse<ControlledTransactionDto>.Ok(item, "Transaction posted"));
    }
}
