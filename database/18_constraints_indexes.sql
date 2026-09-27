
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* Deferred FKs that break circular / ordering dependencies */

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_InvoiceTaxes_Sales')
    ALTER TABLE dbo.InvoiceTaxes ADD CONSTRAINT FK_InvoiceTaxes_Sales
        FOREIGN KEY (SaleId) REFERENCES dbo.Sales(Id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_InvoiceTaxes_SaleLines')
    ALTER TABLE dbo.InvoiceTaxes ADD CONSTRAINT FK_InvoiceTaxes_SaleLines
        FOREIGN KEY (SaleLineId) REFERENCES dbo.SaleLines(Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sales_Customers')
    ALTER TABLE dbo.Sales ADD CONSTRAINT FK_Sales_Customers
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_HeldSales_Customers')
    ALTER TABLE dbo.HeldSales ADD CONSTRAINT FK_HeldSales_Customers
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SaleLines_PrescriptionItems')
    ALTER TABLE dbo.SaleLines ADD CONSTRAINT FK_SaleLines_PrescriptionItems
        FOREIGN KEY (PrescriptionItemId) REFERENCES dbo.PrescriptionItems(Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Prescriptions_Attachments')
    ALTER TABLE dbo.Prescriptions ADD CONSTRAINT FK_Prescriptions_Attachments
        FOREIGN KEY (PrescriptionAttachmentId) REFERENCES dbo.Attachments(Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SRL_Batches')
    ALTER TABLE dbo.SupplierReturnLines ADD CONSTRAINT FK_SRL_Batches
        FOREIGN KEY (BatchId) REFERENCES dbo.InventoryBatches(Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SupplierPayments_Methods')
    ALTER TABLE dbo.SupplierPayments ADD CONSTRAINT FK_SupplierPayments_Methods
        FOREIGN KEY (PaymentMethodId) REFERENCES dbo.PaymentMethods(Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SCL_ReasonCodes')
    ALTER TABLE dbo.StockCountLines ADD CONSTRAINT FK_SCL_ReasonCodes
        FOREIGN KEY (ReasonCodeId) REFERENCES dbo.ReasonCodes(Id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SA_ReasonCodes')
    ALTER TABLE dbo.StockAdjustments ADD CONSTRAINT FK_SA_ReasonCodes
        FOREIGN KEY (ReasonCodeId) REFERENCES dbo.ReasonCodes(Id);
GO

/* Indexes — FK joins, FEFO, stock location, sales by branch/date, barcodes, filtered IsActive */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Branches_TenantId' AND object_id = OBJECT_ID(N'dbo.Branches'))
    CREATE NONCLUSTERED INDEX IX_Branches_TenantId ON dbo.Branches(TenantId) WHERE IsActive = 1;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Warehouses_BranchId' AND object_id = OBJECT_ID(N'dbo.Warehouses'))
    CREATE NONCLUSTERED INDEX IX_Warehouses_BranchId ON dbo.Warehouses(BranchId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_WarehouseLocations_WarehouseId' AND object_id = OBJECT_ID(N'dbo.WarehouseLocations'))
    CREATE NONCLUSTERED INDEX IX_WarehouseLocations_WarehouseId ON dbo.WarehouseLocations(WarehouseId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_POSTerminals_BranchId' AND object_id = OBJECT_ID(N'dbo.POSTerminals'))
    CREATE NONCLUSTERED INDEX IX_POSTerminals_BranchId ON dbo.POSTerminals(BranchId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_TenantId' AND object_id = OBJECT_ID(N'dbo.Users'))
    CREATE NONCLUSTERED INDEX IX_Users_TenantId ON dbo.Users(TenantId) WHERE IsActive = 1;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Products_TenantId' AND object_id = OBJECT_ID(N'dbo.Products'))
    CREATE NONCLUSTERED INDEX IX_Products_TenantId ON dbo.Products(TenantId) WHERE IsActive = 1;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Products_CategoryId' AND object_id = OBJECT_ID(N'dbo.Products'))
    CREATE NONCLUSTERED INDEX IX_Products_CategoryId ON dbo.Products(CategoryId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Barcodes_BarcodeValue' AND object_id = OBJECT_ID(N'dbo.Barcodes'))
    CREATE NONCLUSTERED INDEX IX_Barcodes_BarcodeValue ON dbo.Barcodes(BarcodeValue) WHERE IsActive = 1;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Barcodes_ProductId' AND object_id = OBJECT_ID(N'dbo.Barcodes'))
    CREATE NONCLUSTERED INDEX IX_Barcodes_ProductId ON dbo.Barcodes(ProductId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InventoryBatches_FEFO' AND object_id = OBJECT_ID(N'dbo.InventoryBatches'))
    CREATE NONCLUSTERED INDEX IX_InventoryBatches_FEFO
        ON dbo.InventoryBatches(ProductId, ExpiryDate, BatchStatus)
        INCLUDE (IsRecalled, WarehouseId, BatchNumber);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InventoryBatchLocations_Location_Batch' AND object_id = OBJECT_ID(N'dbo.InventoryBatchLocations'))
    CREATE NONCLUSTERED INDEX IX_InventoryBatchLocations_Location_Batch
        ON dbo.InventoryBatchLocations(WarehouseLocationId, BatchId)
        INCLUDE (QuantityOnHand, ReservedQuantity);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InventoryMovements_BatchId' AND object_id = OBJECT_ID(N'dbo.InventoryMovements'))
    CREATE NONCLUSTERED INDEX IX_InventoryMovements_BatchId ON dbo.InventoryMovements(BatchId, MovementDate);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InventoryMovements_Idempotency' AND object_id = OBJECT_ID(N'dbo.InventoryMovements'))
    CREATE UNIQUE NONCLUSTERED INDEX IX_InventoryMovements_Idempotency
        ON dbo.InventoryMovements(IdempotencyKey) WHERE IdempotencyKey IS NOT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Sales_Branch_SaleDate' AND object_id = OBJECT_ID(N'dbo.Sales'))
    CREATE NONCLUSTERED INDEX IX_Sales_Branch_SaleDate ON dbo.Sales(BranchId, SaleDate)
        INCLUDE (Status, NetAmount, PaymentStatus);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SaleLines_SaleId' AND object_id = OBJECT_ID(N'dbo.SaleLines'))
    CREATE NONCLUSTERED INDEX IX_SaleLines_SaleId ON dbo.SaleLines(SaleId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SaleLineBatches_SaleLineId' AND object_id = OBJECT_ID(N'dbo.SaleLineBatches'))
    CREATE NONCLUSTERED INDEX IX_SaleLineBatches_SaleLineId ON dbo.SaleLineBatches(SaleLineId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseOrders_SupplierId' AND object_id = OBJECT_ID(N'dbo.PurchaseOrders'))
    CREATE NONCLUSTERED INDEX IX_PurchaseOrders_SupplierId ON dbo.PurchaseOrders(SupplierId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GoodsReceipts_SupplierId' AND object_id = OBJECT_ID(N'dbo.GoodsReceipts'))
    CREATE NONCLUSTERED INDEX IX_GoodsReceipts_SupplierId ON dbo.GoodsReceipts(SupplierId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Customers_TenantId' AND object_id = OBJECT_ID(N'dbo.Customers'))
    CREATE NONCLUSTERED INDEX IX_Customers_TenantId ON dbo.Customers(TenantId) WHERE IsActive = 1;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerLedger_Customer' AND object_id = OBJECT_ID(N'dbo.CustomerLedger'))
    CREATE NONCLUSTERED INDEX IX_CustomerLedger_Customer ON dbo.CustomerLedger(CustomerId, SequenceNo);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SupplierLedger_Supplier' AND object_id = OBJECT_ID(N'dbo.SupplierLedger'))
    CREATE NONCLUSTERED INDEX IX_SupplierLedger_Supplier ON dbo.SupplierLedger(SupplierId, SequenceNo);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_FiscalDocuments_SaleId' AND object_id = OBJECT_ID(N'dbo.FiscalDocuments'))
    CREATE NONCLUSTERED INDEX IX_FiscalDocuments_SaleId ON dbo.FiscalDocuments(SaleId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_Entity' AND object_id = OBJECT_ID(N'dbo.AuditLogs'))
    CREATE NONCLUSTERED INDEX IX_AuditLogs_Entity ON dbo.AuditLogs(EntityName, EntityId, CreatedAt);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ControlledDrugTransactions_Register' AND object_id = OBJECT_ID(N'dbo.ControlledDrugTransactions'))
    CREATE NONCLUSTERED INDEX IX_ControlledDrugTransactions_Register
        ON dbo.ControlledDrugTransactions(RegisterId, TransactionDate);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_NumberSequences_Lookup' AND object_id = OBJECT_ID(N'dbo.NumberSequences'))
    CREATE UNIQUE NONCLUSTERED INDEX IX_NumberSequences_Lookup
        ON dbo.NumberSequences(TenantId, DocumentType, BranchId, TerminalId);
GO

PRINT N'Deferred FKs and indexes applied.';
GO
