using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Expenses;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/expense-categories")]
public sealed class ExpenseCategoriesController(
    IExpenseCategoryService categories,
    IValidator<CreateExpenseCategoryRequest> createValidator,
    IValidator<UpdateExpenseCategoryRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.FinExpense)]
    public async Task<ActionResult<ApiResponse<PagedResult<ExpenseCategoryDto>>>> Search(
        [FromQuery] ExpenseCategoryQuery query, CancellationToken ct)
    {
        var result = await categories.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<ExpenseCategoryDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.FinExpense)]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> Get(long id, CancellationToken ct)
    {
        var item = await categories.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<ExpenseCategoryDto>.Fail("Expense category not found"));
        return Ok(ApiResponse<ExpenseCategoryDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.FinExpense)]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> Create(
        [FromBody] CreateExpenseCategoryRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ExpenseCategoryDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await categories.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<ExpenseCategoryDto>.Ok(item, "Expense category created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.FinExpense)]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> Update(
        long id, [FromBody] UpdateExpenseCategoryRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ExpenseCategoryDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await categories.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<ExpenseCategoryDto>.Ok(item, "Expense category updated"));
    }
}
