using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Prescriptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/prescriptions")]
public sealed class PrescriptionsController(
    IPrescriptionService prescriptions,
    IValidator<CreatePrescriptionRequest> createValidator,
    IValidator<DispensePrescriptionRequest> dispenseValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.RxDispense)]
    public async Task<ActionResult<ApiResponse<PagedResult<PrescriptionDto>>>> Search(
        [FromQuery] PrescriptionQuery query, CancellationToken ct)
    {
        var result = await prescriptions.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<PrescriptionDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.RxDispense)]
    public async Task<ActionResult<ApiResponse<PrescriptionDto>>> Get(long id, CancellationToken ct)
    {
        var item = await prescriptions.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<PrescriptionDto>.Fail("Prescription not found"));
        return Ok(ApiResponse<PrescriptionDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.RxDispense)]
    public async Task<ActionResult<ApiResponse<PrescriptionDto>>> Create(
        [FromBody] CreatePrescriptionRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<PrescriptionDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await prescriptions.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<PrescriptionDto>.Ok(item, "Prescription created"));
    }

    [HttpPost("{id:long}/cancel")]
    [Authorize(Policy = PermissionCodes.RxDispense)]
    public async Task<ActionResult<ApiResponse<PrescriptionDto>>> Cancel(long id, CancellationToken ct)
    {
        var item = await prescriptions.CancelAsync(id, ct);
        return Ok(ApiResponse<PrescriptionDto>.Ok(item, "Prescription cancelled"));
    }

    [HttpPost("{id:long}/dispense")]
    [Authorize(Policy = PermissionCodes.RxDispense)]
    public async Task<ActionResult<ApiResponse<PrescriptionDto>>> Dispense(
        long id, [FromBody] DispensePrescriptionRequest request, CancellationToken ct)
    {
        var validation = await dispenseValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<PrescriptionDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await prescriptions.DispenseAsync(id, request, ct);
        return Ok(ApiResponse<PrescriptionDto>.Ok(item, "Prescription dispensed"));
    }
}
