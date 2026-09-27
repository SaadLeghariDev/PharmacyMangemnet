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
[Route("api/v1/expenses")]
public sealed class ExpensesController(
    IExpenseService expenses,
    IValidator<CreateExpenseRequest> createValidator,
    IValidator<UpdateExpenseRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.FinExpense)]
    public async Task<ActionResult<ApiResponse<PagedResult<ExpenseDto>>>> Search(
        [FromQuery] ExpenseQuery query, CancellationToken ct)
    {
        var result = await expenses.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<ExpenseDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.FinExpense)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> Get(long id, CancellationToken ct)
    {
        var item = await expenses.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<ExpenseDto>.Fail("Expense not found"));
        return Ok(ApiResponse<ExpenseDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.FinExpense)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> Create(
        [FromBody] CreateExpenseRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ExpenseDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await expenses.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<ExpenseDto>.Ok(item, "Expense created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.FinExpense)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> Update(
        long id, [FromBody] UpdateExpenseRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ExpenseDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await expenses.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<ExpenseDto>.Ok(item, "Expense updated"));
    }
}
