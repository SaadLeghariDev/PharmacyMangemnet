using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Admin;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/audit-logs")]
public sealed class AuditLogsController(IAuditLogService auditLogs) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.SecUsers)]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLogDto>>>> Search(
        [FromQuery] AuditLogQuery query, CancellationToken ct)
    {
        var result = await auditLogs.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.SecUsers)]
    public async Task<ActionResult<ApiResponse<AuditLogDto>>> Get(long id, CancellationToken ct)
    {
        var item = await auditLogs.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<AuditLogDto>.Fail("Audit log not found"));
        return Ok(ApiResponse<AuditLogDto>.Ok(item));
    }
}
