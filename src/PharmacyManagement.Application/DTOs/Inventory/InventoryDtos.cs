using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Inventory;

public sealed class StockBalanceDto
{
    public long ProductId { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }
    public long BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly ExpiryDate { get; set; }
    public string BatchStatus { get; set; } = string.Empty;
    public bool IsRecalled { get; set; }
    public long WarehouseId { get; set; }
    public long WarehouseLocationId { get; set; }
    public string? LocationCode { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
}

public sealed class StockQuery : PaginationQuery
{
    public long? ProductId { get; set; }
    public long? WarehouseId { get; set; }
    public long? WarehouseLocationId { get; set; }
    public long? BatchId { get; set; }
    public bool? IncludeZero { get; set; }
}

public sealed class FefoCandidateDto
{
    public long BatchId { get; set; }
    public long ProductId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly ExpiryDate { get; set; }
    public long WarehouseLocationId { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal SalePrice { get; set; }
}

public sealed class FefoQuery
{
    public long ProductId { get; set; }
    public long? WarehouseId { get; set; }
    public long? WarehouseLocationId { get; set; }
    public decimal? RequiredQuantity { get; set; }
}

public sealed class NearExpiryQuery : PaginationQuery
{
    public int DaysAhead { get; set; } = 90;
    public long? WarehouseId { get; set; }
    public long? ProductId { get; set; }
}

public sealed class StockTransferLineDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public long BatchId { get; set; }
    public long ProductUnitId { get; set; }
    public long FromLocationId { get; set; }
    public long ToLocationId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
}

public sealed class StockTransferDto
{
    public long Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public long FromWarehouseId { get; set; }
    public long ToWarehouseId { get; set; }
    public DateTime TransferDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public long? RequestedBy { get; set; }
    public long? ApprovedBy { get; set; }
    public long? ReceivedBy { get; set; }
    public IReadOnlyList<StockTransferLineDto> Lines { get; set; } = Array.Empty<StockTransferLineDto>();
}

public sealed class StockTransferLineRequest
{
    public long ProductId { get; set; }
    public long BatchId { get; set; }
    public long ProductUnitId { get; set; }
    public long FromLocationId { get; set; }
    public long ToLocationId { get; set; }
    public decimal Quantity { get; set; }
}

public sealed class CreateStockTransferRequest
{
    public long FromWarehouseId { get; set; }
    public long ToWarehouseId { get; set; }
    public List<StockTransferLineRequest> Lines { get; set; } = new();
}

public sealed class StockAdjustmentLineDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public long BatchId { get; set; }
    public long WarehouseLocationId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public sealed class StockAdjustmentDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public string AdjustmentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long ReasonCodeId { get; set; }
    public long? CreatedBy { get; set; }
    public long? ApprovedBy { get; set; }
    public IReadOnlyList<StockAdjustmentLineDto> Lines { get; set; } = Array.Empty<StockAdjustmentLineDto>();
}

public sealed class StockAdjustmentLineRequest
{
    public long ProductId { get; set; }
    public long BatchId { get; set; }
    public long WarehouseLocationId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public sealed class CreateStockAdjustmentRequest
{
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public string AdjustmentType { get; set; } = string.Empty;
    public long ReasonCodeId { get; set; }
    public List<StockAdjustmentLineRequest> Lines { get; set; } = new();
}

public sealed class StockCountLineDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public long BatchId { get; set; }
    public long WarehouseLocationId { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal VarianceQuantity { get; set; }
    public long ReasonCodeId { get; set; }
}

public sealed class StockCountDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public string CountNumber { get; set; } = string.Empty;
    public DateTime CountDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CountType { get; set; }
    public long? CreatedBy { get; set; }
    public long? ApprovedBy { get; set; }
    public IReadOnlyList<StockCountLineDto> Lines { get; set; } = Array.Empty<StockCountLineDto>();
}

public sealed class StockCountLineRequest
{
    public long ProductId { get; set; }
    public long BatchId { get; set; }
    public long WarehouseLocationId { get; set; }
    public decimal CountedQuantity { get; set; }
    public long ReasonCodeId { get; set; }
}

public sealed class CreateStockCountRequest
{
    public long BranchId { get; set; }
    public long WarehouseId { get; set; }
    public string? CountType { get; set; }
    public List<StockCountLineRequest> Lines { get; set; } = new();
}
