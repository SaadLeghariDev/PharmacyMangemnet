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
[Route("api/v1/notification-templates")]
public sealed class NotificationTemplatesController(
    INotificationTemplateService templates,
    IValidator<CreateNotificationTemplateRequest> createValidator,
    IValidator<UpdateNotificationTemplateRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.AlertView)]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationTemplateDto>>>> Search(
        [FromQuery] NotificationTemplateQuery query, CancellationToken ct)
    {
        var result = await templates.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<NotificationTemplateDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.AlertView)]
    public async Task<ActionResult<ApiResponse<NotificationTemplateDto>>> Get(long id, CancellationToken ct)
    {
        var item = await templates.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<NotificationTemplateDto>.Fail("Notification template not found"));
        return Ok(ApiResponse<NotificationTemplateDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.AlertManage)]
    public async Task<ActionResult<ApiResponse<NotificationTemplateDto>>> Create(
        [FromBody] CreateNotificationTemplateRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<NotificationTemplateDto>.Fail(
                "Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await templates.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id },
            ApiResponse<NotificationTemplateDto>.Ok(item, "Notification template created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.AlertManage)]
    public async Task<ActionResult<ApiResponse<NotificationTemplateDto>>> Update(
        long id, [FromBody] UpdateNotificationTemplateRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<NotificationTemplateDto>.Fail(
                "Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await templates.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<NotificationTemplateDto>.Ok(item, "Notification template updated"));
    }

    [HttpGet("~/api/v1/notification-logs")]
    [Authorize(Policy = PermissionCodes.AlertView)]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationLogDto>>>> SearchLogs(
        [FromQuery] NotificationLogQuery query, CancellationToken ct)
    {
        var result = await templates.SearchLogsAsync(query, ct);
        return Ok(ApiResponse<PagedResult<NotificationLogDto>>.Ok(result));
    }
}
