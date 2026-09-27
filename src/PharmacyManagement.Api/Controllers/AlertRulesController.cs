using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Alerts;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/alert-rules")]
public sealed class AlertRulesController(
    IAlertRuleService alertRules,
    IValidator<CreateAlertRuleRequest> createValidator,
    IValidator<UpdateAlertRuleRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.AlertView)]
    public async Task<ActionResult<ApiResponse<PagedResult<AlertRuleDto>>>> Search(
        [FromQuery] AlertRuleQuery query, CancellationToken ct)
    {
        var result = await alertRules.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<AlertRuleDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.AlertView)]
    public async Task<ActionResult<ApiResponse<AlertRuleDto>>> Get(long id, CancellationToken ct)
    {
        var item = await alertRules.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<AlertRuleDto>.Fail("Alert rule not found"));
        return Ok(ApiResponse<AlertRuleDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.AlertManage)]
    public async Task<ActionResult<ApiResponse<AlertRuleDto>>> Create(
        [FromBody] CreateAlertRuleRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<AlertRuleDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await alertRules.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<AlertRuleDto>.Ok(item, "Alert rule created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.AlertManage)]
    public async Task<ActionResult<ApiResponse<AlertRuleDto>>> Update(
        long id, [FromBody] UpdateAlertRuleRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<AlertRuleDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await alertRules.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<AlertRuleDto>.Ok(item, "Alert rule updated"));
    }
}
