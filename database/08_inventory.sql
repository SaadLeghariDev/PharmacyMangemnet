
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.InventoryBatches', N'U') IS NULL
CREATE TABLE dbo.InventoryBatches (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    ProductId               BIGINT NOT NULL,
    GoodsReceiptLineId      BIGINT NOT NULL, /* PDF: NOT NULL — opening stock needs GRN path */
    SupplierId              BIGINT NOT NULL,
    WarehouseId             BIGINT NOT NULL,
    BatchNumber             NVARCHAR(100) NOT NULL,
    ManufacturingDate       DATE NULL,
    ExpiryDate              DATE NOT NULL,
    QuantityReceived        DECIMAL(19,6) NOT NULL,
    FreeQuantity            DECIMAL(19,6) NOT NULL CONSTRAINT DF_IB_Free DEFAULT (0),
    PurchaseCost            DECIMAL(19,4) NOT NULL,
    MRP                     DECIMAL(19,4) NOT NULL,
    SalePrice               DECIMAL(19,4) NOT NULL,
    BatchStatus             NVARCHAR(30) NOT NULL,
    IsRecalled              BIT NOT NULL CONSTRAINT DF_IB_IsRecalled DEFAULT (0),
    CreatedAt               DATETIME2 NOT NULL CONSTRAINT DF_IB_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt               DATETIME2 NOT NULL CONSTRAINT DF_IB_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    RowVersion              ROWVERSION NOT NULL,
    CONSTRAINT PK_InventoryBatches PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_InventoryBatches_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_InventoryBatches_GRL FOREIGN KEY (GoodsReceiptLineId) REFERENCES dbo.GoodsReceiptLines(Id),
    CONSTRAINT FK_InventoryBatches_Suppliers FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_InventoryBatches_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT CK_IB_QuantityReceived CHECK (QuantityReceived >= 0),
    CONSTRAINT CK_IB_FreeQuantity CHECK (FreeQuantity >= 0),
    CONSTRAINT CK_IB_PurchaseCost CHECK (PurchaseCost >= 0),
    CONSTRAINT CK_IB_MRP CHECK (MRP >= 0),
    CONSTRAINT CK_IB_SalePrice CHECK (SalePrice >= 0),
    CONSTRAINT CK_IB_BatchStatus CHECK (BatchStatus IN (
        N'Available', N'Blocked', N'Quarantine', N'Recalled', N'Expired', N'Depleted'
    ))
);
GO

IF OBJECT_ID(N'dbo.InventoryBatchLocations', N'U') IS NULL
CREATE TABLE dbo.InventoryBatchLocations (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    BatchId                 BIGINT NOT NULL,
    WarehouseLocationId     BIGINT NOT NULL,
    QuantityOnHand          DECIMAL(19,6) NOT NULL,
    ReservedQuantity        DECIMAL(19,6) NOT NULL CONSTRAINT DF_IBL_Reserved DEFAULT (0),
    UpdatedAt               DATETIME2 NOT NULL CONSTRAINT DF_IBL_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    RowVersion              ROWVERSION NOT NULL,
    CONSTRAINT PK_InventoryBatchLocations PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_IBL_Batches FOREIGN KEY (BatchId) REFERENCES dbo.InventoryBatches(Id),
    CONSTRAINT FK_IBL_Locations FOREIGN KEY (WarehouseLocationId) REFERENCES dbo.WarehouseLocations(Id),
    CONSTRAINT UQ_IBL_Batch_Location UNIQUE (BatchId, WarehouseLocationId),
    CONSTRAINT CK_IBL_QoH CHECK (QuantityOnHand >= 0),
    CONSTRAINT CK_IBL_Reserved CHECK (ReservedQuantity >= 0),
    CONSTRAINT CK_IBL_Available CHECK (QuantityOnHand >= ReservedQuantity)
);
GO

IF OBJECT_ID(N'dbo.InventoryMovements', N'U') IS NULL
CREATE TABLE dbo.InventoryMovements (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    BranchId                BIGINT NOT NULL,
    WarehouseId             BIGINT NOT NULL,
    WarehouseLocationId     BIGINT NOT NULL,
    ProductId               BIGINT NOT NULL,
    BatchId                 BIGINT NOT NULL,
    ProductUnitId           BIGINT NOT NULL,
    MovementType            NVARCHAR(50) NOT NULL,
    ReferenceType           NVARCHAR(100) NULL,
    ReferenceId             BIGINT NULL,
    Quantity                DECIMAL(19,6) NOT NULL,
    UnitCost                DECIMAL(19,4) NOT NULL,
    TotalCost               DECIMAL(19,4) NOT NULL,
    BalanceBefore           DECIMAL(19,6) NOT NULL,
    BalanceAfter            DECIMAL(19,6) NOT NULL,
    MovementDate            DATETIME2 NOT NULL,
    PerformedBy             BIGINT NULL,
    IdempotencyKey          NVARCHAR(150) NULL,
    CreatedAt               DATETIME2 NOT NULL CONSTRAINT DF_IM_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_InventoryMovements PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_IM_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_IM_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_IM_Locations FOREIGN KEY (WarehouseLocationId) REFERENCES dbo.WarehouseLocations(Id),
    CONSTRAINT FK_IM_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_IM_Batches FOREIGN KEY (BatchId) REFERENCES dbo.InventoryBatches(Id),
    CONSTRAINT FK_IM_ProductUnits FOREIGN KEY (ProductUnitId) REFERENCES dbo.ProductUnits(Id),
    CONSTRAINT CK_IM_UnitCost CHECK (UnitCost >= 0),
    CONSTRAINT CK_IM_TotalCost CHECK (TotalCost >= 0)
);
GO

IF OBJECT_ID(N'dbo.StockTransfers', N'U') IS NULL
CREATE TABLE dbo.StockTransfers (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    TransferNumber      NVARCHAR(50) NOT NULL,
    FromWarehouseId     BIGINT NOT NULL,
    ToWarehouseId       BIGINT NOT NULL,
    TransferDate        DATETIME2 NOT NULL,
    Status              NVARCHAR(30) NOT NULL,
    RequestedBy         BIGINT NULL,
    ApprovedBy          BIGINT NULL,
    ReceivedBy          BIGINT NULL,
    CONSTRAINT PK_StockTransfers PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_ST_FromWarehouse FOREIGN KEY (FromWarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_ST_ToWarehouse FOREIGN KEY (ToWarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT UQ_StockTransfers_Number UNIQUE (TransferNumber),
    CONSTRAINT CK_ST_Status CHECK (Status IN (N'Draft', N'InTransit', N'Received', N'Cancelled')),
    CONSTRAINT CK_ST_DifferentWarehouses CHECK (FromWarehouseId <> ToWarehouseId)
);
GO

IF OBJECT_ID(N'dbo.StockTransferLines', N'U') IS NULL
CREATE TABLE dbo.StockTransferLines (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    StockTransferId     BIGINT NOT NULL,
    ProductId           BIGINT NOT NULL,
    BatchId             BIGINT NOT NULL,
    ProductUnitId       BIGINT NOT NULL,
    FromLocationId      BIGINT NOT NULL,
    ToLocationId        BIGINT NOT NULL,
    Quantity            DECIMAL(19,6) NOT NULL,
    ReceivedQuantity    DECIMAL(19,6) NOT NULL CONSTRAINT DF_STL_Received DEFAULT (0),
    CONSTRAINT PK_StockTransferLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_STL_Transfers FOREIGN KEY (StockTransferId) REFERENCES dbo.StockTransfers(Id),
    CONSTRAINT FK_STL_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_STL_Batches FOREIGN KEY (BatchId) REFERENCES dbo.InventoryBatches(Id),
    CONSTRAINT FK_STL_ProductUnits FOREIGN KEY (ProductUnitId) REFERENCES dbo.ProductUnits(Id),
    CONSTRAINT FK_STL_FromLocation FOREIGN KEY (FromLocationId) REFERENCES dbo.WarehouseLocations(Id),
    CONSTRAINT FK_STL_ToLocation FOREIGN KEY (ToLocationId) REFERENCES dbo.WarehouseLocations(Id),
    CONSTRAINT CK_STL_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_STL_ReceivedQuantity CHECK (ReceivedQuantity >= 0)
);
GO

/* ReasonCodeId FK deferred to 18 (ReasonCodes in common) */
IF OBJECT_ID(N'dbo.StockCounts', N'U') IS NULL
CREATE TABLE dbo.StockCounts (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    BranchId        BIGINT NOT NULL,
    WarehouseId     BIGINT NOT NULL,
    CountNumber     NVARCHAR(50) NOT NULL,
    CountDate       DATETIME2 NOT NULL,
    Status          NVARCHAR(30) NOT NULL,
    CountType       NVARCHAR(50) NULL,
    CreatedBy       BIGINT NULL,
    ApprovedBy      BIGINT NULL,
    CONSTRAINT PK_StockCounts PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_StockCounts_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_StockCounts_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT UQ_StockCounts_Branch_Number UNIQUE (BranchId, CountNumber),
    CONSTRAINT CK_StockCounts_Status CHECK (Status IN (N'Draft', N'InProgress', N'Completed', N'Approved', N'Cancelled'))
);
GO

IF OBJECT_ID(N'dbo.StockCountLines', N'U') IS NULL
CREATE TABLE dbo.StockCountLines (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    StockCountId            BIGINT NOT NULL,
    ProductId               BIGINT NOT NULL,
    BatchId                 BIGINT NOT NULL,
    WarehouseLocationId     BIGINT NOT NULL,
    SystemQuantity          DECIMAL(19,6) NOT NULL,
    CountedQuantity         DECIMAL(19,6) NOT NULL,
    VarianceQuantity        DECIMAL(19,6) NOT NULL,
    ReasonCodeId            BIGINT NOT NULL, /* FK deferred */
    CONSTRAINT PK_StockCountLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SCL_StockCounts FOREIGN KEY (StockCountId) REFERENCES dbo.StockCounts(Id),
    CONSTRAINT FK_SCL_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_SCL_Batches FOREIGN KEY (BatchId) REFERENCES dbo.InventoryBatches(Id),
    CONSTRAINT FK_SCL_Locations FOREIGN KEY (WarehouseLocationId) REFERENCES dbo.WarehouseLocations(Id),
    CONSTRAINT CK_SCL_SystemQty CHECK (SystemQuantity >= 0),
    CONSTRAINT CK_SCL_CountedQty CHECK (CountedQuantity >= 0)
);
GO

IF OBJECT_ID(N'dbo.StockAdjustments', N'U') IS NULL
CREATE TABLE dbo.StockAdjustments (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    BranchId            BIGINT NOT NULL,
    WarehouseId         BIGINT NOT NULL,
    AdjustmentNumber    NVARCHAR(50) NOT NULL,
    AdjustmentDate      DATETIME2 NOT NULL,
    AdjustmentType      NVARCHAR(50) NOT NULL,
    Status              NVARCHAR(30) NOT NULL,
    ReasonCodeId        BIGINT NOT NULL, /* FK deferred */
    CreatedBy           BIGINT NULL,
    ApprovedBy          BIGINT NULL,
    CONSTRAINT PK_StockAdjustments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_StockAdjustments_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_StockAdjustments_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT UQ_StockAdjustments_Branch_Number UNIQUE (BranchId, AdjustmentNumber),
    CONSTRAINT CK_SA_Status CHECK (Status IN (N'Draft', N'PendingApproval', N'Approved', N'Posted', N'Rejected', N'Cancelled')),
    CONSTRAINT CK_SA_Type CHECK (AdjustmentType IN (N'Increase', N'Decrease', N'WriteOff', N'Damage', N'Expiry'))
);
GO

IF OBJECT_ID(N'dbo.StockAdjustmentLines', N'U') IS NULL
CREATE TABLE dbo.StockAdjustmentLines (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    StockAdjustmentId       BIGINT NOT NULL,
    ProductId               BIGINT NOT NULL,
    BatchId                 BIGINT NOT NULL,
    WarehouseLocationId     BIGINT NOT NULL,
    Quantity                DECIMAL(19,6) NOT NULL,
    UnitCost                DECIMAL(19,4) NOT NULL,
    CONSTRAINT PK_StockAdjustmentLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SAL_Adjustments FOREIGN KEY (StockAdjustmentId) REFERENCES dbo.StockAdjustments(Id),
    CONSTRAINT FK_SAL_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_SAL_Batches FOREIGN KEY (BatchId) REFERENCES dbo.InventoryBatches(Id),
    CONSTRAINT FK_SAL_Locations FOREIGN KEY (WarehouseLocationId) REFERENCES dbo.WarehouseLocations(Id),
    CONSTRAINT CK_SAL_Quantity CHECK (Quantity <> 0),
    CONSTRAINT CK_SAL_UnitCost CHECK (UnitCost >= 0)
);
GO

IF OBJECT_ID(N'dbo.ReorderRules', N'U') IS NULL
CREATE TABLE dbo.ReorderRules (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    BranchId                BIGINT NOT NULL,
    WarehouseId             BIGINT NOT NULL,
    ProductId               BIGINT NOT NULL,
    MinimumStock            DECIMAL(19,6) NOT NULL,
    MaximumStock            DECIMAL(19,6) NOT NULL,
    ReorderPoint            DECIMAL(19,6) NOT NULL,
    ReorderQuantity         DECIMAL(19,6) NOT NULL,
    PreferredSupplierId     BIGINT NOT NULL,
    IsActive                BIT NOT NULL CONSTRAINT DF_ReorderRules_IsActive DEFAULT (1),
    CONSTRAINT PK_ReorderRules PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_ReorderRules_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_ReorderRules_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_ReorderRules_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_ReorderRules_Suppliers FOREIGN KEY (PreferredSupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT CK_RR_Min CHECK (MinimumStock >= 0),
    CONSTRAINT CK_RR_Max CHECK (MaximumStock >= MinimumStock),
    CONSTRAINT CK_RR_ReorderPoint CHECK (ReorderPoint >= 0),
    CONSTRAINT CK_RR_ReorderQty CHECK (ReorderQuantity > 0)
);
GO
