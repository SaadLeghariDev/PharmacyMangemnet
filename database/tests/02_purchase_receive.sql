USE PharmacyManagement;
GO
SET NOCOUNT ON;

/* Ensure product from suite 01 exists; recreate minimal if wiped mid-run */
IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE SKU = N'SKU-PARA-500')
BEGIN
    PRINT N'TEST_FAIL: prerequisite product SKU-PARA-500 missing (run 01 first)';
    RETURN;
END
GO

/* TEST 8: Purchase order creation via NumberSequences (no MAX+1) */
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE TenantId = @TenantId AND Code = N'MAIN');
DECLARE @WhId BIGINT = (SELECT Id FROM dbo.Warehouses WHERE BranchId = @BranchId AND Code = N'MAIN-WH');
DECLARE @ProdId BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
DECLARE @PU_Box BIGINT = (SELECT TOP 1 Id FROM dbo.ProductUnits WHERE ProductId = @ProdId AND IsPurchaseUnit = 1);
DECLARE @SupplierId BIGINT, @POId BIGINT, @NextNum BIGINT, @PONumber NVARCHAR(50);

IF NOT EXISTS (SELECT 1 FROM dbo.Suppliers WHERE TenantId = @TenantId AND Code = N'SUP01')
    INSERT INTO dbo.Suppliers (TenantId, Code, Name, CompanyName, CreditLimit, PaymentTermsDays, IsActive)
    VALUES (@TenantId, N'SUP01', N'Demo Supplier', N'Demo Supplier Co', 100000, 30, 1);
SET @SupplierId = (SELECT Id FROM dbo.Suppliers WHERE TenantId = @TenantId AND Code = N'SUP01');

UPDATE dbo.NumberSequences
SET @NextNum = CurrentNumber = CurrentNumber + 1
OUTPUT inserted.CurrentNumber
WHERE TenantId = @TenantId AND DocumentType = N'PO' AND BranchId = @BranchId AND TerminalId IS NULL;

SELECT @NextNum = CurrentNumber FROM dbo.NumberSequences
WHERE TenantId = @TenantId AND DocumentType = N'PO' AND BranchId = @BranchId AND TerminalId IS NULL;

SET @PONumber = N'PO-' + RIGHT(N'000000' + CAST(@NextNum AS NVARCHAR(10)), 6);

INSERT INTO dbo.PurchaseOrders (BranchId, WarehouseId, SupplierId, PONumber, PODate, ExpectedDate, Status, CreatedBy)
VALUES (@BranchId, @WhId, @SupplierId, @PONumber, SYSUTCDATETIME(), DATEADD(DAY, 7, SYSUTCDATETIME()), N'Approved', 1);
SET @POId = SCOPE_IDENTITY();

INSERT INTO dbo.PurchaseOrderLines (PurchaseOrderId, ProductId, ProductUnitId, Quantity, FreeQuantity, UnitPrice, DiscountAmount, TaxAmount, NetAmount)
VALUES (@POId, @ProdId, @PU_Box, 5, 0, 200.0000, 0, 0, 1000.0000);

IF @POId IS NOT NULL AND @PONumber LIKE N'PO-%'
    PRINT N'TEST_PASS: 08 purchase order via NumberSequences';
ELSE
    PRINT N'TEST_FAIL: 08 purchase order creation failed';
GO

/* TEST 9: Goods receipt with line batches creates inventory batches + locations */
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE TenantId = @TenantId AND Code = N'MAIN');
DECLARE @WhId BIGINT = (SELECT Id FROM dbo.Warehouses WHERE BranchId = @BranchId AND Code = N'MAIN-WH');
DECLARE @LocId BIGINT = (SELECT Id FROM dbo.WarehouseLocations WHERE WarehouseId = @WhId AND Code = N'A-01');
DECLARE @SupplierId BIGINT = (SELECT Id FROM dbo.Suppliers WHERE Code = N'SUP01');
DECLARE @POId BIGINT = (SELECT TOP 1 Id FROM dbo.PurchaseOrders WHERE BranchId = @BranchId ORDER BY Id DESC);
DECLARE @ProdId BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
DECLARE @PU_Box BIGINT = (SELECT TOP 1 Id FROM dbo.ProductUnits WHERE ProductId = @ProdId AND IsPurchaseUnit = 1);
DECLARE @PU_Tab BIGINT = (SELECT TOP 1 Id FROM dbo.ProductUnits WHERE ProductId = @ProdId AND IsBaseUnit = 1);
DECLARE @UserId BIGINT = (SELECT Id FROM dbo.Users WHERE Username = N'admin');
DECLARE @NextNum BIGINT, @GRN NVARCHAR(50), @GRId BIGINT, @GRLId BIGINT, @BatchEarly BIGINT, @BatchLate BIGINT;

UPDATE dbo.NumberSequences SET CurrentNumber = CurrentNumber + 1
WHERE TenantId = @TenantId AND DocumentType = N'GRN' AND BranchId = @BranchId AND TerminalId IS NULL;
SELECT @NextNum = CurrentNumber FROM dbo.NumberSequences
WHERE TenantId = @TenantId AND DocumentType = N'GRN' AND BranchId = @BranchId AND TerminalId IS NULL;
SET @GRN = N'GRN-' + RIGHT(N'000000' + CAST(@NextNum AS NVARCHAR(10)), 6);

BEGIN TRAN;
INSERT INTO dbo.GoodsReceipts (BranchId, WarehouseId, SupplierId, PurchaseOrderId, GRNNumber, ReceiptDate, Status,
    Subtotal, DiscountAmount, TaxAmount, NetAmount, ReceivedBy)
VALUES (@BranchId, @WhId, @SupplierId, @POId, @GRN, SYSUTCDATETIME(), N'Posted', 1000, 0, 0, 1000, @UserId);
SET @GRId = SCOPE_IDENTITY();

INSERT INTO dbo.GoodsReceiptLines (GoodsReceiptId, ProductId, ProductUnitId, OrderedQuantity, ReceivedQuantity, FreeQuantity, UnitCost, DiscountAmount, TaxAmount, NetCost)
VALUES (@GRId, @ProdId, @PU_Box, 5, 5, 0, 200, 0, 0, 1000);
SET @GRLId = SCOPE_IDENTITY();

/* Two batches: early expiry and late expiry (base units: 5 boxes * 100 = 500 tablets, split 200+300) */
INSERT INTO dbo.GoodsReceiptLineBatches (GoodsReceiptLineId, BatchNumber, ManufacturingDate, ExpiryDate, MRP, SalePrice, Quantity, FreeQuantity, WarehouseLocationId)
VALUES (@GRLId, N'BATCH-EARLY', '2025-01-01', '2026-12-15', 6, 5, 200, 0, @LocId);
INSERT INTO dbo.GoodsReceiptLineBatches (GoodsReceiptLineId, BatchNumber, ManufacturingDate, ExpiryDate, MRP, SalePrice, Quantity, FreeQuantity, WarehouseLocationId)
VALUES (@GRLId, N'BATCH-LATE', '2025-06-01', '2028-06-30', 6, 5, 300, 0, @LocId);

INSERT INTO dbo.InventoryBatches (ProductId, GoodsReceiptLineId, SupplierId, WarehouseId, BatchNumber, ManufacturingDate, ExpiryDate,
    QuantityReceived, FreeQuantity, PurchaseCost, MRP, SalePrice, BatchStatus)
VALUES (@ProdId, @GRLId, @SupplierId, @WhId, N'BATCH-EARLY', '2025-01-01', '2026-12-15', 200, 0, 2.00, 6, 5, N'Available');
SET @BatchEarly = SCOPE_IDENTITY();
INSERT INTO dbo.InventoryBatches (ProductId, GoodsReceiptLineId, SupplierId, WarehouseId, BatchNumber, ManufacturingDate, ExpiryDate,
    QuantityReceived, FreeQuantity, PurchaseCost, MRP, SalePrice, BatchStatus)
VALUES (@ProdId, @GRLId, @SupplierId, @WhId, N'BATCH-LATE', '2025-06-01', '2028-06-30', 300, 0, 2.00, 6, 5, N'Available');
SET @BatchLate = SCOPE_IDENTITY();

INSERT INTO dbo.InventoryBatchLocations (BatchId, WarehouseLocationId, QuantityOnHand, ReservedQuantity)
VALUES (@BatchEarly, @LocId, 200, 0), (@BatchLate, @LocId, 300, 0);

INSERT INTO dbo.InventoryMovements (BranchId, WarehouseId, WarehouseLocationId, ProductId, BatchId, ProductUnitId,
    MovementType, ReferenceType, ReferenceId, Quantity, UnitCost, TotalCost, BalanceBefore, BalanceAfter, MovementDate, PerformedBy)
VALUES
 (@BranchId, @WhId, @LocId, @ProdId, @BatchEarly, @PU_Tab, N'Receipt', N'GoodsReceipt', @GRId, 200, 2, 400, 0, 200, SYSUTCDATETIME(), @UserId),
 (@BranchId, @WhId, @LocId, @ProdId, @BatchLate, @PU_Tab, N'Receipt', N'GoodsReceipt', @GRId, 300, 2, 600, 0, 300, SYSUTCDATETIME(), @UserId);

INSERT INTO dbo.SupplierLedger (SupplierId, BranchId, TransactionDate, TransactionType, ReferenceType, ReferenceId, Debit, Credit, SequenceNo, Remarks)
VALUES (@SupplierId, @BranchId, SYSUTCDATETIME(), N'Purchase', N'GoodsReceipt', @GRId, 1000, 0, 1, N'GRN posted');
COMMIT;

IF (SELECT COUNT(*) FROM dbo.InventoryBatches WHERE ProductId = @ProdId) = 2
   AND (SELECT SUM(QuantityOnHand) FROM dbo.InventoryBatchLocations ibl
        JOIN dbo.InventoryBatches b ON b.Id = ibl.BatchId WHERE b.ProductId = @ProdId) = 500
    PRINT N'TEST_PASS: 09 goods receipt created batches and locations';
ELSE
    PRINT N'TEST_FAIL: 09 goods receipt batch/location mismatch';
GO

/* TEST 10: Inventory movements ledger balances match location stock */
DECLARE @ProdId BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
DECLARE @LocSum DECIMAL(19,6) = (
    SELECT SUM(ibl.QuantityOnHand) FROM dbo.InventoryBatchLocations ibl
    JOIN dbo.InventoryBatches b ON b.Id = ibl.BatchId WHERE b.ProductId = @ProdId);
DECLARE @MovSum DECIMAL(19,6) = (
    SELECT SUM(CASE WHEN MovementType = N'Receipt' THEN Quantity ELSE -Quantity END)
    FROM dbo.InventoryMovements WHERE ProductId = @ProdId);

IF @LocSum = 500 AND @MovSum = 500
    PRINT N'TEST_PASS: 10 inventory movement ledger matches stock';
ELSE
BEGIN
    DECLARE @LocTxt NVARCHAR(40) = CAST(@LocSum AS NVARCHAR(40));
    DECLARE @MovTxt NVARCHAR(40) = CAST(@MovSum AS NVARCHAR(40));
    RAISERROR(N'TEST_FAIL: 10 ledger/stock mismatch loc=%s mov=%s', 16, 1, @LocTxt, @MovTxt);
END
GO
