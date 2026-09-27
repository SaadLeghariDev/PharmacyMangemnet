USE PharmacyManagement;
GO
SET NOCOUNT ON;

/* TEST 33: Barcode lookup resolves product/unit */
DECLARE @ProdId BIGINT, @PU BIGINT, @BC NVARCHAR(150);
SELECT TOP 1 @ProdId = ProductId, @PU = ProductUnitId, @BC = BarcodeValue
FROM dbo.Barcodes WHERE IsActive = 1;

IF @BC IS NULL
BEGIN
    PRINT N'TEST_FAIL: 33 no barcode seeded from prior tests';
    RETURN;
END

IF EXISTS (
    SELECT 1
    FROM dbo.Barcodes b
    JOIN dbo.Products p ON p.Id = b.ProductId
    JOIN dbo.ProductUnits pu ON pu.Id = b.ProductUnitId
    WHERE b.BarcodeValue = @BC AND p.SKU = N'SKU-PARA-500'
)
    PRINT N'TEST_PASS: 33 barcode lookup resolves product';
ELSE
    PRINT N'TEST_FAIL: 33 barcode lookup failed';
GO

/* TEST 34: Device assignment + barcode print job */
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE Code = N'MAIN');
DECLARE @CounterId BIGINT = (SELECT Id FROM dbo.Counters WHERE Code = N'C1');
DECLARE @TerminalId BIGINT = (SELECT Id FROM dbo.POSTerminals WHERE TerminalCode = N'T1');
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @PrinterType BIGINT = (SELECT Id FROM dbo.DeviceTypes WHERE Code = N'PRINTER');
DECLARE @ProdId BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
DECLARE @BatchId BIGINT = (SELECT TOP 1 Id FROM dbo.InventoryBatches ORDER BY Id);
DECLARE @DeviceId BIGINT, @TplId BIGINT, @JobId BIGINT;

INSERT INTO dbo.Devices (BranchId, CounterId, DeviceTypeId, Name, Manufacturer, Model, ConnectionType, IsDefault, IsActive)
VALUES (@BranchId, @CounterId, @PrinterType, N'Zebra GC420t', N'Zebra', N'GC420t', N'USB', 1, 1);
SET @DeviceId = SCOPE_IDENTITY();
INSERT INTO dbo.DeviceAssignments (DeviceId, TerminalId, AssignedFrom, IsActive)
VALUES (@DeviceId, @TerminalId, SYSUTCDATETIME(), 1);
INSERT INTO dbo.PrintTemplates (TenantId, TemplateType, Name, TemplateContent, PaperWidth, IsDefault, IsActive)
VALUES (@TenantId, N'Barcode', N'Standard Label', N'^XA^FO50,50^BY3^BCN,100^FD{{BARCODE}}^FS^XZ', 50, 1, 1);
SET @TplId = SCOPE_IDENTITY();
INSERT INTO dbo.BarcodePrintJobs (BranchId, PrinterDeviceId, ProductId, BatchId, Quantity, TemplateId, Status, CreatedBy)
VALUES (@BranchId, @DeviceId, @ProdId, @BatchId, 10, @TplId, N'Queued', 1);
SET @JobId = SCOPE_IDENTITY();
UPDATE dbo.BarcodePrintJobs SET Status = N'Printed', PrintedAt = SYSUTCDATETIME() WHERE Id = @JobId;

IF EXISTS (
    SELECT 1 FROM dbo.BarcodePrintJobs j
    JOIN dbo.DeviceAssignments da ON da.DeviceId = j.PrinterDeviceId
    WHERE j.Id = @JobId AND j.Status = N'Printed' AND da.TerminalId = @TerminalId
)
    PRINT N'TEST_PASS: 34 device assignment and barcode print job';
ELSE
    PRINT N'TEST_FAIL: 34 hardware print job failed';
GO

/* TEST 35: AuditLogs soft FK — insert without referential enforcement */
INSERT INTO dbo.AuditLogs (TenantId, BranchId, UserId, EntityName, EntityId, Action, OldValues, NewValues, IPAddress, TerminalId)
VALUES (999999, 888888, 777777, N'Sales', 123456, N'Update', N'{"Status":"Draft"}', N'{"Status":"Completed"}', N'127.0.0.1', 666666);

IF EXISTS (SELECT 1 FROM dbo.AuditLogs WHERE EntityName = N'Sales' AND EntityId = 123456 AND UserId = 777777)
    PRINT N'TEST_PASS: 35 audit log soft FK insert allowed';
ELSE
    PRINT N'TEST_FAIL: 35 audit log insert failed';

INSERT INTO dbo.ApprovalRequests (BranchId, RequestType, ReferenceType, ReferenceId, RequestedBy, Status, Reason, RequestedAt)
VALUES (888888, N'StockAdjustment', N'StockAdjustment', 1, 777777, N'Pending', N'Test soft FK', SYSUTCDATETIME());
GO
