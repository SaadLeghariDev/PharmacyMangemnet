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
[Route("api/v1/attachments")]
public sealed class AttachmentsController(
    IAttachmentService attachments,
    IValidator<CreateAttachmentRequest> createValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.HwView)]
    public async Task<ActionResult<ApiResponse<PagedResult<AttachmentDto>>>> Search(
        [FromQuery] AttachmentQuery query, CancellationToken ct)
    {
        var result = await attachments.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<AttachmentDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.HwView)]
    public async Task<ActionResult<ApiResponse<AttachmentDto>>> Get(long id, CancellationToken ct)
    {
        var item = await attachments.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<AttachmentDto>.Fail("Attachment not found"));
        return Ok(ApiResponse<AttachmentDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.HwManage)]
    public async Task<ActionResult<ApiResponse<AttachmentDto>>> Create(
        [FromBody] CreateAttachmentRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<AttachmentDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await attachments.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<AttachmentDto>.Ok(item, "Attachment created"));
    }
}

[ApiController]
[Authorize]
[Route("api/v1/entity-attachments")]
public sealed class EntityAttachmentsController(
    IAttachmentService attachments,
    IValidator<CreateEntityAttachmentRequest> linkValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.HwView)]
    public async Task<ActionResult<ApiResponse<PagedResult<EntityAttachmentDto>>>> Search(
        [FromQuery] EntityAttachmentQuery query, CancellationToken ct)
    {
        var result = await attachments.SearchLinksAsync(query, ct);
        return Ok(ApiResponse<PagedResult<EntityAttachmentDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.HwManage)]
    public async Task<ActionResult<ApiResponse<EntityAttachmentDto>>> Link(
        [FromBody] CreateEntityAttachmentRequest request, CancellationToken ct)
    {
        var validation = await linkValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<EntityAttachmentDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await attachments.LinkAsync(request, ct);
        return Ok(ApiResponse<EntityAttachmentDto>.Ok(item, "Attachment linked"));
    }
}
