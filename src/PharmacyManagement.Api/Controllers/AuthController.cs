using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Auth;
using PharmacyManagement.Application.Interfaces;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    IAuthService authService,
    IValidator<LoginRequest> loginValidator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var validation = await loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<LoginResponse>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));

        var result = await authService.LoginAsync(request, ct);
        return Ok(ApiResponse<LoginResponse>.Ok(result, "Login successful"));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> Me(CancellationToken ct)
    {
        var profile = await authService.GetCurrentUserAsync(ct);
        return Ok(ApiResponse<UserProfileDto>.Ok(profile));
    }
}
