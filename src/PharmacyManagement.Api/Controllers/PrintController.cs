using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Hardware;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/print-templates")]
public sealed class PrintTemplatesController(
    IPrintService print,
    IValidator<CreatePrintTemplateRequest> createValidator,
    IValidator<UpdatePrintTemplateRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.PrintManage)]
    public async Task<ActionResult<ApiResponse<PagedResult<PrintTemplateDto>>>> Search(
        [FromQuery] PrintTemplateQuery query, CancellationToken ct)
    {
        var result = await print.SearchTemplatesAsync(query, ct);
        return Ok(ApiResponse<PagedResult<PrintTemplateDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.PrintManage)]
    public async Task<ActionResult<ApiResponse<PrintTemplateDto>>> Get(long id, CancellationToken ct)
    {
        var item = await print.GetTemplateByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<PrintTemplateDto>.Fail("Print template not found"));
        return Ok(ApiResponse<PrintTemplateDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.PrintManage)]
    public async Task<ActionResult<ApiResponse<PrintTemplateDto>>> Create(
        [FromBody] CreatePrintTemplateRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<PrintTemplateDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await print.CreateTemplateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<PrintTemplateDto>.Ok(item, "Print template created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.PrintManage)]
    public async Task<ActionResult<ApiResponse<PrintTemplateDto>>> Update(
        long id, [FromBody] UpdatePrintTemplateRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<PrintTemplateDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await print.UpdateTemplateAsync(id, request, ct);
        return Ok(ApiResponse<PrintTemplateDto>.Ok(item, "Print template updated"));
    }
}

[ApiController]
[Authorize]
[Route("api/v1/barcode-print-jobs")]
public sealed class BarcodePrintJobsController(
    IPrintService print,
    IValidator<CreateBarcodePrintJobRequest> createValidator,
    IValidator<UpdateBarcodePrintJobStatusRequest> statusValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.PrintManage)]
    public async Task<ActionResult<ApiResponse<PagedResult<BarcodePrintJobDto>>>> Search(
        [FromQuery] BarcodePrintJobQuery query, CancellationToken ct)
    {
        var result = await print.SearchJobsAsync(query, ct);
        return Ok(ApiResponse<PagedResult<BarcodePrintJobDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.PrintManage)]
    public async Task<ActionResult<ApiResponse<BarcodePrintJobDto>>> Get(long id, CancellationToken ct)
    {
        var item = await print.GetJobByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<BarcodePrintJobDto>.Fail("Barcode print job not found"));
        return Ok(ApiResponse<BarcodePrintJobDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.PrintManage)]
    public async Task<ActionResult<ApiResponse<BarcodePrintJobDto>>> Create(
        [FromBody] CreateBarcodePrintJobRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<BarcodePrintJobDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await print.CreateJobAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<BarcodePrintJobDto>.Ok(item, "Print job queued"));
    }

    [HttpPut("{id:long}/status")]
    [Authorize(Policy = PermissionCodes.PrintManage)]
    public async Task<ActionResult<ApiResponse<BarcodePrintJobDto>>> UpdateStatus(
        long id, [FromBody] UpdateBarcodePrintJobStatusRequest request, CancellationToken ct)
    {
        var validation = await statusValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<BarcodePrintJobDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await print.UpdateJobStatusAsync(id, request, ct);
        return Ok(ApiResponse<BarcodePrintJobDto>.Ok(item, "Print job status updated"));
    }

    [HttpPost("{id:long}/simulate-complete")]
    [Authorize(Policy = PermissionCodes.PrintManage)]
    public async Task<ActionResult<ApiResponse<BarcodePrintJobDto>>> SimulateComplete(
        long id, [FromBody] SimulateBarcodePrintJobRequest? request, CancellationToken ct)
    {
        var item = await print.SimulateCompleteAsync(id, request ?? new SimulateBarcodePrintJobRequest(), ct);
        return Ok(ApiResponse<BarcodePrintJobDto>.Ok(item, request?.Fail == true ? "Print job marked failed" : "Print job simulated as printed"));
    }
}
