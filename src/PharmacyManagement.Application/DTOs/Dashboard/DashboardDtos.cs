namespace PharmacyManagement.Application.DTOs.Dashboard;

public sealed class DashboardSummaryQuery
{
    public long? BranchId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public sealed class DashboardKpisDto
{
    public decimal SalesTotal { get; set; }
    public int BillsCount { get; set; }
    public decimal AvgTicket { get; set; }
    public int LowStockCount { get; set; }
    public int NearExpiryCount { get; set; }
    public int OpenCriticalAlerts { get; set; }
    public decimal PreviousPeriodSalesTotal { get; set; }
    public int PreviousPeriodBillsCount { get; set; }
}

public sealed class DashboardSeriesPointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public sealed class DashboardPaymentBreakdownDto
{
    public string MethodName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public sealed class DashboardTopProductDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Quantity { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class DashboardRecentSaleDto
{
    public long Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal NetAmount { get; set; }
}

public sealed class DashboardRiskItemDto
{
    public string Kind { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Quantity { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public decimal? ReorderPoint { get; set; }
}

public sealed class DashboardSummaryDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public long? BranchId { get; set; }
    public DashboardKpisDto Kpis { get; set; } = new();
    public IReadOnlyList<DashboardSeriesPointDto> SalesByDay { get; set; } = Array.Empty<DashboardSeriesPointDto>();
    public IReadOnlyList<DashboardSeriesPointDto> SalesByHour { get; set; } = Array.Empty<DashboardSeriesPointDto>();
    public IReadOnlyList<DashboardPaymentBreakdownDto> PaymentBreakdown { get; set; } = Array.Empty<DashboardPaymentBreakdownDto>();
    public IReadOnlyList<DashboardTopProductDto> TopProducts { get; set; } = Array.Empty<DashboardTopProductDto>();
    public IReadOnlyList<DashboardRecentSaleDto> RecentSales { get; set; } = Array.Empty<DashboardRecentSaleDto>();
    public IReadOnlyList<DashboardRiskItemDto> RiskItems { get; set; } = Array.Empty<DashboardRiskItemDto>();
}
