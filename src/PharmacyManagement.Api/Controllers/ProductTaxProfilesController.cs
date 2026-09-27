using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Tax;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/products/{productId:long}/tax-profiles")]
public sealed class ProductTaxProfilesController(
    ITaxProfileService taxProfiles,
    IValidator<ReplaceProductTaxProfilesRequest> replaceValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.TaxView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductTaxProfileDto>>>> Get(
        long productId, CancellationToken ct)
    {
        var items = await taxProfiles.GetProductTaxProfilesAsync(productId, ct);
        return Ok(ApiResponse<IReadOnlyList<ProductTaxProfileDto>>.Ok(items));
    }

    [HttpPut]
    [Authorize(Policy = PermissionCodes.TaxEdit)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductTaxProfileDto>>>> Replace(
        long productId, [FromBody] ReplaceProductTaxProfilesRequest request, CancellationToken ct)
    {
        var validation = await replaceValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<IReadOnlyList<ProductTaxProfileDto>>.Fail(
                "Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var items = await taxProfiles.ReplaceProductTaxProfilesAsync(productId, request, ct);
        return Ok(ApiResponse<IReadOnlyList<ProductTaxProfileDto>>.Ok(items, "Product tax profiles updated"));
    }
}
