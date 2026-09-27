using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Hardware;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class DeviceService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IDeviceService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<DeviceTypeDto>> SearchDeviceTypesAsync(DeviceTypeQuery query, CancellationToken ct = default)
    {
        _ = RequireTenantId();
        var q = db.DeviceTypes.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(t => t.Code.Contains(s) || t.Name.Contains(s));
        }

        q = q.OrderBy(t => t.Code);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<DeviceTypeDto>
        {
            Items = rows.Select(t => new DeviceTypeDto { Id = t.Id, Code = t.Code, Name = t.Name }).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<PagedResult<DeviceDto>> SearchDevicesAsync(DeviceQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.Devices.AsNoTracking()
            .Include(d => d.Branch)
            .Include(d => d.DeviceType)
            .Where(d => d.Branch.TenantId == tenantId);

        if (query.BranchId is long branchId) q = q.Where(d => d.BranchId == branchId);
        if (query.DeviceTypeId is long typeId) q = q.Where(d => d.DeviceTypeId == typeId);
        if (query.IsActive is bool active) q = q.Where(d => d.IsActive == active);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(d => d.Name.Contains(s)
                || (d.SerialNumber != null && d.SerialNumber.Contains(s))
                || (d.Model != null && d.Model.Contains(s)));
        }

        q = q.OrderBy(d => d.Name);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<DeviceDto>
        {
            Items = rows.Select(MapDevice).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<DeviceDto?> GetDeviceByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Devices.AsNoTracking()
            .Include(d => d.Branch)
            .Include(d => d.DeviceType)
            .FirstOrDefaultAsync(d => d.Id == id && d.Branch.TenantId == tenantId, ct);
        return entity is null ? null : MapDevice(entity);
    }

    public async Task<DeviceDto> CreateDeviceAsync(CreateDeviceRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var branch = await db.Branches.FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Branch {request.BranchId} not found.");
        _ = await db.DeviceTypes.FirstOrDefaultAsync(t => t.Id == request.DeviceTypeId, ct)
            ?? throw new NotFoundException($"Device type {request.DeviceTypeId} not found.");

        if (request.CounterId is long counterId)
        {
            _ = await db.Counters.FirstOrDefaultAsync(c => c.Id == counterId && c.BranchId == branch.Id, ct)
                ?? throw new ValidationAppException([$"Counter {counterId} does not belong to branch {branch.Id}."]);
        }

        var entity = new Device
        {
            BranchId = branch.Id,
            CounterId = request.CounterId,
            DeviceTypeId = request.DeviceTypeId,
            Name = request.Name.Trim(),
            Manufacturer = TrimOrNull(request.Manufacturer),
            Model = TrimOrNull(request.Model),
            SerialNumber = TrimOrNull(request.SerialNumber),
            ConnectionType = TrimOrNull(request.ConnectionType),
            Ip = TrimOrNull(request.Ip),
            Port = request.Port,
            Comport = TrimOrNull(request.ComPort),
            MacAddress = TrimOrNull(request.MacAddress),
            DriverName = TrimOrNull(request.DriverName),
            DriverVersion = TrimOrNull(request.DriverVersion),
            IsDefault = request.IsDefault,
            IsActive = request.IsActive
        };
        db.Devices.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetDeviceByIdAsync(entity.Id, ct))!;
    }

    public async Task<DeviceDto> UpdateDeviceAsync(long id, UpdateDeviceRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Devices.Include(d => d.Branch)
            .FirstOrDefaultAsync(d => d.Id == id && d.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Device {id} not found.");

        _ = await db.DeviceTypes.FirstOrDefaultAsync(t => t.Id == request.DeviceTypeId, ct)
            ?? throw new NotFoundException($"Device type {request.DeviceTypeId} not found.");

        if (request.CounterId is long counterId)
        {
            _ = await db.Counters.FirstOrDefaultAsync(c => c.Id == counterId && c.BranchId == entity.BranchId, ct)
                ?? throw new ValidationAppException([$"Counter {counterId} does not belong to branch {entity.BranchId}."]);
        }

        entity.CounterId = request.CounterId;
        entity.DeviceTypeId = request.DeviceTypeId;
        entity.Name = request.Name.Trim();
        entity.Manufacturer = TrimOrNull(request.Manufacturer);
        entity.Model = TrimOrNull(request.Model);
        entity.SerialNumber = TrimOrNull(request.SerialNumber);
        entity.ConnectionType = TrimOrNull(request.ConnectionType);
        entity.Ip = TrimOrNull(request.Ip);
        entity.Port = request.Port;
        entity.Comport = TrimOrNull(request.ComPort);
        entity.MacAddress = TrimOrNull(request.MacAddress);
        entity.DriverName = TrimOrNull(request.DriverName);
        entity.DriverVersion = TrimOrNull(request.DriverVersion);
        entity.IsDefault = request.IsDefault;
        entity.IsActive = request.IsActive;
        if (request.LastSeenAt.HasValue) entity.LastSeenAt = request.LastSeenAt;

        await db.SaveChangesAsync(ct);
        return (await GetDeviceByIdAsync(entity.Id, ct))!;
    }

    public async Task<PagedResult<DeviceAssignmentDto>> SearchAssignmentsAsync(
        DeviceAssignmentQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.DeviceAssignments.AsNoTracking()
            .Include(a => a.Device).ThenInclude(d => d.Branch)
            .Include(a => a.Terminal)
            .Where(a => a.Device.Branch.TenantId == tenantId);

        if (query.DeviceId is long deviceId) q = q.Where(a => a.DeviceId == deviceId);
        if (query.TerminalId is long terminalId) q = q.Where(a => a.TerminalId == terminalId);
        if (query.IsActive is bool active) q = q.Where(a => a.IsActive == active);

        q = q.OrderByDescending(a => a.AssignedFrom);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<DeviceAssignmentDto>
        {
            Items = rows.Select(MapAssignment).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<DeviceAssignmentDto> CreateAssignmentAsync(
        CreateDeviceAssignmentRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var device = await db.Devices.Include(d => d.Branch)
            .FirstOrDefaultAsync(d => d.Id == request.DeviceId && d.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Device {request.DeviceId} not found.");
        var terminal = await db.Posterminals
            .FirstOrDefaultAsync(t => t.Id == request.TerminalId && t.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Terminal {request.TerminalId} not found.");

        if (terminal.BranchId != device.BranchId)
            throw new ValidationAppException(["Device and terminal must belong to the same branch."]);

        var entity = new DeviceAssignment
        {
            DeviceId = device.Id,
            TerminalId = terminal.Id,
            AssignedFrom = request.AssignedFrom ?? DateTime.UtcNow,
            IsActive = true
        };
        db.DeviceAssignments.Add(entity);
        await db.SaveChangesAsync(ct);

        return new DeviceAssignmentDto
        {
            Id = entity.Id,
            DeviceId = entity.DeviceId,
            DeviceName = device.Name,
            TerminalId = entity.TerminalId,
            TerminalCode = terminal.TerminalCode,
            AssignedFrom = entity.AssignedFrom,
            AssignedTo = entity.AssignedTo,
            IsActive = entity.IsActive
        };
    }

    public async Task<DeviceAssignmentDto> EndAssignmentAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.DeviceAssignments
            .Include(a => a.Device).ThenInclude(d => d.Branch)
            .Include(a => a.Terminal)
            .FirstOrDefaultAsync(a => a.Id == id && a.Device.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Device assignment {id} not found.");

        entity.IsActive = false;
        entity.AssignedTo ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return MapAssignment(entity);
    }

    public async Task<IReadOnlyList<DeviceSettingDto>> GetSettingsAsync(
        long deviceId, string? key, CancellationToken ct = default)
    {
        await RequireDeviceAsync(deviceId, ct);
        var q = db.DeviceSettings.AsNoTracking().Where(s => s.DeviceId == deviceId);
        if (!string.IsNullOrWhiteSpace(key))
        {
            var k = key.Trim();
            q = q.Where(s => s.SettingKey == k);
        }

        var rows = await q.OrderBy(s => s.SettingKey).ToListAsync(ct);
        return rows.Select(MapSetting).ToList();
    }

    public async Task<DeviceSettingDto> UpsertSettingAsync(
        long deviceId, UpsertDeviceSettingRequest request, CancellationToken ct = default)
    {
        await RequireDeviceAsync(deviceId, ct);
        var key = request.SettingKey.Trim();
        var entity = await db.DeviceSettings.FirstOrDefaultAsync(s => s.DeviceId == deviceId && s.SettingKey == key, ct);
        if (entity is null)
        {
            entity = new DeviceSetting
            {
                DeviceId = deviceId,
                SettingKey = key,
                SettingValue = request.SettingValue,
                IsEncrypted = request.IsEncrypted
            };
            db.DeviceSettings.Add(entity);
        }
        else
        {
            entity.SettingValue = request.SettingValue;
            entity.IsEncrypted = request.IsEncrypted;
        }

        await db.SaveChangesAsync(ct);
        return MapSetting(entity);
    }

    public async Task<PagedResult<DeviceEventDto>> SearchEventsAsync(
        long deviceId, DeviceEventQuery query, CancellationToken ct = default)
    {
        await RequireDeviceAsync(deviceId, ct);
        var q = db.DeviceEvents.AsNoTracking().Where(e => e.DeviceId == deviceId).OrderByDescending(e => e.CreatedAt);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<DeviceEventDto>
        {
            Items = rows.Select(MapEvent).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<DeviceEventDto> AppendEventAsync(
        long deviceId, CreateDeviceEventRequest request, CancellationToken ct = default)
    {
        await RequireDeviceAsync(deviceId, ct);
        var entity = new DeviceEvent
        {
            DeviceId = deviceId,
            EventType = request.EventType.Trim(),
            Payload = string.IsNullOrWhiteSpace(request.Payload) ? null : request.Payload,
            Status = request.Status.Trim(),
            ErrorMessage = TrimOrNull(request.ErrorMessage),
            CreatedAt = DateTime.UtcNow
        };
        db.DeviceEvents.Add(entity);
        await db.SaveChangesAsync(ct);
        return MapEvent(entity);
    }

    private async Task RequireDeviceAsync(long deviceId, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        _ = await db.Devices.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Device {deviceId} not found.");
    }

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DeviceDto MapDevice(Device d) => new()
    {
        Id = d.Id,
        BranchId = d.BranchId,
        BranchName = d.Branch?.Name,
        CounterId = d.CounterId,
        DeviceTypeId = d.DeviceTypeId,
        DeviceTypeCode = d.DeviceType?.Code,
        DeviceTypeName = d.DeviceType?.Name,
        Name = d.Name,
        Manufacturer = d.Manufacturer,
        Model = d.Model,
        SerialNumber = d.SerialNumber,
        ConnectionType = d.ConnectionType,
        Ip = d.Ip,
        Port = d.Port,
        ComPort = d.Comport,
        MacAddress = d.MacAddress,
        DriverName = d.DriverName,
        DriverVersion = d.DriverVersion,
        IsDefault = d.IsDefault,
        IsActive = d.IsActive,
        LastSeenAt = d.LastSeenAt
    };

    private static DeviceAssignmentDto MapAssignment(DeviceAssignment a) => new()
    {
        Id = a.Id,
        DeviceId = a.DeviceId,
        DeviceName = a.Device?.Name,
        TerminalId = a.TerminalId,
        TerminalCode = a.Terminal?.TerminalCode,
        AssignedFrom = a.AssignedFrom,
        AssignedTo = a.AssignedTo,
        IsActive = a.IsActive
    };

    private static DeviceSettingDto MapSetting(DeviceSetting s) => new()
    {
        Id = s.Id,
        DeviceId = s.DeviceId,
        SettingKey = s.SettingKey,
        SettingValue = s.SettingValue,
        IsEncrypted = s.IsEncrypted
    };

    private static DeviceEventDto MapEvent(DeviceEvent e) => new()
    {
        Id = e.Id,
        DeviceId = e.DeviceId,
        EventType = e.EventType,
        Payload = e.Payload,
        Status = e.Status,
        ErrorMessage = e.ErrorMessage,
        CreatedAt = e.CreatedAt
    };
}
