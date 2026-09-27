using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Sync;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/sync-nodes")]
public sealed class SyncNodesController(
    ISyncService sync,
    IValidator<RegisterSyncNodeRequest> registerValidator,
    IValidator<UpdateSyncNodeRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.SyncView)]
    public async Task<ActionResult<ApiResponse<PagedResult<SyncNodeDto>>>> Search(
        [FromQuery] SyncNodeQuery query, CancellationToken ct)
    {
        var result = await sync.SearchNodesAsync(query, ct);
        return Ok(ApiResponse<PagedResult<SyncNodeDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.SyncView)]
    public async Task<ActionResult<ApiResponse<SyncNodeDto>>> Get(long id, CancellationToken ct)
    {
        var item = await sync.GetNodeByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<SyncNodeDto>.Fail("Sync node not found"));
        return Ok(ApiResponse<SyncNodeDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.SyncManage)]
    public async Task<ActionResult<ApiResponse<SyncNodeDto>>> Register(
        [FromBody] RegisterSyncNodeRequest request, CancellationToken ct)
    {
        var validation = await registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SyncNodeDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await sync.RegisterNodeAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<SyncNodeDto>.Ok(item, "Sync node registered"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.SyncManage)]
    public async Task<ActionResult<ApiResponse<SyncNodeDto>>> Update(
        long id, [FromBody] UpdateSyncNodeRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SyncNodeDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await sync.UpdateNodeAsync(id, request, ct);
        return Ok(ApiResponse<SyncNodeDto>.Ok(item, "Sync node updated"));
    }
}

[ApiController]
[Authorize]
[Route("api/v1/sync-batches")]
public sealed class SyncBatchesController(
    ISyncService sync,
    IValidator<CreateSyncBatchRequest> createValidator,
    IValidator<UpdateSyncBatchStatusRequest> statusValidator,
    IValidator<CreateSyncItemsRequest> itemsValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.SyncView)]
    public async Task<ActionResult<ApiResponse<PagedResult<SyncBatchDto>>>> Search(
        [FromQuery] SyncBatchQuery query, CancellationToken ct)
    {
        var result = await sync.SearchBatchesAsync(query, ct);
        return Ok(ApiResponse<PagedResult<SyncBatchDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.SyncView)]
    public async Task<ActionResult<ApiResponse<SyncBatchDto>>> Get(long id, CancellationToken ct)
    {
        var item = await sync.GetBatchByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<SyncBatchDto>.Fail("Sync batch not found"));
        return Ok(ApiResponse<SyncBatchDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.SyncManage)]
    public async Task<ActionResult<ApiResponse<SyncBatchDto>>> Create(
        [FromBody] CreateSyncBatchRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SyncBatchDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await sync.CreateBatchAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<SyncBatchDto>.Ok(item, "Sync batch created"));
    }

    [HttpPut("{id:long}/status")]
    [Authorize(Policy = PermissionCodes.SyncManage)]
    public async Task<ActionResult<ApiResponse<SyncBatchDto>>> UpdateStatus(
        long id, [FromBody] UpdateSyncBatchStatusRequest request, CancellationToken ct)
    {
        var validation = await statusValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SyncBatchDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await sync.UpdateBatchStatusAsync(id, request, ct);
        return Ok(ApiResponse<SyncBatchDto>.Ok(item, "Sync batch status updated"));
    }

    [HttpGet("{batchId:long}/items")]
    [Authorize(Policy = PermissionCodes.SyncView)]
    public async Task<ActionResult<ApiResponse<PagedResult<SyncItemDto>>>> ListItems(
        long batchId, [FromQuery] SyncItemQuery query, CancellationToken ct)
    {
        var result = await sync.SearchItemsAsync(batchId, query, ct);
        return Ok(ApiResponse<PagedResult<SyncItemDto>>.Ok(result));
    }

    [HttpPost("{batchId:long}/items")]
    [Authorize(Policy = PermissionCodes.SyncManage)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SyncItemDto>>>> AddItems(
        long batchId, [FromBody] CreateSyncItemsRequest request, CancellationToken ct)
    {
        var validation = await itemsValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<IReadOnlyList<SyncItemDto>>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var items = await sync.AddItemsAsync(batchId, request, ct);
        return Ok(ApiResponse<IReadOnlyList<SyncItemDto>>.Ok(items, "Sync items added"));
    }
}

[ApiController]
[Authorize]
[Route("api/v1/sync-items")]
public sealed class SyncItemsController(
    ISyncService sync,
    IValidator<UpdateSyncItemStatusRequest> statusValidator) : ControllerBase
{
    [HttpPut("{id:long}/status")]
    [Authorize(Policy = PermissionCodes.SyncManage)]
    public async Task<ActionResult<ApiResponse<SyncItemDto>>> UpdateStatus(
        long id, [FromBody] UpdateSyncItemStatusRequest request, CancellationToken ct)
    {
        var validation = await statusValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SyncItemDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await sync.UpdateItemStatusAsync(id, request, ct);
        return Ok(ApiResponse<SyncItemDto>.Ok(item, "Sync item status updated"));
    }
}

[ApiController]
[Authorize]
[Route("api/v1/sync")]
public sealed class SyncController(
    ISyncService sync,
    IValidator<SyncPushRequest> pushValidator,
    IValidator<SyncPullRequest> pullValidator) : ControllerBase
{
    [HttpPost("push")]
    [Authorize(Policy = PermissionCodes.SyncManage)]
    public async Task<ActionResult<ApiResponse<SyncPushPullResultDto>>> Push(
        [FromBody] SyncPushRequest request, CancellationToken ct)
    {
        var validation = await pushValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SyncPushPullResultDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var result = await sync.PushAsync(request, ct);
        return Ok(ApiResponse<SyncPushPullResultDto>.Ok(result, "Sync push completed"));
    }

    [HttpPost("pull")]
    [Authorize(Policy = PermissionCodes.SyncManage)]
    public async Task<ActionResult<ApiResponse<SyncPushPullResultDto>>> Pull(
        [FromBody] SyncPullRequest request, CancellationToken ct)
    {
        var validation = await pullValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<SyncPushPullResultDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var result = await sync.PullAsync(request, ct);
        return Ok(ApiResponse<SyncPushPullResultDto>.Ok(result, "Sync pull completed"));
    }
}

[ApiController]
[Authorize]
[Route("api/v1/idempotency-keys")]
public sealed class IdempotencyKeysController(ISyncService sync) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.SyncView)]
    public async Task<ActionResult<ApiResponse<PagedResult<IdempotencyKeyDto>>>> Search(
        [FromQuery] IdempotencyKeyQuery query, CancellationToken ct)
    {
        var result = await sync.SearchIdempotencyKeysAsync(query, ct);
        return Ok(ApiResponse<PagedResult<IdempotencyKeyDto>>.Ok(result));
    }
}
