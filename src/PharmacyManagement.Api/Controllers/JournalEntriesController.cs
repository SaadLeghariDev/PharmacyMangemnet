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
[Route("api/v1/journal-entries")]
public sealed class JournalEntriesController(
    IJournalEntryService journals,
    IValidator<CreateJournalEntryRequest> createValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.FinJournal)]
    public async Task<ActionResult<ApiResponse<PagedResult<JournalEntryDto>>>> Search(
        [FromQuery] JournalEntryQuery query, CancellationToken ct)
    {
        var result = await journals.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<JournalEntryDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.FinJournal)]
    public async Task<ActionResult<ApiResponse<JournalEntryDto>>> Get(long id, CancellationToken ct)
    {
        var item = await journals.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<JournalEntryDto>.Fail("Journal entry not found"));
        return Ok(ApiResponse<JournalEntryDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.FinJournal)]
    public async Task<ActionResult<ApiResponse<JournalEntryDto>>> Create(
        [FromBody] CreateJournalEntryRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<JournalEntryDto>.Fail(
                "Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await journals.CreateDraftAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id },
            ApiResponse<JournalEntryDto>.Ok(item, "Journal draft created"));
    }

    [HttpPost("{id:long}/post")]
    [Authorize(Policy = PermissionCodes.FinJournal)]
    public async Task<ActionResult<ApiResponse<JournalEntryDto>>> Post(long id, CancellationToken ct)
    {
        var item = await journals.PostAsync(id, ct);
        return Ok(ApiResponse<JournalEntryDto>.Ok(item, "Journal posted"));
    }

    [HttpPost("{id:long}/reverse")]
    [Authorize(Policy = PermissionCodes.FinJournal)]
    public async Task<ActionResult<ApiResponse<JournalEntryDto>>> Reverse(long id, CancellationToken ct)
    {
        var item = await journals.ReverseAsync(id, ct);
        return Ok(ApiResponse<JournalEntryDto>.Ok(item, "Journal reversed"));
    }
}
