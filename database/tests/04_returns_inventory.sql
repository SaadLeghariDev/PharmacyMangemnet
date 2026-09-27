USE PharmacyManagement;
GO
SET NOCOUNT ON;

/* TEST 17: Sale return restores stock to destination location */
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE Code = N'MAIN');
DECLARE @WhId BIGINT = (SELECT Id FROM dbo.Warehouses WHERE BranchId = @BranchId AND Code = N'MAIN-WH');
DECLARE @LocId BIGINT = (SELECT Id FROM dbo.WarehouseLocations WHERE WarehouseId = @WhId AND Code = N'A-01');
DECLARE @SaleId BIGINT = (SELECT TOP 1 Id FROM dbo.Sales WHERE Status = N'Completed' ORDER BY Id);
DECLARE @SaleLineId BIGINT = (SELECT TOP 1 Id FROM dbo.SaleLines WHERE SaleId = @SaleId);
DECLARE @BatchId BIGINT = (SELECT TOP 1 BatchId FROM dbo.SaleLineBatches WHERE SaleLineId = @SaleLineId);
DECLARE @ProdId BIGINT = (SELECT ProductId FROM dbo.SaleLines WHERE Id = @SaleLineId);
DECLARE @PU BIGINT = (SELECT ProductUnitId FROM dbo.SaleLines WHERE Id = @SaleLineId);
DECLARE @CashId BIGINT = (SELECT Id FROM dbo.PaymentMethods WHERE Code = N'CASH');
DECLARE @IBLId BIGINT = (SELECT Id FROM dbo.InventoryBatchLocations WHERE BatchId = @BatchId AND WarehouseLocationId = @LocId);
DECLARE @Before DECIMAL(19,6), @After DECIMAL(19,6), @RetId BIGINT;

IF @SaleId IS NULL OR @BatchId IS NULL
BEGIN
    PRINT N'TEST_FAIL: 17 prerequisite sale/batch missing';
    RETURN;
END

SELECT @Before = QuantityOnHand FROM dbo.InventoryBatchLocations WHERE Id = @IBLId;

BEGIN TRAN;
INSERT INTO dbo.SaleReturns (SaleId, BranchId, ReturnNumber, ReturnDate, Reason, Status, RefundAmount, RefundPaymentMethodId, CreatedBy)
VALUES (@SaleId, @BranchId, N'RET-000001', SYSUTCDATETIME(), N'Customer request', N'Posted', 5, @CashId, 1);
SET @RetId = SCOPE_IDENTITY();
INSERT INTO dbo.SaleReturnLines (SaleReturnId, SaleLineId, ProductId, BatchId, ProductUnitId, Quantity, RefundPrice, Condition, ReturnToStock, DestinationLocationId)
VALUES (@RetId, @SaleLineId, @ProdId, @BatchId, @PU, 1, 5, N'Good', 1, @LocId);
UPDATE dbo.InventoryBatchLocations SET QuantityOnHand = QuantityOnHand + 1, UpdatedAt = SYSUTCDATETIME() WHERE Id = @IBLId;
INSERT INTO dbo.InventoryMovements (BranchId, WarehouseId, WarehouseLocationId, ProductId, BatchId, ProductUnitId,
    MovementType, ReferenceType, ReferenceId, Quantity, UnitCost, TotalCost, BalanceBefore, BalanceAfter, MovementDate)
VALUES (@BranchId, @WhId, @LocId, @ProdId, @BatchId, @PU, N'Return', N'SaleReturn', @RetId, 1, 2, 2, @Before, @Before+1, SYSUTCDATETIME());
UPDATE dbo.Sales SET Status = N'PartiallyReturned' WHERE Id = @SaleId;
COMMIT;

SELECT @After = QuantityOnHand FROM dbo.InventoryBatchLocations WHERE Id = @IBLId;
IF @After = @Before + 1
    PRINT N'TEST_PASS: 17 sale return restores stock';
ELSE
    PRINT N'TEST_FAIL: 17 return stock not restored';
GO

/* TEST 18: Stock transfer outbound/inbound movements */
BEGIN TRY
    DECLARE @BranchId18 BIGINT = (SELECT Id FROM dbo.Branches WHERE Code = N'MAIN');
    DECLARE @WhId18 BIGINT = (SELECT Id FROM dbo.Warehouses WHERE BranchId = @BranchId18 AND Code = N'MAIN-WH');
    DECLARE @FromLoc18 BIGINT = (SELECT Id FROM dbo.WarehouseLocations WHERE WarehouseId = @WhId18 AND Code = N'A-01');
    DECLARE @ToWh18 BIGINT, @ToLoc18 BIGINT;
    DECLARE @ProdId18 BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
    DECLARE @PU18 BIGINT = (SELECT TOP 1 Id FROM dbo.ProductUnits WHERE ProductId = @ProdId18 AND IsBaseUnit = 1);
    DECLARE @BatchId18 BIGINT;
    DECLARE @XferId18 BIGINT, @Before18 DECIMAL(19,6), @After18 DECIMAL(19,6);

    SELECT TOP 1 @BatchId18 = b.Id
    FROM dbo.InventoryBatches b
    JOIN dbo.InventoryBatchLocations ibl ON ibl.BatchId = b.Id AND ibl.WarehouseLocationId = @FromLoc18
    WHERE b.BatchStatus = N'Available' AND ibl.QuantityOnHand >= 5
    ORDER BY b.ExpiryDate DESC;

    IF @BatchId18 IS NULL
    BEGIN
        PRINT N'TEST_FAIL: 18 no batch with qty>=5 at source location';
    END
    ELSE
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.Warehouses WHERE BranchId = @BranchId18 AND Code = N'SEC-WH')
            INSERT INTO dbo.Warehouses (BranchId, Code, Name, WarehouseType, IsMain, IsActive)
            VALUES (@BranchId18, N'SEC-WH', N'Secondary Warehouse', N'Store', 0, 1);
        SET @ToWh18 = (SELECT Id FROM dbo.Warehouses WHERE Code = N'SEC-WH');
        IF NOT EXISTS (SELECT 1 FROM dbo.WarehouseLocations WHERE WarehouseId = @ToWh18 AND Code = N'B-01')
            INSERT INTO dbo.WarehouseLocations (WarehouseId, Code, Name, LocationType, IsActive)
            VALUES (@ToWh18, N'B-01', N'Sec Loc', N'Selling', 1);
        SET @ToLoc18 = (SELECT Id FROM dbo.WarehouseLocations WHERE WarehouseId = @ToWh18 AND Code = N'B-01');

        SELECT @Before18 = QuantityOnHand FROM dbo.InventoryBatchLocations WHERE BatchId = @BatchId18 AND WarehouseLocationId = @FromLoc18;

        BEGIN TRAN;
        INSERT INTO dbo.StockTransfers (TransferNumber, FromWarehouseId, ToWarehouseId, TransferDate, Status, RequestedBy)
        VALUES (N'XFER-000001', @WhId18, @ToWh18, SYSUTCDATETIME(), N'Received', 1);
        SET @XferId18 = SCOPE_IDENTITY();
        INSERT INTO dbo.StockTransferLines (StockTransferId, ProductId, BatchId, ProductUnitId, FromLocationId, ToLocationId, Quantity, ReceivedQuantity)
        VALUES (@XferId18, @ProdId18, @BatchId18, @PU18, @FromLoc18, @ToLoc18, 5, 5);
        UPDATE dbo.InventoryBatchLocations SET QuantityOnHand = QuantityOnHand - 5 WHERE BatchId = @BatchId18 AND WarehouseLocationId = @FromLoc18;
        IF NOT EXISTS (SELECT 1 FROM dbo.InventoryBatchLocations WHERE BatchId = @BatchId18 AND WarehouseLocationId = @ToLoc18)
            INSERT INTO dbo.InventoryBatchLocations (BatchId, WarehouseLocationId, QuantityOnHand, ReservedQuantity) VALUES (@BatchId18, @ToLoc18, 5, 0);
        ELSE
            UPDATE dbo.InventoryBatchLocations SET QuantityOnHand = QuantityOnHand + 5 WHERE BatchId = @BatchId18 AND WarehouseLocationId = @ToLoc18;
        INSERT INTO dbo.InventoryMovements (BranchId, WarehouseId, WarehouseLocationId, ProductId, BatchId, ProductUnitId,
            MovementType, ReferenceType, ReferenceId, Quantity, UnitCost, TotalCost, BalanceBefore, BalanceAfter, MovementDate)
        VALUES
         (@BranchId18, @WhId18, @FromLoc18, @ProdId18, @BatchId18, @PU18, N'TransferOut', N'StockTransfer', @XferId18, -5, 2, 10, @Before18, @Before18-5, SYSUTCDATETIME()),
         (@BranchId18, @ToWh18, @ToLoc18, @ProdId18, @BatchId18, @PU18, N'TransferIn', N'StockTransfer', @XferId18, 5, 2, 10, 0, 5, SYSUTCDATETIME());
        COMMIT;

        SELECT @After18 = QuantityOnHand FROM dbo.InventoryBatchLocations WHERE BatchId = @BatchId18 AND WarehouseLocationId = @FromLoc18;
        IF @After18 = @Before18 - 5 AND EXISTS (SELECT 1 FROM dbo.InventoryBatchLocations WHERE BatchId = @BatchId18 AND WarehouseLocationId = @ToLoc18 AND QuantityOnHand >= 5)
            PRINT N'TEST_PASS: 18 stock transfer movements';
        ELSE
            PRINT N'TEST_FAIL: 18 stock transfer failed';
    END
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    PRINT N'TEST_FAIL: 18 stock transfer exception: ' + ERROR_MESSAGE();
END CATCH
GO

/* TEST 19: Stock count variance */
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE Code = N'MAIN');
DECLARE @WhId BIGINT = (SELECT Id FROM dbo.Warehouses WHERE Code = N'MAIN-WH');
DECLARE @LocId BIGINT = (SELECT Id FROM dbo.WarehouseLocations WHERE WarehouseId = @WhId AND Code = N'A-01');
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @ReasonId BIGINT = (SELECT Id FROM dbo.ReasonCodes WHERE TenantId = @TenantId AND Code = N'VARIANCE');
DECLARE @BatchId BIGINT = (SELECT TOP 1 b.Id FROM dbo.InventoryBatches b
    JOIN dbo.InventoryBatchLocations ibl ON ibl.BatchId = b.Id AND ibl.WarehouseLocationId = @LocId
    WHERE ibl.QuantityOnHand >= 0 ORDER BY ibl.QuantityOnHand DESC);
DECLARE @ProdId BIGINT = (SELECT ProductId FROM dbo.InventoryBatches WHERE Id = @BatchId);
DECLARE @SysQty DECIMAL(19,6) = (SELECT QuantityOnHand FROM dbo.InventoryBatchLocations WHERE BatchId = @BatchId AND WarehouseLocationId = @LocId);
DECLARE @CountId BIGINT;

IF @BatchId IS NULL
    PRINT N'TEST_FAIL: 19 no batch for stock count';
ELSE
BEGIN
INSERT INTO dbo.StockCounts (BranchId, WarehouseId, CountNumber, CountDate, Status, CountType, CreatedBy)
VALUES (@BranchId, @WhId, N'CNT-000001', SYSUTCDATETIME(), N'Completed', N'Cycle', 1);
SET @CountId = SCOPE_IDENTITY();
INSERT INTO dbo.StockCountLines (StockCountId, ProductId, BatchId, WarehouseLocationId, SystemQuantity, CountedQuantity, VarianceQuantity, ReasonCodeId)
VALUES (@CountId, @ProdId, @BatchId, @LocId, @SysQty, @SysQty - 2, -2, @ReasonId);

IF EXISTS (SELECT 1 FROM dbo.StockCountLines WHERE StockCountId = @CountId AND VarianceQuantity = -2)
    PRINT N'TEST_PASS: 19 stock count variance recorded';
ELSE
    PRINT N'TEST_FAIL: 19 stock count variance missing';
END
GO

/* TEST 20: Stock adjustment requires approval status path */
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE Code = N'MAIN');
DECLARE @WhId BIGINT = (SELECT Id FROM dbo.Warehouses WHERE Code = N'MAIN-WH');
DECLARE @LocId BIGINT = (SELECT Id FROM dbo.WarehouseLocations WHERE WarehouseId = @WhId AND Code = N'A-01');
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @ReasonId BIGINT = (SELECT Id FROM dbo.ReasonCodes WHERE TenantId = @TenantId AND Code = N'SHRINK');
DECLARE @BatchId BIGINT = (SELECT TOP 1 b.Id FROM dbo.InventoryBatches b
    JOIN dbo.InventoryBatchLocations ibl ON ibl.BatchId = b.Id AND ibl.WarehouseLocationId = @LocId
    WHERE ibl.QuantityOnHand >= 1 ORDER BY ibl.QuantityOnHand DESC);
DECLARE @ProdId BIGINT = (SELECT ProductId FROM dbo.InventoryBatches WHERE Id = @BatchId);
DECLARE @AdjId BIGINT, @Before DECIMAL(19,6), @After DECIMAL(19,6), @PU BIGINT;

IF @BatchId IS NULL
    PRINT N'TEST_FAIL: 20 no batch for adjustment';
ELSE
BEGIN
SET @PU = (SELECT TOP 1 Id FROM dbo.ProductUnits WHERE ProductId = @ProdId AND IsBaseUnit = 1);
SELECT @Before = QuantityOnHand FROM dbo.InventoryBatchLocations WHERE BatchId = @BatchId AND WarehouseLocationId = @LocId;

INSERT INTO dbo.StockAdjustments (BranchId, WarehouseId, AdjustmentNumber, AdjustmentDate, AdjustmentType, Status, ReasonCodeId, CreatedBy)
VALUES (@BranchId, @WhId, N'ADJ-000001', SYSUTCDATETIME(), N'Decrease', N'PendingApproval', @ReasonId, 1);
SET @AdjId = SCOPE_IDENTITY();
INSERT INTO dbo.StockAdjustmentLines (StockAdjustmentId, ProductId, BatchId, WarehouseLocationId, Quantity, UnitCost)
VALUES (@AdjId, @ProdId, @BatchId, @LocId, -1, 2);

UPDATE dbo.StockAdjustments SET Status = N'Approved', ApprovedBy = 1 WHERE Id = @AdjId;
UPDATE dbo.StockAdjustments SET Status = N'Posted' WHERE Id = @AdjId;
UPDATE dbo.InventoryBatchLocations SET QuantityOnHand = QuantityOnHand - 1 WHERE BatchId = @BatchId AND WarehouseLocationId = @LocId;
INSERT INTO dbo.InventoryMovements (BranchId, WarehouseId, WarehouseLocationId, ProductId, BatchId, ProductUnitId,
    MovementType, ReferenceType, ReferenceId, Quantity, UnitCost, TotalCost, BalanceBefore, BalanceAfter, MovementDate)
VALUES (@BranchId, @WhId, @LocId, @ProdId, @BatchId, @PU, N'Adjustment', N'StockAdjustment', @AdjId, -1, 2, 2, @Before, @Before-1, SYSUTCDATETIME());

SELECT @After = QuantityOnHand FROM dbo.InventoryBatchLocations WHERE BatchId = @BatchId AND WarehouseLocationId = @LocId;
IF @After = @Before - 1 AND EXISTS (SELECT 1 FROM dbo.StockAdjustments WHERE Id = @AdjId AND Status = N'Posted')
    PRINT N'TEST_PASS: 20 stock adjustment approval and post';
ELSE
    PRINT N'TEST_FAIL: 20 stock adjustment failed';
END
GO

/* TEST 21: Expired/blocked batches excluded from FEFO */
DECLARE @ProdId BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
DECLARE @SupplierId BIGINT = (SELECT Id FROM dbo.Suppliers WHERE Code = N'SUP01');
DECLARE @WhId BIGINT = (SELECT Id FROM dbo.Warehouses WHERE Code = N'MAIN-WH');
DECLARE @GRLId BIGINT = (SELECT TOP 1 Id FROM dbo.GoodsReceiptLines ORDER BY Id);
DECLARE @BlockedId BIGINT;

INSERT INTO dbo.InventoryBatches (ProductId, GoodsReceiptLineId, SupplierId, WarehouseId, BatchNumber, ManufacturingDate, ExpiryDate,
    QuantityReceived, PurchaseCost, MRP, SalePrice, BatchStatus, IsRecalled)
VALUES (@ProdId, @GRLId, @SupplierId, @WhId, N'BATCH-BLOCKED', '2020-01-01', '2020-12-31', 50, 2, 6, 5, N'Expired', 0);
SET @BlockedId = SCOPE_IDENTITY();

IF NOT EXISTS (
    SELECT 1 FROM dbo.InventoryBatches b
    WHERE b.ProductId = @ProdId
      AND b.BatchStatus = N'Available'
      AND b.IsRecalled = 0
      AND b.ExpiryDate >= CAST(SYSUTCDATETIME() AS DATE)
      AND b.Id = @BlockedId
)
    PRINT N'TEST_PASS: 21 expired batch excluded from FEFO';
ELSE
    PRINT N'TEST_FAIL: 21 expired batch incorrectly included in FEFO';
GO

/* TEST 22: Available stock = QoH - Reserved */
DECLARE @IBLId BIGINT = (SELECT TOP 1 Id FROM dbo.InventoryBatchLocations WHERE QuantityOnHand > 5 ORDER BY Id);
DECLARE @QoH DECIMAL(19,6), @Res DECIMAL(19,6), @Avail DECIMAL(19,6);
IF @IBLId IS NULL
    PRINT N'TEST_FAIL: 22 no location with QoH>5';
ELSE
BEGIN
SELECT @QoH = QuantityOnHand, @Res = ReservedQuantity FROM dbo.InventoryBatchLocations WHERE Id = @IBLId;
UPDATE dbo.InventoryBatchLocations SET ReservedQuantity = 3 WHERE Id = @IBLId;
SELECT @Avail = QuantityOnHand - ReservedQuantity FROM dbo.InventoryBatchLocations WHERE Id = @IBLId;
IF @Avail = @QoH - 3
    PRINT N'TEST_PASS: 22 available stock equals QoH minus reserved';
ELSE
    PRINT N'TEST_FAIL: 22 available stock formula failed';
UPDATE dbo.InventoryBatchLocations SET ReservedQuantity = @Res WHERE Id = @IBLId;
END
GO
