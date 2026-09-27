using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Finance;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/chart-of-accounts")]
public sealed class ChartOfAccountsController(
    IChartOfAccountService accounts,
    IValidator<CreateChartOfAccountRequest> createValidator,
    IValidator<UpdateChartOfAccountRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.FinCoa)]
    public async Task<ActionResult<ApiResponse<PagedResult<ChartOfAccountDto>>>> Search(
        [FromQuery] ChartOfAccountQuery query, CancellationToken ct)
    {
        var result = await accounts.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<ChartOfAccountDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.FinCoa)]
    public async Task<ActionResult<ApiResponse<ChartOfAccountDto>>> Get(long id, CancellationToken ct)
    {
        var item = await accounts.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<ChartOfAccountDto>.Fail("Chart of account not found"));
        return Ok(ApiResponse<ChartOfAccountDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.FinCoa)]
    public async Task<ActionResult<ApiResponse<ChartOfAccountDto>>> Create(
        [FromBody] CreateChartOfAccountRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ChartOfAccountDto>.Fail(
                "Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await accounts.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id },
            ApiResponse<ChartOfAccountDto>.Ok(item, "Account created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.FinCoa)]
    public async Task<ActionResult<ApiResponse<ChartOfAccountDto>>> Update(
        long id, [FromBody] UpdateChartOfAccountRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ChartOfAccountDto>.Fail(
                "Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await accounts.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<ChartOfAccountDto>.Ok(item, "Account updated"));
    }

    [HttpPost("{id:long}/deactivate")]
    [Authorize(Policy = PermissionCodes.FinCoa)]
    public async Task<ActionResult<ApiResponse<ChartOfAccountDto>>> Deactivate(long id, CancellationToken ct)
    {
        var item = await accounts.DeactivateAsync(id, ct);
        return Ok(ApiResponse<ChartOfAccountDto>.Ok(item, "Account deactivated"));
    }
}
