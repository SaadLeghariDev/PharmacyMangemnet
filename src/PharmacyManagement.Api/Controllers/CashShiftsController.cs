using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Cash;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/cash-shifts")]
public sealed class CashShiftsController(
    ICashShiftService cashShifts,
    IValidator<OpenCashShiftRequest> openValidator,
    IValidator<CashDrawerMovementRequest> movementValidator,
    IValidator<CloseCashShiftRequest> closeValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.FinCash)]
    public async Task<ActionResult<ApiResponse<PagedResult<CashShiftDto>>>> Search(
        [FromQuery] CashShiftQuery query, CancellationToken ct)
    {
        var result = await cashShifts.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<CashShiftDto>>.Ok(result));
    }

    [HttpGet("current")]
    [Authorize(Policy = PermissionCodes.FinCash)]
    public async Task<ActionResult<ApiResponse<CashShiftDto>>> Current([FromQuery] long terminalId, CancellationToken ct)
    {
        if (terminalId <= 0)
            return BadRequest(ApiResponse<CashShiftDto>.Fail("terminalId is required."));
        var item = await cashShifts.GetCurrentAsync(terminalId, ct);
        if (item is null) return NotFound(ApiResponse<CashShiftDto>.Fail("No open cash shift for terminal"));
        return Ok(ApiResponse<CashShiftDto>.Ok(item));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.FinCash)]
    public async Task<ActionResult<ApiResponse<CashShiftDto>>> Get(long id, CancellationToken ct)
    {
        var item = await cashShifts.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<CashShiftDto>.Fail("Cash shift not found"));
        return Ok(ApiResponse<CashShiftDto>.Ok(item));
    }

    [HttpPost("open")]
    [Authorize(Policy = PermissionCodes.FinCash)]
    public async Task<ActionResult<ApiResponse<CashShiftDto>>> Open(
        [FromBody] OpenCashShiftRequest request, CancellationToken ct)
    {
        var validation = await openValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<CashShiftDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await cashShifts.OpenAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<CashShiftDto>.Ok(item, "Cash shift opened"));
    }

    [HttpPost("{id:long}/pay-in")]
    [Authorize(Policy = PermissionCodes.FinCash)]
    public async Task<ActionResult<ApiResponse<CashShiftDto>>> PayIn(
        long id, [FromBody] CashDrawerMovementRequest request, CancellationToken ct)
    {
        var validation = await movementValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<CashShiftDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await cashShifts.PayInAsync(id, request, ct);
        return Ok(ApiResponse<CashShiftDto>.Ok(item, "Pay-in recorded"));
    }

    [HttpPost("{id:long}/pay-out")]
    [Authorize(Policy = PermissionCodes.FinCash)]
    public async Task<ActionResult<ApiResponse<CashShiftDto>>> PayOut(
        long id, [FromBody] CashDrawerMovementRequest request, CancellationToken ct)
    {
        var validation = await movementValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<CashShiftDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await cashShifts.PayOutAsync(id, request, ct);
        return Ok(ApiResponse<CashShiftDto>.Ok(item, "Pay-out recorded"));
    }

    [HttpPost("{id:long}/close")]
    [Authorize(Policy = PermissionCodes.FinCash)]
    public async Task<ActionResult<ApiResponse<CashShiftDto>>> Close(
        long id, [FromBody] CloseCashShiftRequest request, CancellationToken ct)
    {
        var validation = await closeValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<CashShiftDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await cashShifts.CloseAsync(id, request, ct);
        return Ok(ApiResponse<CashShiftDto>.Ok(item, "Cash shift closed"));
    }
}
