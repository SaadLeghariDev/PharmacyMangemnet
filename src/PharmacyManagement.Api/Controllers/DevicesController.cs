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
[Route("api/v1/device-types")]
public sealed class DeviceTypesController(IDeviceService devices) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.HwView)]
    public async Task<ActionResult<ApiResponse<PagedResult<DeviceTypeDto>>>> Search(
        [FromQuery] DeviceTypeQuery query, CancellationToken ct)
    {
        var result = await devices.SearchDeviceTypesAsync(query, ct);
        return Ok(ApiResponse<PagedResult<DeviceTypeDto>>.Ok(result));
    }
}

[ApiController]
[Authorize]
[Route("api/v1/devices")]
public sealed class DevicesController(
    IDeviceService devices,
    IValidator<CreateDeviceRequest> createValidator,
    IValidator<UpdateDeviceRequest> updateValidator,
    IValidator<UpsertDeviceSettingRequest> settingValidator,
    IValidator<CreateDeviceEventRequest> eventValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.HwView)]
    public async Task<ActionResult<ApiResponse<PagedResult<DeviceDto>>>> Search(
        [FromQuery] DeviceQuery query, CancellationToken ct)
    {
        var result = await devices.SearchDevicesAsync(query, ct);
        return Ok(ApiResponse<PagedResult<DeviceDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionCodes.HwView)]
    public async Task<ActionResult<ApiResponse<DeviceDto>>> Get(long id, CancellationToken ct)
    {
        var item = await devices.GetDeviceByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<DeviceDto>.Fail("Device not found"));
        return Ok(ApiResponse<DeviceDto>.Ok(item));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.HwManage)]
    public async Task<ActionResult<ApiResponse<DeviceDto>>> Create(
        [FromBody] CreateDeviceRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<DeviceDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await devices.CreateDeviceAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ApiResponse<DeviceDto>.Ok(item, "Device created"));
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCodes.HwManage)]
    public async Task<ActionResult<ApiResponse<DeviceDto>>> Update(
        long id, [FromBody] UpdateDeviceRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<DeviceDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await devices.UpdateDeviceAsync(id, request, ct);
        return Ok(ApiResponse<DeviceDto>.Ok(item, "Device updated"));
    }

    [HttpGet("{deviceId:long}/settings")]
    [Authorize(Policy = PermissionCodes.HwView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DeviceSettingDto>>>> GetSettings(
        long deviceId, [FromQuery] string? key, CancellationToken ct)
    {
        var items = await devices.GetSettingsAsync(deviceId, key, ct);
        return Ok(ApiResponse<IReadOnlyList<DeviceSettingDto>>.Ok(items));
    }

    [HttpPut("{deviceId:long}/settings")]
    [Authorize(Policy = PermissionCodes.HwManage)]
    public async Task<ActionResult<ApiResponse<DeviceSettingDto>>> UpsertSetting(
        long deviceId, [FromBody] UpsertDeviceSettingRequest request, CancellationToken ct)
    {
        var validation = await settingValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<DeviceSettingDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await devices.UpsertSettingAsync(deviceId, request, ct);
        return Ok(ApiResponse<DeviceSettingDto>.Ok(item, "Device setting saved"));
    }

    [HttpGet("{deviceId:long}/events")]
    [Authorize(Policy = PermissionCodes.HwView)]
    public async Task<ActionResult<ApiResponse<PagedResult<DeviceEventDto>>>> ListEvents(
        long deviceId, [FromQuery] DeviceEventQuery query, CancellationToken ct)
    {
        var result = await devices.SearchEventsAsync(deviceId, query, ct);
        return Ok(ApiResponse<PagedResult<DeviceEventDto>>.Ok(result));
    }

    [HttpPost("{deviceId:long}/events")]
    [Authorize(Policy = PermissionCodes.HwManage)]
    public async Task<ActionResult<ApiResponse<DeviceEventDto>>> AppendEvent(
        long deviceId, [FromBody] CreateDeviceEventRequest request, CancellationToken ct)
    {
        var validation = await eventValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<DeviceEventDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await devices.AppendEventAsync(deviceId, request, ct);
        return Ok(ApiResponse<DeviceEventDto>.Ok(item, "Device event recorded"));
    }
}

[ApiController]
[Authorize]
[Route("api/v1/device-assignments")]
public sealed class DeviceAssignmentsController(
    IDeviceService devices,
    IValidator<CreateDeviceAssignmentRequest> createValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.HwView)]
    public async Task<ActionResult<ApiResponse<PagedResult<DeviceAssignmentDto>>>> Search(
        [FromQuery] DeviceAssignmentQuery query, CancellationToken ct)
    {
        var result = await devices.SearchAssignmentsAsync(query, ct);
        return Ok(ApiResponse<PagedResult<DeviceAssignmentDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.HwManage)]
    public async Task<ActionResult<ApiResponse<DeviceAssignmentDto>>> Create(
        [FromBody] CreateDeviceAssignmentRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<DeviceAssignmentDto>.Fail("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));
        var item = await devices.CreateAssignmentAsync(request, ct);
        return Ok(ApiResponse<DeviceAssignmentDto>.Ok(item, "Device assigned"));
    }

    [HttpPost("{id:long}/end")]
    [Authorize(Policy = PermissionCodes.HwManage)]
    public async Task<ActionResult<ApiResponse<DeviceAssignmentDto>>> End(long id, CancellationToken ct)
    {
        var item = await devices.EndAssignmentAsync(id, ct);
        return Ok(ApiResponse<DeviceAssignmentDto>.Ok(item, "Assignment ended"));
    }
}
