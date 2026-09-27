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
[Route("api/v1/branch-settings")]
public sealed class BranchSettingsController(
    ISettingsService settings,
    IValidator<UpsertSettingRequest> upsertValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<PagedResult<BranchSettingDto>>>> Search(
        [FromQuery] BranchSettingQuery query, CancellationToken ct)
    {
        var result = await settings.SearchBranchSettingsAsync(query, ct);
        return Ok(ApiResponse<PagedResult<BranchSettingDto>>.Ok(result));
    }

    [HttpGet("{branchId:long}/{key}")]
    [Authorize(Policy = PermissionCodes.OrgView)]
    public async Task<ActionResult<ApiResponse<BranchSettingDto>>> Get(long branchId, string key, CancellationToken ct)
    {
        var item = await settings.GetBranchSettingAsync(branchId, key, ct);
        if (item is null) return NotFound(ApiResponse<BranchSettingDto>.Fail("Branch setting not found"));
        return Ok(ApiResponse<BranchSettingDto>.Ok(item));
    }

    [HttpPut("{branchId:long}")]
    [Authorize(Policy = PermissionCodes.OrgEdit)]
    public async Task<ActionResult<ApiResponse<BranchSettingDto>>> Upsert(
        long branchId, [FromBody] UpsertSettingRequest request, CancellationToken ct)
    {
        var validation = await upsertValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<BranchSettingDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await settings.UpsertBranchSettingAsync(branchId, request, ct);
        return Ok(ApiResponse<BranchSettingDto>.Ok(item, "Branch setting saved"));
    }
}
