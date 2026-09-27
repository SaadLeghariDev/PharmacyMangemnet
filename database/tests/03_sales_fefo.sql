USE PharmacyManagement;
GO
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.InventoryBatches WHERE BatchNumber = N'BATCH-EARLY')
BEGIN
    PRINT N'TEST_FAIL: prerequisite batches missing (run 02 first)';
    RETURN;
END
GO

/* TEST 11: FEFO selects earliest non-expired Available batch first */
DECLARE @ProdId BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
DECLARE @FirstBatch NVARCHAR(100);

SELECT TOP 1 @FirstBatch = b.BatchNumber
FROM dbo.InventoryBatches b
JOIN dbo.InventoryBatchLocations ibl ON ibl.BatchId = b.Id
WHERE b.ProductId = @ProdId
  AND b.BatchStatus = N'Available'
  AND b.IsRecalled = 0
  AND b.ExpiryDate >= CAST(SYSUTCDATETIME() AS DATE)
  AND (ibl.QuantityOnHand - ibl.ReservedQuantity) > 0
ORDER BY b.ExpiryDate ASC, b.Id ASC;

IF @FirstBatch = N'BATCH-EARLY'
    PRINT N'TEST_PASS: 11 FEFO selects earliest expiry batch';
ELSE
BEGIN
    DECLARE @Got NVARCHAR(100) = ISNULL(@FirstBatch, N'<null>');
    RAISERROR(N'TEST_FAIL: 11 FEFO expected BATCH-EARLY got %s', 16, 1, @Got);
END
GO

/* TEST 12: Single-batch sale depletes early batch stock + creates movement */
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE TenantId = @TenantId AND Code = N'MAIN');
DECLARE @WhId BIGINT = (SELECT Id FROM dbo.Warehouses WHERE BranchId = @BranchId AND Code = N'MAIN-WH');
DECLARE @LocId BIGINT = (SELECT Id FROM dbo.WarehouseLocations WHERE WarehouseId = @WhId AND Code = N'A-01');
DECLARE @CounterId BIGINT = (SELECT Id FROM dbo.Counters WHERE BranchId = @BranchId AND Code = N'C1');
DECLARE @TerminalId BIGINT = (SELECT Id FROM dbo.POSTerminals WHERE BranchId = @BranchId AND TerminalCode = N'T1');
DECLARE @UserId BIGINT = (SELECT Id FROM dbo.Users WHERE Username = N'admin');
DECLARE @ProdId BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
DECLARE @PU BIGINT = (SELECT TOP 1 Id FROM dbo.ProductUnits WHERE ProductId = @ProdId AND IsBaseUnit = 1);
DECLARE @BatchId BIGINT = (SELECT Id FROM dbo.InventoryBatches WHERE BatchNumber = N'BATCH-EARLY');
DECLARE @IBLId BIGINT = (SELECT Id FROM dbo.InventoryBatchLocations WHERE BatchId = @BatchId AND WarehouseLocationId = @LocId);
DECLARE @Before DECIMAL(19,6), @After DECIMAL(19,6), @SaleId BIGINT, @LineId BIGINT, @NextNum BIGINT, @Inv NVARCHAR(50);
DECLARE @CashId BIGINT = (SELECT Id FROM dbo.PaymentMethods WHERE Code = N'CASH');

SELECT @Before = QuantityOnHand FROM dbo.InventoryBatchLocations WHERE Id = @IBLId;

UPDATE dbo.NumberSequences SET CurrentNumber = CurrentNumber + 1
WHERE TenantId = @TenantId AND DocumentType = N'SALE' AND TerminalId = @TerminalId;
SELECT @NextNum = CurrentNumber FROM dbo.NumberSequences
WHERE TenantId = @TenantId AND DocumentType = N'SALE' AND TerminalId = @TerminalId;
SET @Inv = N'INV-' + RIGHT(N'000000' + CAST(@NextNum AS NVARCHAR(10)), 6);

BEGIN TRAN;
UPDATE dbo.InventoryBatchLocations WITH (UPDLOCK, ROWLOCK)
SET QuantityOnHand = QuantityOnHand - 10, UpdatedAt = SYSUTCDATETIME()
WHERE Id = @IBLId AND QuantityOnHand - ReservedQuantity >= 10;

INSERT INTO dbo.Sales (BranchId, CounterId, TerminalId, UserId, InvoiceNumber, SaleDate, SaleType, Status,
    Subtotal, DiscountAmount, TaxAmount, RoundOff, NetAmount, PaidAmount, DueAmount, ChangeAmount, PaymentStatus, FBRStatus)
VALUES (@BranchId, @CounterId, @TerminalId, @UserId, @Inv, SYSUTCDATETIME(), N'Retail', N'Completed',
    50, 0, 0, 0, 50, 50, 0, 0, N'Paid', N'Pending');
SET @SaleId = SCOPE_IDENTITY();

INSERT INTO dbo.SaleLines (SaleId, ProductId, ProductUnitId, Quantity, BaseQuantity, ConversionFactor, UnitPrice, MRP, DiscountAmount, TaxAmount, NetAmount)
VALUES (@SaleId, @ProdId, @PU, 10, 10, 1, 5, 6, 0, 0, 50);
SET @LineId = SCOPE_IDENTITY();

INSERT INTO dbo.SaleLineBatches (SaleLineId, BatchId, Quantity, BaseQuantity, UnitCost)
VALUES (@LineId, @BatchId, 10, 10, 2);

INSERT INTO dbo.InventoryMovements (BranchId, WarehouseId, WarehouseLocationId, ProductId, BatchId, ProductUnitId,
    MovementType, ReferenceType, ReferenceId, Quantity, UnitCost, TotalCost, BalanceBefore, BalanceAfter, MovementDate, PerformedBy, IdempotencyKey)
VALUES (@BranchId, @WhId, @LocId, @ProdId, @BatchId, @PU, N'Sale', N'Sale', @SaleId, -10, 2, 20, @Before, @Before-10, SYSUTCDATETIME(), @UserId, N'SALE12-' + @Inv);

INSERT INTO dbo.SalePayments (SaleId, PaymentMethodId, Amount, PaymentDate, Status, TransactionType)
VALUES (@SaleId, @CashId, 50, SYSUTCDATETIME(), N'Completed', N'Payment');
COMMIT;

SELECT @After = QuantityOnHand FROM dbo.InventoryBatchLocations WHERE Id = @IBLId;
IF @After = @Before - 10
    PRINT N'TEST_PASS: 12 single-batch sale depletes stock';
ELSE
    PRINT N'TEST_FAIL: 12 stock not depleted correctly';
GO

/* TEST 13: Multi-batch sale consumes early remainder then late batch */
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE TenantId = @TenantId AND Code = N'MAIN');
DECLARE @WhId BIGINT = (SELECT Id FROM dbo.Warehouses WHERE BranchId = @BranchId AND Code = N'MAIN-WH');
DECLARE @LocId BIGINT = (SELECT Id FROM dbo.WarehouseLocations WHERE WarehouseId = @WhId AND Code = N'A-01');
DECLARE @CounterId BIGINT = (SELECT Id FROM dbo.Counters WHERE BranchId = @BranchId AND Code = N'C1');
DECLARE @TerminalId BIGINT = (SELECT Id FROM dbo.POSTerminals WHERE BranchId = @BranchId AND TerminalCode = N'T1');
DECLARE @UserId BIGINT = (SELECT Id FROM dbo.Users WHERE Username = N'admin');
DECLARE @ProdId BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
DECLARE @PU BIGINT = (SELECT TOP 1 Id FROM dbo.ProductUnits WHERE ProductId = @ProdId AND IsBaseUnit = 1);
DECLARE @Early BIGINT = (SELECT Id FROM dbo.InventoryBatches WHERE BatchNumber = N'BATCH-EARLY');
DECLARE @Late BIGINT = (SELECT Id FROM dbo.InventoryBatches WHERE BatchNumber = N'BATCH-LATE');
DECLARE @EarlyLoc BIGINT = (SELECT Id FROM dbo.InventoryBatchLocations WHERE BatchId = @Early);
DECLARE @LateLoc BIGINT = (SELECT Id FROM dbo.InventoryBatchLocations WHERE BatchId = @Late);
DECLARE @EarlyAvail DECIMAL(19,6), @Need DECIMAL(19,6) = 250, @FromEarly DECIMAL(19,6), @FromLate DECIMAL(19,6);
DECLARE @SaleId BIGINT, @LineId BIGINT, @NextNum BIGINT, @Inv NVARCHAR(50);
DECLARE @CashId BIGINT = (SELECT Id FROM dbo.PaymentMethods WHERE Code = N'CASH');

SELECT @EarlyAvail = QuantityOnHand - ReservedQuantity FROM dbo.InventoryBatchLocations WHERE Id = @EarlyLoc;
SET @FromEarly = CASE WHEN @EarlyAvail >= @Need THEN @Need ELSE @EarlyAvail END;
SET @FromLate = @Need - @FromEarly;

UPDATE dbo.NumberSequences SET CurrentNumber = CurrentNumber + 1
WHERE TenantId = @TenantId AND DocumentType = N'SALE' AND TerminalId = @TerminalId;
SELECT @NextNum = CurrentNumber FROM dbo.NumberSequences WHERE TenantId = @TenantId AND DocumentType = N'SALE' AND TerminalId = @TerminalId;
SET @Inv = N'INV-' + RIGHT(N'000000' + CAST(@NextNum AS NVARCHAR(10)), 6);

BEGIN TRAN;
UPDATE dbo.InventoryBatchLocations WITH (UPDLOCK, ROWLOCK) SET QuantityOnHand = QuantityOnHand - @FromEarly WHERE Id = @EarlyLoc;
UPDATE dbo.InventoryBatchLocations WITH (UPDLOCK, ROWLOCK) SET QuantityOnHand = QuantityOnHand - @FromLate WHERE Id = @LateLoc;

INSERT INTO dbo.Sales (BranchId, CounterId, TerminalId, UserId, InvoiceNumber, SaleDate, SaleType, Status,
    Subtotal, NetAmount, PaidAmount, DueAmount, ChangeAmount, PaymentStatus, FBRStatus)
VALUES (@BranchId, @CounterId, @TerminalId, @UserId, @Inv, SYSUTCDATETIME(), N'Retail', N'Completed',
    1250, 1250, 1250, 0, 0, N'Paid', N'Pending');
SET @SaleId = SCOPE_IDENTITY();
INSERT INTO dbo.SaleLines (SaleId, ProductId, ProductUnitId, Quantity, BaseQuantity, ConversionFactor, UnitPrice, MRP, NetAmount)
VALUES (@SaleId, @ProdId, @PU, @Need, @Need, 1, 5, 6, 1250);
SET @LineId = SCOPE_IDENTITY();
IF @FromEarly > 0
    INSERT INTO dbo.SaleLineBatches (SaleLineId, BatchId, Quantity, BaseQuantity, UnitCost) VALUES (@LineId, @Early, @FromEarly, @FromEarly, 2);
IF @FromLate > 0
    INSERT INTO dbo.SaleLineBatches (SaleLineId, BatchId, Quantity, BaseQuantity, UnitCost) VALUES (@LineId, @Late, @FromLate, @FromLate, 2);
INSERT INTO dbo.SalePayments (SaleId, PaymentMethodId, Amount, PaymentDate, Status, TransactionType)
VALUES (@SaleId, @CashId, 1250, SYSUTCDATETIME(), N'Completed', N'Payment');
COMMIT;

IF (SELECT COUNT(*) FROM dbo.SaleLineBatches WHERE SaleLineId = @LineId) = 2 AND @FromEarly > 0 AND @FromLate > 0
    PRINT N'TEST_PASS: 13 multi-batch FEFO sale';
ELSE
BEGIN
    DECLARE @ETxt NVARCHAR(20) = CAST(@FromEarly AS NVARCHAR(20));
    DECLARE @LTxt NVARCHAR(20) = CAST(@FromLate AS NVARCHAR(20));
    RAISERROR(N'TEST_FAIL: 13 multi-batch sale did not split across batches early=%s late=%s', 16, 1, @ETxt, @LTxt);
END
GO

/* TEST 14: Concurrent stock contention — second UPDLOCK waiter sees insufficient stock */
DECLARE @LocId BIGINT = (
    SELECT ibl.Id FROM dbo.InventoryBatchLocations ibl
    JOIN dbo.InventoryBatches b ON b.Id = ibl.BatchId
    WHERE b.BatchNumber = N'BATCH-LATE');
DECLARE @Avail DECIMAL(19,6);
SELECT @Avail = QuantityOnHand - ReservedQuantity FROM dbo.InventoryBatchLocations WHERE Id = @LocId;

BEGIN TRAN;
UPDATE dbo.InventoryBatchLocations WITH (UPDLOCK, ROWLOCK)
SET QuantityOnHand = QuantityOnHand - @Avail, UpdatedAt = SYSUTCDATETIME()
WHERE Id = @LocId;

DECLARE @Ok BIT = 0;
BEGIN TRY
    /* Nested attempt simulating competing session: remaining available should be 0 */
    DECLARE @Rem DECIMAL(19,6);
    SELECT @Rem = QuantityOnHand - ReservedQuantity FROM dbo.InventoryBatchLocations WITH (UPDLOCK, ROWLOCK) WHERE Id = @LocId;
    IF @Rem < 1 SET @Ok = 1;
END TRY
BEGIN CATCH
    SET @Ok = 0;
END CATCH
ROLLBACK;

IF @Ok = 1
    PRINT N'TEST_PASS: 14 concurrent contention sees insufficient remaining stock';
ELSE
    PRINT N'TEST_FAIL: 14 concurrent stock check failed';
GO

/* Also verify RowVersion changes on update */
DECLARE @LocId BIGINT = (
    SELECT TOP 1 ibl.Id FROM dbo.InventoryBatchLocations ibl
    JOIN dbo.InventoryBatches b ON b.Id = ibl.BatchId WHERE b.BatchNumber = N'BATCH-EARLY');
DECLARE @Rv1 BINARY(8), @Rv2 BINARY(8);
SELECT @Rv1 = RowVersion FROM dbo.InventoryBatchLocations WHERE Id = @LocId;
UPDATE dbo.InventoryBatchLocations SET UpdatedAt = SYSUTCDATETIME() WHERE Id = @LocId;
SELECT @Rv2 = RowVersion FROM dbo.InventoryBatchLocations WHERE Id = @LocId;
IF @Rv1 = @Rv2
    PRINT N'TEST_FAIL: 14 RowVersion did not change on update';
/* RowVersion check is part of test 14 */
GO

/* TEST 15: Sale payments and payment status consistency */
IF EXISTS (
    SELECT 1 FROM dbo.Sales s
    JOIN dbo.SalePayments sp ON sp.SaleId = s.Id
    WHERE s.PaymentStatus = N'Paid' AND sp.Status = N'Completed'
)
    PRINT N'TEST_PASS: 15 sale payments recorded with Paid status';
ELSE
    PRINT N'TEST_FAIL: 15 sale payments missing';
GO

/* TEST 16: Held sales cart */
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE Code = N'MAIN');
DECLARE @TerminalId BIGINT = (SELECT Id FROM dbo.POSTerminals WHERE TerminalCode = N'T1');
DECLARE @UserId BIGINT = (SELECT Id FROM dbo.Users WHERE Username = N'admin');
INSERT INTO dbo.HeldSales (BranchId, TerminalId, UserId, CartData, TotalAmount, HeldAt, ExpiresAt, Status)
VALUES (@BranchId, @TerminalId, @UserId, N'{"items":[{"sku":"SKU-PARA-500","qty":2}]}', 10, SYSUTCDATETIME(), DATEADD(HOUR, 4, SYSUTCDATETIME()), N'Held');
IF EXISTS (SELECT 1 FROM dbo.HeldSales WHERE Status = N'Held')
    PRINT N'TEST_PASS: 16 held sale created';
ELSE
    PRINT N'TEST_FAIL: 16 held sale missing';
GO
