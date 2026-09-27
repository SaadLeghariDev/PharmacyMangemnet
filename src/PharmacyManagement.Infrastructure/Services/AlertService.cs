using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Alerts;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class AlertService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IAlertService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<AlertDto>> SearchAsync(AlertQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = BaseQuery(tenantId);

        if (query.BranchId is long branchId) q = q.Where(a => a.BranchId == branchId);
        if (query.ProductId is long productId) q = q.Where(a => a.ProductId == productId);
        if (query.AlertRuleId is long ruleId) q = q.Where(a => a.AlertRuleId == ruleId);
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim();
            q = q.Where(a => a.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(query.Severity))
        {
            var severity = query.Severity.Trim();
            q = q.Where(a => a.Severity == severity);
        }
        if (!string.IsNullOrWhiteSpace(query.AlertType))
        {
            var type = query.AlertType.Trim();
            q = q.Where(a => a.AlertRule.AlertType == type);
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(a =>
                a.Title.Contains(s) ||
                (a.Message != null && a.Message.Contains(s)) ||
                (a.Product != null && (a.Product.Sku.Contains(s) || a.Product.Name.Contains(s))));
        }

        q = q.OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.Id);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return new PagedResult<AlertDto>
        {
            Items = rows.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<AlertDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await BaseQuery(tenantId).FirstOrDefaultAsync(a => a.Id == id, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<AlertDto> AcknowledgeAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Alerts
            .Include(a => a.AlertRule)
            .Include(a => a.Branch)
            .Include(a => a.Product)
            .Include(a => a.Batch)
            .FirstOrDefaultAsync(a => a.Id == id && a.AlertRule.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Alert {id} not found.");

        if (entity.Status is AlertStatuses.Resolved or AlertStatuses.Dismissed)
            throw new ConflictException($"Cannot acknowledge a {entity.Status.ToLowerInvariant()} alert.");

        entity.Status = AlertStatuses.Acknowledged;
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<AlertDto> ResolveAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.Alerts
            .Include(a => a.AlertRule)
            .Include(a => a.Branch)
            .Include(a => a.Product)
            .Include(a => a.Batch)
            .FirstOrDefaultAsync(a => a.Id == id && a.AlertRule.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Alert {id} not found.");

        if (entity.Status == AlertStatuses.Resolved)
            throw new ConflictException("Alert is already resolved.");

        entity.Status = AlertStatuses.Resolved;
        entity.ResolvedAt = DateTime.UtcNow;
        entity.ResolvedBy = currentUser.UserId;
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<EvaluateAlertsResultDto> EvaluateAsync(CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var rules = await db.AlertRules
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .ToListAsync(ct);

        var created = new List<Alert>();
        var notificationLogs = 0;

        foreach (var rule in rules)
        {
            if (AlertTypes.IsLowStock(rule.AlertType))
                created.AddRange(await EvaluateLowStockAsync(tenantId, rule, ct));
            else if (AlertTypes.IsExpiry(rule.AlertType))
                created.AddRange(await EvaluateExpiryAsync(tenantId, rule, ct));
        }

        if (created.Count > 0)
        {
            db.Alerts.AddRange(created);
            await db.SaveChangesAsync(ct);

            foreach (var alert in created)
                notificationLogs += await TryLogNotificationAsync(tenantId, alert, ct);

            if (notificationLogs > 0)
                await db.SaveChangesAsync(ct);
        }

        var createdIds = created.Select(a => a.Id).ToList();
        var mapped = createdIds.Count == 0
            ? []
            : (await BaseQuery(tenantId).Where(a => createdIds.Contains(a.Id)).ToListAsync(ct))
                .Select(Map)
                .ToList();

        return new EvaluateAlertsResultDto
        {
            RulesScanned = rules.Count,
            AlertsCreated = created.Count,
            NotificationLogsCreated = notificationLogs,
            CreatedAlerts = mapped
        };
    }

    private async Task<List<Alert>> EvaluateLowStockAsync(long tenantId, AlertRule rule, CancellationToken ct)
    {
        var reorderQuery = db.ReorderRules.AsNoTracking()
            .Include(r => r.Product)
            .Include(r => r.Warehouse)
            .Where(r => r.Branch.TenantId == tenantId && r.IsActive);

        if (rule.BranchId is long branchId)
            reorderQuery = reorderQuery.Where(r => r.BranchId == branchId);

        var reorderRules = await reorderQuery.ToListAsync(ct);
        if (reorderRules.Count == 0) return [];

        var productIds = reorderRules.Select(r => r.ProductId).Distinct().ToList();
        var warehouseIds = reorderRules.Select(r => r.WarehouseId).Distinct().ToList();

        var stockRows = await db.InventoryBatchLocations.AsNoTracking()
            .Where(x =>
                productIds.Contains(x.Batch.ProductId) &&
                warehouseIds.Contains(x.Batch.WarehouseId) &&
                x.Batch.BatchStatus == BatchStatuses.Available &&
                !x.Batch.IsRecalled)
            .GroupBy(x => new { x.Batch.ProductId, x.Batch.WarehouseId })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.WarehouseId,
                Available = g.Sum(x => x.QuantityOnHand - x.ReservedQuantity)
            })
            .ToListAsync(ct);
        var stockMap = stockRows.ToDictionary(x => (x.ProductId, x.WarehouseId));

        var openKeys = await db.Alerts.AsNoTracking()
            .Where(a =>
                a.AlertRuleId == rule.Id &&
                (a.Status == AlertStatuses.Open || a.Status == AlertStatuses.Acknowledged) &&
                a.ProductId != null &&
                a.BatchId == null)
            .Select(a => new { a.BranchId, ProductId = a.ProductId!.Value })
            .ToListAsync(ct);
        var openSet = openKeys.Select(k => (k.BranchId, k.ProductId)).ToHashSet();

        var created = new List<Alert>();
        foreach (var rr in reorderRules)
        {
            stockMap.TryGetValue((rr.ProductId, rr.WarehouseId), out var stock);
            var available = stock?.Available ?? 0m;
            var compareTo = rule.Threshold.HasValue
                ? Math.Min(rr.ReorderPoint, rule.Threshold.Value)
                : rr.ReorderPoint;

            if (available > compareTo) continue;
            if (openSet.Contains((rr.BranchId, rr.ProductId))) continue;

            var severity = available <= rr.MinimumStock
                ? AlertSeverities.Critical
                : AlertSeverities.Warning;

            created.Add(new Alert
            {
                AlertRuleId = rule.Id,
                BranchId = rr.BranchId,
                ProductId = rr.ProductId,
                BatchId = null,
                Severity = severity,
                Title = $"Low stock: {rr.Product.Sku}",
                Message =
                    $"{rr.Product.Name} at {rr.Warehouse.Code} has {available:0.####} available " +
                    $"(reorder point {rr.ReorderPoint:0.####}" +
                    (rule.Threshold.HasValue ? $", threshold {rule.Threshold:0.####}" : "") +
                    "). Suggested reorder qty " + $"{rr.ReorderQuantity:0.####}.",
                Status = AlertStatuses.Open,
                CreatedAt = DateTime.UtcNow
            });
            openSet.Add((rr.BranchId, rr.ProductId));
        }

        return created;
    }

    private async Task<List<Alert>> EvaluateExpiryAsync(long tenantId, AlertRule rule, CancellationToken ct)
    {
        var days = Math.Max(rule.DaysBeforeExpiry ?? 0, 0);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = today.AddDays(days);

        var q = db.InventoryBatchLocations.AsNoTracking()
            .Include(x => x.Batch).ThenInclude(b => b.Product)
            .Include(x => x.Batch).ThenInclude(b => b.Warehouse).ThenInclude(w => w.Branch)
            .Where(x =>
                x.Batch.Product.TenantId == tenantId &&
                x.Batch.BatchStatus == BatchStatuses.Available &&
                !x.Batch.IsRecalled &&
                x.QuantityOnHand - x.ReservedQuantity > 0 &&
                x.Batch.ExpiryDate <= cutoff);

        if (rule.BranchId is long branchId)
            q = q.Where(x => x.Batch.Warehouse.BranchId == branchId);

        var rows = await q.ToListAsync(ct);
        if (rows.Count == 0) return [];

        var openBatchIds = await db.Alerts.AsNoTracking()
            .Where(a =>
                a.AlertRuleId == rule.Id &&
                (a.Status == AlertStatuses.Open || a.Status == AlertStatuses.Acknowledged) &&
                a.BatchId != null)
            .Select(a => a.BatchId!.Value)
            .ToListAsync(ct);
        var openSet = openBatchIds.ToHashSet();

        var created = new List<Alert>();
        foreach (var group in rows.GroupBy(x => x.BatchId))
        {
            var sample = group.First();
            var batch = sample.Batch;
            if (openSet.Contains(batch.Id)) continue;

            var available = group.Sum(x => x.QuantityOnHand - x.ReservedQuantity);
            var daysLeft = batch.ExpiryDate.DayNumber - today.DayNumber;
            var severity = daysLeft <= 7 || batch.ExpiryDate < today
                ? AlertSeverities.Critical
                : AlertSeverities.Warning;

            created.Add(new Alert
            {
                AlertRuleId = rule.Id,
                BranchId = batch.Warehouse.BranchId,
                ProductId = batch.ProductId,
                BatchId = batch.Id,
                Severity = severity,
                Title = $"Expiry: {batch.Product.Sku} / {batch.BatchNumber}",
                Message =
                    $"{batch.Product.Name} batch {batch.BatchNumber} expires {batch.ExpiryDate:yyyy-MM-dd} " +
                    $"({daysLeft} day(s)); available {available:0.####}.",
                Status = AlertStatuses.Open,
                CreatedAt = DateTime.UtcNow
            });
            openSet.Add(batch.Id);
        }

        return created;
    }

    private async Task<int> TryLogNotificationAsync(long tenantId, Alert alert, CancellationToken ct)
    {
        var rule = await db.AlertRules.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == alert.AlertRuleId, ct);
        if (rule is null) return 0;

        var code = $"ALERT_{rule.AlertType}";
        var template = await db.NotificationTemplates
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Code == code && t.IsActive, ct);
        if (template is null) return 0;

        db.NotificationLogs.Add(new NotificationLog
        {
            TemplateId = template.Id,
            Recipient = "system",
            Channel = template.Channel,
            ReferenceType = "Alert",
            ReferenceId = alert.Id,
            Status = NotificationLogStatuses.Pending,
            SentAt = null,
            ErrorMessage = null
        });
        return 1;
    }

    private IQueryable<Alert> BaseQuery(long tenantId) =>
        db.Alerts.AsNoTracking()
            .Include(a => a.AlertRule)
            .Include(a => a.Branch)
            .Include(a => a.Product)
            .Include(a => a.Batch)
            .Where(a => a.AlertRule.TenantId == tenantId);

    private static AlertDto Map(Alert a) => new()
    {
        Id = a.Id,
        AlertRuleId = a.AlertRuleId,
        AlertType = a.AlertRule?.AlertType,
        BranchId = a.BranchId,
        BranchCode = a.Branch?.Code,
        ProductId = a.ProductId,
        ProductSku = a.Product?.Sku,
        ProductName = a.Product?.Name,
        BatchId = a.BatchId,
        BatchNumber = a.Batch?.BatchNumber,
        Severity = a.Severity,
        Title = a.Title,
        Message = a.Message,
        Status = a.Status,
        CreatedAt = a.CreatedAt,
        ResolvedAt = a.ResolvedAt,
        ResolvedBy = a.ResolvedBy
    };
}
