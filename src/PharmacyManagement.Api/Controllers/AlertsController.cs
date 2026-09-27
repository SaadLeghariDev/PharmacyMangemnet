using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Alerts;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/alerts")]
public sealed class AlertsController(IAlertService alerts) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.AlertView)]
    public async Task<ActionResult<ApiResponse<PagedResult<AlertDto>>>> Search(
        [FromQuery] AlertQuery query, CancellationToken ct)
    {
        var result = await alerts.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<AlertDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.AlertView)]
    public async Task<ActionResult<ApiResponse<AlertDto>>> Get(long id, CancellationToken ct)
    {
        var item = await alerts.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<AlertDto>.Fail("Alert not found"));
        return Ok(ApiResponse<AlertDto>.Ok(item));
    }

    [HttpPost("evaluate")]
    [Authorize(Policy = PermissionCodes.AlertManage)]
    public async Task<ActionResult<ApiResponse<EvaluateAlertsResultDto>>> Evaluate(CancellationToken ct)
    {
        var result = await alerts.EvaluateAsync(ct);
        return Ok(ApiResponse<EvaluateAlertsResultDto>.Ok(result, "Alert evaluation completed"));
    }

    [HttpPost("{id:long}/acknowledge")]
    [Authorize(Policy = PermissionCodes.AlertManage)]
    public async Task<ActionResult<ApiResponse<AlertDto>>> Acknowledge(long id, CancellationToken ct)
    {
        var item = await alerts.AcknowledgeAsync(id, ct);
        return Ok(ApiResponse<AlertDto>.Ok(item, "Alert acknowledged"));
    }

    [HttpPost("{id:long}/resolve")]
    [Authorize(Policy = PermissionCodes.AlertManage)]
    public async Task<ActionResult<ApiResponse<AlertDto>>> Resolve(long id, CancellationToken ct)
    {
        var item = await alerts.ResolveAsync(id, ct);
        return Ok(ApiResponse<AlertDto>.Ok(item, "Alert resolved"));
    }
}
