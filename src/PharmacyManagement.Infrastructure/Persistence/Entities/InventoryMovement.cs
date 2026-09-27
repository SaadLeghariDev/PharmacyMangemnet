using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class InventoryMovement
{
    public long Id { get; set; }

    public long BranchId { get; set; }

    public long WarehouseId { get; set; }

    public long WarehouseLocationId { get; set; }

    public long ProductId { get; set; }

    public long BatchId { get; set; }

    public long ProductUnitId { get; set; }

    public string MovementType { get; set; } = null!;

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal TotalCost { get; set; }

    public decimal BalanceBefore { get; set; }

    public decimal BalanceAfter { get; set; }

    public DateTime MovementDate { get; set; }

    public long? PerformedBy { get; set; }

    public string? IdempotencyKey { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual InventoryBatch Batch { get; set; } = null!;

    public virtual Branch Branch { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ProductUnit ProductUnit { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;

    public virtual WarehouseLocation WarehouseLocation { get; set; } = null!;
}
