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
[Route("api/v1/doctors")]
public sealed class DoctorsController(
    IDoctorService doctors,
    IValidator<CreateDoctorRequest> createValidator,
    IValidator<UpdateDoctorRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.RxDispense)]
    public async Task<ActionResult<ApiResponse<PagedResult<DoctorDto>>>> Search(
        [FromQuery] DoctorQuery query, CancellationToken ct)
    {
        var result = await doctors.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<DoctorDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.RxDispense)]
    public async Task<ActionResult<ApiResponse<DoctorDto>>> Get(long id, CancellationToken ct)
    {
        var item = await doctors.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<DoctorDto>.Fail("Doctor not found"));
        return Ok(ApiResponse<DoctorDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.RxDispense)]
    public async Task<ActionResult<ApiResponse<DoctorDto>>> Create(
        [FromBody] CreateDoctorRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<DoctorDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await doctors.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<DoctorDto>.Ok(item, "Doctor created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.RxDispense)]
    public async Task<ActionResult<ApiResponse<DoctorDto>>> Update(
        long id, [FromBody] UpdateDoctorRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<DoctorDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await doctors.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<DoctorDto>.Ok(item, "Doctor updated"));
    }

    [HttpPost("{id:long}/deactivate")]
    [Authorize(Policy = PermissionCodes.RxDispense)]
    public async Task<ActionResult<ApiResponse>> Deactivate(long id, CancellationToken ct)
    {
        await doctors.DeactivateAsync(id, ct);
        return Ok(ApiResponse.Ok("Doctor deactivated"));
    }
}
