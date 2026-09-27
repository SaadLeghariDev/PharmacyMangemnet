using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Hardware;

public sealed class DeviceTypeDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class DeviceTypeQuery : PaginationQuery
{
}

public sealed class DeviceDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public string? BranchName { get; set; }
    public long? CounterId { get; set; }
    public long DeviceTypeId { get; set; }
    public string? DeviceTypeCode { get; set; }
    public string? DeviceTypeName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? ConnectionType { get; set; }
    public string? Ip { get; set; }
    public int? Port { get; set; }
    public string? ComPort { get; set; }
    public string? MacAddress { get; set; }
    public string? DriverName { get; set; }
    public string? DriverVersion { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastSeenAt { get; set; }
}

public sealed class DeviceQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public long? DeviceTypeId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class CreateDeviceRequest
{
    public long BranchId { get; set; }
    public long? CounterId { get; set; }
    public long DeviceTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? ConnectionType { get; set; }
    public string? Ip { get; set; }
    public int? Port { get; set; }
    public string? ComPort { get; set; }
    public string? MacAddress { get; set; }
    public string? DriverName { get; set; }
    public string? DriverVersion { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateDeviceRequest
{
    public long? CounterId { get; set; }
    public long DeviceTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? ConnectionType { get; set; }
    public string? Ip { get; set; }
    public int? Port { get; set; }
    public string? ComPort { get; set; }
    public string? MacAddress { get; set; }
    public string? DriverName { get; set; }
    public string? DriverVersion { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastSeenAt { get; set; }
}

public sealed class DeviceAssignmentDto
{
    public long Id { get; set; }
    public long DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public long TerminalId { get; set; }
    public string? TerminalCode { get; set; }
    public DateTime AssignedFrom { get; set; }
    public DateTime? AssignedTo { get; set; }
    public bool IsActive { get; set; }
}

public sealed class DeviceAssignmentQuery : PaginationQuery
{
    public long? DeviceId { get; set; }
    public long? TerminalId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class CreateDeviceAssignmentRequest
{
    public long DeviceId { get; set; }
    public long TerminalId { get; set; }
    public DateTime? AssignedFrom { get; set; }
}

public sealed class DeviceSettingDto
{
    public long Id { get; set; }
    public long DeviceId { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string? SettingValue { get; set; }
    public bool IsEncrypted { get; set; }
}

public sealed class UpsertDeviceSettingRequest
{
    public string SettingKey { get; set; } = string.Empty;
    public string? SettingValue { get; set; }
    public bool IsEncrypted { get; set; }
}

public sealed class DeviceEventDto
{
    public long Id { get; set; }
    public long DeviceId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class DeviceEventQuery : PaginationQuery
{
}

public sealed class CreateDeviceEventRequest
{
    public string EventType { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public sealed class PrintTemplateDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string TemplateType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TemplateContent { get; set; } = string.Empty;
    public decimal? PaperWidth { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

public sealed class PrintTemplateQuery : PaginationQuery
{
    public string? TemplateType { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class CreatePrintTemplateRequest
{
    public string TemplateType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TemplateContent { get; set; } = string.Empty;
    public decimal? PaperWidth { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdatePrintTemplateRequest
{
    public string TemplateType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TemplateContent { get; set; } = string.Empty;
    public decimal? PaperWidth { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class BarcodePrintJobDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public string? BranchName { get; set; }
    public long PrinterDeviceId { get; set; }
    public string? PrinterDeviceName { get; set; }
    public long ProductId { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductName { get; set; }
    public long? BatchId { get; set; }
    public int Quantity { get; set; }
    public long TemplateId { get; set; }
    public string? TemplateName { get; set; }
    public string Status { get; set; } = string.Empty;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PrintedAt { get; set; }
}

public sealed class BarcodePrintJobQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public string? Status { get; set; }
    public long? ProductId { get; set; }
}

public sealed class CreateBarcodePrintJobRequest
{
    public long BranchId { get; set; }
    public long PrinterDeviceId { get; set; }
    public long ProductId { get; set; }
    public long? BatchId { get; set; }
    public int Quantity { get; set; }
    public long TemplateId { get; set; }
}

public sealed class UpdateBarcodePrintJobStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public sealed class SimulateBarcodePrintJobRequest
{
    public bool Fail { get; set; }
}

public sealed class AttachmentDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public string? Hash { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class AttachmentQuery : PaginationQuery
{
}

public sealed class CreateAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;
    /// <summary>File path, URL, or stub (e.g. stub://base64:...) — no blob column.</summary>
    public string StoragePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public string? Hash { get; set; }
}

public sealed class EntityAttachmentDto
{
    public long Id { get; set; }
    public long AttachmentId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public string? FileName { get; set; }
    public string? StoragePath { get; set; }
    public string? ContentType { get; set; }
}

public sealed class EntityAttachmentQuery : PaginationQuery
{
    public string? EntityName { get; set; }
    public long? EntityId { get; set; }
    public long? AttachmentId { get; set; }
}

public sealed class CreateEntityAttachmentRequest
{
    public long AttachmentId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public long EntityId { get; set; }
}
