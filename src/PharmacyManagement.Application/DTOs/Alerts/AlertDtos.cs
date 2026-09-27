using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Alerts;

public sealed class ReorderRuleQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public long? WarehouseId { get; set; }
    public long? ProductId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ReorderRuleDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public string? BranchCode { get; set; }
    public long WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }
    public long ProductId { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductName { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal MaximumStock { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal ReorderQuantity { get; set; }
    public long PreferredSupplierId { get; set; }
    public string? PreferredSupplierName { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateReorderRuleRequest
{
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public long ProductId { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal MaximumStock { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal ReorderQuantity { get; set; }
    public long PreferredSupplierId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateReorderRuleRequest
{
    public decimal MinimumStock { get; set; }
    public decimal MaximumStock { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal ReorderQuantity { get; set; }
    public long PreferredSupplierId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class LowStockCandidateQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public long? WarehouseId { get; set; }
    public long? ProductId { get; set; }
}

public sealed class LowStockCandidateDto
{
    public long ReorderRuleId { get; set; }
    public long BranchId { get; set; }
    public string? BranchCode { get; set; }
    public long WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }
    public long ProductId { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductName { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal ReorderQuantity { get; set; }
    public long PreferredSupplierId { get; set; }
    public string? PreferredSupplierName { get; set; }
    public decimal OnHandQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal ShortageQuantity { get; set; }
}

public sealed class AlertRuleQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public string? AlertType { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class AlertRuleDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public long? BranchId { get; set; }
    public string? BranchCode { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public decimal? Threshold { get; set; }
    public int? DaysBeforeExpiry { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateAlertRuleRequest
{
    public long? BranchId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public decimal? Threshold { get; set; }
    public int? DaysBeforeExpiry { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateAlertRuleRequest
{
    public long? BranchId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public decimal? Threshold { get; set; }
    public int? DaysBeforeExpiry { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AlertQuery : PaginationQuery
{
    public long? BranchId { get; set; }
    public long? ProductId { get; set; }
    public long? AlertRuleId { get; set; }
    public string? Status { get; set; }
    public string? Severity { get; set; }
    public string? AlertType { get; set; }
}

public sealed class AlertDto
{
    public long Id { get; set; }
    public long AlertRuleId { get; set; }
    public string? AlertType { get; set; }
    public long BranchId { get; set; }
    public string? BranchCode { get; set; }
    public long? ProductId { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductName { get; set; }
    public long? BatchId { get; set; }
    public string? BatchNumber { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public long? ResolvedBy { get; set; }
}

public sealed class EvaluateAlertsResultDto
{
    public int RulesScanned { get; set; }
    public int AlertsCreated { get; set; }
    public int NotificationLogsCreated { get; set; }
    public IReadOnlyList<AlertDto> CreatedAlerts { get; set; } = Array.Empty<AlertDto>();
}

public sealed class NotificationTemplateQuery : PaginationQuery
{
    public string? Channel { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class NotificationTemplateDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class CreateNotificationTemplateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateNotificationTemplateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class NotificationLogQuery : PaginationQuery
{
    public long? TemplateId { get; set; }
    public string? Status { get; set; }
    public string? ReferenceType { get; set; }
}

public sealed class NotificationLogDto
{
    public long Id { get; set; }
    public long TemplateId { get; set; }
    public string? TemplateCode { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
}
