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
[Route("api/v1/reorder-rules")]
public sealed class ReorderRulesController(
    IReorderRuleService reorderRules,
    IValidator<CreateReorderRuleRequest> createValidator,
    IValidator<UpdateReorderRuleRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.InvReorder)]
    public async Task<ActionResult<ApiResponse<PagedResult<ReorderRuleDto>>>> Search(
        [FromQuery] ReorderRuleQuery query, CancellationToken ct)
    {
        var result = await reorderRules.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<ReorderRuleDto>>.Ok(result));
    }

    [HttpGet("low-stock")]
    [Authorize(Policy = PermissionCodes.InvReorder)]
    public async Task<ActionResult<ApiResponse<PagedResult<LowStockCandidateDto>>>> LowStock(
        [FromQuery] LowStockCandidateQuery query, CancellationToken ct)
    {
        var result = await reorderRules.GetLowStockCandidatesAsync(query, ct);
        return Ok(ApiResponse<PagedResult<LowStockCandidateDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.InvReorder)]
    public async Task<ActionResult<ApiResponse<ReorderRuleDto>>> Get(long id, CancellationToken ct)
    {
        var item = await reorderRules.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<ReorderRuleDto>.Fail("Reorder rule not found"));
        return Ok(ApiResponse<ReorderRuleDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.InvReorder)]
    public async Task<ActionResult<ApiResponse<ReorderRuleDto>>> Create(
        [FromBody] CreateReorderRuleRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ReorderRuleDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await reorderRules.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<ReorderRuleDto>.Ok(item, "Reorder rule created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.InvReorder)]
    public async Task<ActionResult<ApiResponse<ReorderRuleDto>>> Update(
        long id, [FromBody] UpdateReorderRuleRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ReorderRuleDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await reorderRules.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<ReorderRuleDto>.Ok(item, "Reorder rule updated"));
    }
}
