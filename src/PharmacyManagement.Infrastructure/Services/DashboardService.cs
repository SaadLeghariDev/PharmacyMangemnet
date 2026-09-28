using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.DTOs.Dashboard;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class DashboardService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IDashboardService
{
    private const string CompletedStatus = "Completed";
    private const int RiskSampleSize = 8;
    private const int TopProductsSize = 10;
    private const int RecentSalesSize = 10;
    private const int NearExpiryDays = 90;

    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<DashboardSummaryDto> GetSummaryAsync(DashboardSummaryQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var now = DateTime.UtcNow;
        var to = query.To ?? now;
        var from = query.From ?? to.Date;
        if (from > to)
            (from, to) = (to, from);

        var periodLength = to - from;
        if (periodLength < TimeSpan.FromMinutes(1))
            periodLength = TimeSpan.FromDays(1);
        var prevTo = from.AddTicks(-1);
        var prevFrom = prevTo - periodLength;

        var salesQ = db.Sales.AsNoTracking()
            .Where(s => s.Branch.TenantId == tenantId && s.Status == CompletedStatus);
        if (query.BranchId is long branchId)
            salesQ = salesQ.Where(s => s.BranchId == branchId);

        var periodSales = salesQ.Where(s => s.SaleDate >= from && s.SaleDate <= to);
        var prevSales = salesQ.Where(s => s.SaleDate >= prevFrom && s.SaleDate <= prevTo);

        var salesTotal = await periodSales.SumAsync(s => (decimal?)s.NetAmount, ct) ?? 0m;
        var billsCount = await periodSales.CountAsync(ct);
        var prevSalesTotal = await prevSales.SumAsync(s => (decimal?)s.NetAmount, ct) ?? 0m;
        var prevBillsCount = await prevSales.CountAsync(ct);

        var salesByDay = await periodSales
            .GroupBy(s => s.SaleDate.Date)
            .Select(g => new { Day = g.Key, Total = g.Sum(x => x.NetAmount) })
            .OrderBy(x => x.Day)
            .ToListAsync(ct);

        var isSingleDay = to.Date == from.Date;
        List<DashboardSeriesPointDto> salesByHour = [];
        if (isSingleDay)
        {
            var hourly = await periodSales
                .GroupBy(s => s.SaleDate.Hour)
                .Select(g => new { Hour = g.Key, Total = g.Sum(x => x.NetAmount) })
                .OrderBy(x => x.Hour)
                .ToListAsync(ct);
            salesByHour = hourly
                .Select(h => new DashboardSeriesPointDto
                {
                    Label = $"{h.Hour:00}:00",
                    Value = h.Total
                })
                .ToList();
        }

        var paymentBreakdown = await db.SalePayments.AsNoTracking()
            .Where(p =>
                p.Sale.Branch.TenantId == tenantId &&
                p.Sale.Status == CompletedStatus &&
                p.Sale.SaleDate >= from &&
                p.Sale.SaleDate <= to &&
                p.Status == CompletedStatus &&
                p.TransactionType == "Payment")
            .Where(p => query.BranchId == null || p.Sale.BranchId == query.BranchId)
            .GroupBy(p => p.PaymentMethod.Name)
            .Select(g => new DashboardPaymentBreakdownDto
            {
                MethodName = g.Key,
                Amount = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync(ct);

        var topProducts = await db.SaleLines.AsNoTracking()
            .Where(l =>
                l.Sale.Branch.TenantId == tenantId &&
                l.Sale.Status == CompletedStatus &&
                l.Sale.SaleDate >= from &&
                l.Sale.SaleDate <= to)
            .Where(l => query.BranchId == null || l.Sale.BranchId == query.BranchId)
            .GroupBy(l => new { l.ProductId, l.Product.Name, l.Product.Sku })
            .Select(g => new DashboardTopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                Sku = g.Key.Sku,
                Quantity = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.NetAmount)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(TopProductsSize)
            .ToListAsync(ct);

        var recentSales = await periodSales
            .OrderByDescending(s => s.SaleDate)
            .ThenByDescending(s => s.Id)
            .Take(RecentSalesSize)
            .Select(s => new DashboardRecentSaleDto
            {
                Id = s.Id,
                InvoiceNumber = s.InvoiceNumber,
                SaleDate = s.SaleDate,
                NetAmount = s.NetAmount
            })
            .ToListAsync(ct);

        var (lowStockCount, lowStockRisk) = await GetLowStockAsync(tenantId, query.BranchId, ct);
        var (nearExpiryCount, nearExpiryRisk) = await GetNearExpiryAsync(tenantId, query.BranchId, ct);
        var openCriticalAlerts = await db.Alerts.AsNoTracking()
            .Where(a =>
                a.AlertRule.TenantId == tenantId &&
                a.Status == AlertStatuses.Open &&
                a.Severity == AlertSeverities.Critical)
            .Where(a => query.BranchId == null || a.BranchId == query.BranchId)
            .CountAsync(ct);

        var riskItems = lowStockRisk
            .Concat(nearExpiryRisk)
            .Take(RiskSampleSize)
            .ToList();

        return new DashboardSummaryDto
        {
            From = from,
            To = to,
            BranchId = query.BranchId,
            Kpis = new DashboardKpisDto
            {
                SalesTotal = salesTotal,
                BillsCount = billsCount,
                AvgTicket = billsCount > 0 ? Math.Round(salesTotal / billsCount, 2) : 0m,
                LowStockCount = lowStockCount,
                NearExpiryCount = nearExpiryCount,
                OpenCriticalAlerts = openCriticalAlerts,
                PreviousPeriodSalesTotal = prevSalesTotal,
                PreviousPeriodBillsCount = prevBillsCount
            },
            SalesByDay = salesByDay
                .Select(d => new DashboardSeriesPointDto
                {
                    Label = d.Day.ToString("yyyy-MM-dd"),
                    Value = d.Total
                })
                .ToList(),
            SalesByHour = salesByHour,
            PaymentBreakdown = paymentBreakdown,
            TopProducts = topProducts,
            RecentSales = recentSales,
            RiskItems = riskItems
        };
    }

    private async Task<(int Count, List<DashboardRiskItemDto> Items)> GetLowStockAsync(
        long tenantId, long? branchId, CancellationToken ct)
    {
        var rules = await db.ReorderRules.AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.Branch.TenantId == tenantId && r.IsActive)
            .Where(r => branchId == null || r.BranchId == branchId)
            .ToListAsync(ct);

        if (rules.Count == 0)
            return (0, []);

        var productIds = rules.Select(r => r.ProductId).Distinct().ToList();
        var warehouseIds = rules.Select(r => r.WarehouseId).Distinct().ToList();

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
        var items = new List<DashboardRiskItemDto>();
        foreach (var rule in rules)
        {
            stockMap.TryGetValue((rule.ProductId, rule.WarehouseId), out var stock);
            var available = stock?.Available ?? 0m;
            if (available > rule.ReorderPoint) continue;
            items.Add(new DashboardRiskItemDto
            {
                Kind = "LowStock",
                Name = rule.Product.Name,
                Sku = rule.Product.Sku,
                Quantity = available,
                ReorderPoint = rule.ReorderPoint
            });
        }

        items = items.OrderBy(i => i.Quantity).ThenBy(i => i.Name).ToList();
        return (items.Count, items.Take(RiskSampleSize).ToList());
    }

    private async Task<(int Count, List<DashboardRiskItemDto> Items)> GetNearExpiryAsync(
        long tenantId, long? branchId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = today.AddDays(NearExpiryDays);

        var q = db.InventoryBatchLocations.AsNoTracking()
            .Where(x =>
                x.Batch.Product.TenantId == tenantId &&
                x.Batch.ExpiryDate >= today &&
                x.Batch.ExpiryDate <= cutoff &&
                x.Batch.BatchStatus == BatchStatuses.Available &&
                !x.Batch.IsRecalled &&
                x.QuantityOnHand - x.ReservedQuantity > 0);

        if (branchId is long bid)
            q = q.Where(x => x.Batch.Warehouse.BranchId == bid);

        var count = await q.CountAsync(ct);
        var rows = await q
            .OrderBy(x => x.Batch.ExpiryDate)
            .Take(RiskSampleSize)
            .Select(x => new DashboardRiskItemDto
            {
                Kind = "NearExpiry",
                Name = x.Batch.Product.Name,
                Sku = x.Batch.Product.Sku,
                Quantity = x.QuantityOnHand - x.ReservedQuantity,
                ExpiryDate = x.Batch.ExpiryDate
            })
            .ToListAsync(ct);

        return (count, rows);
    }
}
