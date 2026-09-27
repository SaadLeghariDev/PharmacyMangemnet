using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Fiscal;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/fiscal/documents")]
public sealed class FiscalDocumentsController(
    IFiscalService fiscal,
    IValidator<CreateFiscalDocumentRequest> createValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.FiscalSubmit)]
    public async Task<ActionResult<ApiResponse<PagedResult<FiscalDocumentDto>>>> Search(
        [FromQuery] FiscalDocumentQuery query, CancellationToken ct)
    {
        var result = await fiscal.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<FiscalDocumentDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.FiscalSubmit)]
    public async Task<ActionResult<ApiResponse<FiscalDocumentDto>>> Get(long id, CancellationToken ct)
    {
        var item = await fiscal.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<FiscalDocumentDto>.Fail("Fiscal document not found"));
        return Ok(ApiResponse<FiscalDocumentDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.FiscalSubmit)]
    public async Task<ActionResult<ApiResponse<FiscalDocumentDto>>> Create(
        [FromBody] CreateFiscalDocumentRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<FiscalDocumentDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await fiscal.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<FiscalDocumentDto>.Ok(item, "Fiscal document created"));
    }

    [HttpPost("{id:long}/submit")]
    [Authorize(Policy = PermissionCodes.FiscalSubmit)]
    public async Task<ActionResult<ApiResponse<FiscalDocumentDto>>> Submit(
        long id, [FromBody] FiscalSubmitRequest? request, CancellationToken ct)
    {
        var item = await fiscal.SubmitAsync(id, request ?? new FiscalSubmitRequest(), ct);
        return Ok(ApiResponse<FiscalDocumentDto>.Ok(item, "Fiscal document submitted"));
    }

    [HttpPost("{id:long}/retry")]
    [Authorize(Policy = PermissionCodes.FiscalSubmit)]
    public async Task<ActionResult<ApiResponse<FiscalDocumentDto>>> Retry(
        long id, [FromBody] FiscalSubmitRequest? request, CancellationToken ct)
    {
        var item = await fiscal.RetryAsync(id, request ?? new FiscalSubmitRequest(), ct);
        return Ok(ApiResponse<FiscalDocumentDto>.Ok(item, "Fiscal document retry submitted"));
    }
}
