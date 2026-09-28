using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Dashboard;
using PharmacyManagement.Application.Interfaces;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/dashboard")]
public sealed class DashboardController(IDashboardService dashboard) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> Summary(
        [FromQuery] DashboardSummaryQuery query,
        CancellationToken ct)
    {
        var result = await dashboard.GetSummaryAsync(query, ct);
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(result));
    }
}
