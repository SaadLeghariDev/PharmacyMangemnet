using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Finance;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/account-types")]
public sealed class AccountTypesController(IAccountTypeService accountTypes) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.FinCoa)]
    public async Task<ActionResult<ApiResponse<PagedResult<AccountTypeDto>>>> Search(
        [FromQuery] AccountTypeQuery query, CancellationToken ct)
    {
        var result = await accountTypes.SearchAsync(query, ct);
        return Ok(ApiResponse<PagedResult<AccountTypeDto>>.Ok(result));
    }
}
