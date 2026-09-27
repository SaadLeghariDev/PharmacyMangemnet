using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Tax;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/tax-profiles")]
public sealed class TaxProfilesController(
    ITaxProfileService taxProfiles,
    IValidator<CreateTaxProfileRequest> createValidator,
    IValidator<UpdateTaxProfileRequest> updateValidator,
    IValidator<CreateTaxRateRequest> createRateValidator,
    IValidator<UpdateTaxRateRequest> updateRateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.TaxView)]
    public async Task<ActionResult<ApiResponse<PagedResult<TaxProfileDto>>>> Search(
        [FromQuery] TaxProfileQuery query, CancellationToken ct)
    {
        var result = await taxProfiles.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<TaxProfileDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.TaxView)]
    public async Task<ActionResult<ApiResponse<TaxProfileDto>>> Get(long id, CancellationToken ct)
    {
        var item = await taxProfiles.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<TaxProfileDto>.Fail("Tax profile not found"));
        return Ok(ApiResponse<TaxProfileDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.TaxEdit)]
    public async Task<ActionResult<ApiResponse<TaxProfileDto>>> Create(
        [FromBody] CreateTaxProfileRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<TaxProfileDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await taxProfiles.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<TaxProfileDto>.Ok(item, "Tax profile created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.TaxEdit)]
    public async Task<ActionResult<ApiResponse<TaxProfileDto>>> Update(
        long id, [FromBody] UpdateTaxProfileRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<TaxProfileDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await taxProfiles.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<TaxProfileDto>.Ok(item, "Tax profile updated"));
    }

    [HttpGet("{id:long}/rates")]
    [Authorize(Policy = PermissionCodes.TaxView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TaxRateDto>>>> GetRates(long id, CancellationToken ct)
    {
        var rates = await taxProfiles.GetRatesAsync(id, ct);
        return Ok(ApiResponse<IReadOnlyList<TaxRateDto>>.Ok(rates));
    }

    [HttpPost("{id:long}/rates")]
    [Authorize(Policy = PermissionCodes.TaxEdit)]
    public async Task<ActionResult<ApiResponse<TaxRateDto>>> AddRate(
        long id, [FromBody] CreateTaxRateRequest request, CancellationToken ct)
    {
        var validation = await createRateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<TaxRateDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var rate = await taxProfiles.AddRateAsync(id, request, ct);
        return Ok(ApiResponse<TaxRateDto>.Ok(rate, "Tax rate added"));
    }

    [HttpPut("{id:long}/rates/{rateId:long}")]
    [Authorize(Policy = PermissionCodes.TaxEdit)]
    public async Task<ActionResult<ApiResponse<TaxRateDto>>> UpdateRate(
        long id, long rateId, [FromBody] UpdateTaxRateRequest request, CancellationToken ct)
    {
        var validation = await updateRateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<TaxRateDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var rate = await taxProfiles.UpdateRateAsync(id, rateId, request, ct);
        return Ok(ApiResponse<TaxRateDto>.Ok(rate, "Tax rate updated"));
    }
}
