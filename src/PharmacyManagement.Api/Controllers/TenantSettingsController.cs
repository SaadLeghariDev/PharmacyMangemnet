using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Admin;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/tenant-settings")]
public sealed class TenantSettingsController(
    ISettingsService settings,
    IValidator<UpsertSettingRequest> upsertValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<PagedResult<TenantSettingDto>>>> Search(
        [FromQuery] SettingQuery query, CancellationToken ct)
    {
        var result = await settings.SearchTenantSettingsAsync(query, ct);
        return Ok(ApiResponse<PagedResult<TenantSettingDto>>.Ok(result));
    }

    [HttpGet("{key}")]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<TenantSettingDto>>> Get(string key, CancellationToken ct)
    {
        var item = await settings.GetTenantSettingAsync(key, ct);
        if (item is null) return NotFound(ApiResponse<TenantSettingDto>.Fail("Tenant setting not found"));
        return Ok(ApiResponse<TenantSettingDto>.Ok(item));
    }

    [HttpPut]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<TenantSettingDto>>> Upsert(
        [FromBody] UpsertSettingRequest request, CancellationToken ct)
    {
        var validation = await upsertValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<TenantSettingDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await settings.UpsertTenantSettingAsync(request, ct);
        return Ok(ApiResponse<TenantSettingDto>.Ok(item, "Tenant setting saved"));
    }
}
