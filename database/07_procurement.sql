
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Suppliers', N'U') IS NULL
CREATE TABLE dbo.Suppliers (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            BIGINT NOT NULL,
    Code                NVARCHAR(50) NOT NULL,
    Name                NVARCHAR(200) NOT NULL,
    CompanyName         NVARCHAR(250) NULL,
    NTN                 NVARCHAR(50) NULL,
    STRN                NVARCHAR(50) NULL,
    DrugLicenseNo       NVARCHAR(100) NULL,
    Phone               NVARCHAR(50) NULL,
    Email               NVARCHAR(200) NULL,
    Address             NVARCHAR(500) NULL,
    CreditLimit         DECIMAL(19,4) NOT NULL CONSTRAINT DF_Suppliers_CreditLimit DEFAULT (0),
    PaymentTermsDays    INT NOT NULL CONSTRAINT DF_Suppliers_PaymentTerms DEFAULT (0),
    IsActive            BIT NOT NULL CONSTRAINT DF_Suppliers_IsActive DEFAULT (1),
    CONSTRAINT PK_Suppliers PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Suppliers_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT UQ_Suppliers_Tenant_Code UNIQUE (TenantId, Code),
    CONSTRAINT CK_Suppliers_CreditLimit CHECK (CreditLimit >= 0)
);
GO

IF OBJECT_ID(N'dbo.SupplierContacts', N'U') IS NULL
CREATE TABLE dbo.SupplierContacts (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    SupplierId      BIGINT NOT NULL,
    Name            NVARCHAR(200) NOT NULL,
    Phone           NVARCHAR(50) NULL,
    Email           NVARCHAR(200) NULL,
    Designation     NVARCHAR(100) NULL,
    IsPrimary       BIT NOT NULL CONSTRAINT DF_SupplierContacts_IsPrimary DEFAULT (0),
    CONSTRAINT PK_SupplierContacts PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SupplierContacts_Suppliers FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id)
);
GO

IF OBJECT_ID(N'dbo.PurchaseOrders', N'U') IS NULL
CREATE TABLE dbo.PurchaseOrders (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    BranchId        BIGINT NOT NULL,
    WarehouseId     BIGINT NOT NULL,
    SupplierId      BIGINT NOT NULL,
    PONumber        NVARCHAR(50) NOT NULL,
    PODate          DATETIME2 NOT NULL,
    ExpectedDate    DATETIME2 NULL,
    Status          NVARCHAR(30) NOT NULL,
    Remarks         NVARCHAR(1000) NULL,
    CreatedBy       BIGINT NULL,
    ApprovedBy      BIGINT NULL,
    ApprovedAt      DATETIME2 NULL,
    CONSTRAINT PK_PurchaseOrders PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_PurchaseOrders_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_PurchaseOrders_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_PurchaseOrders_Suppliers FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT UQ_PurchaseOrders_Branch_PONumber UNIQUE (BranchId, PONumber),
    CONSTRAINT CK_PurchaseOrders_Status CHECK (Status IN (N'Draft', N'Submitted', N'Approved', N'Partial', N'Received', N'Cancelled'))
);
GO

IF OBJECT_ID(N'dbo.PurchaseOrderLines', N'U') IS NULL
CREATE TABLE dbo.PurchaseOrderLines (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    PurchaseOrderId BIGINT NOT NULL,
    ProductId       BIGINT NOT NULL,
    ProductUnitId   BIGINT NOT NULL,
    Quantity        DECIMAL(19,6) NOT NULL,
    FreeQuantity    DECIMAL(19,6) NOT NULL CONSTRAINT DF_POL_FreeQty DEFAULT (0),
    UnitPrice       DECIMAL(19,4) NOT NULL,
    DiscountAmount  DECIMAL(19,4) NOT NULL CONSTRAINT DF_POL_Discount DEFAULT (0),
    TaxAmount       DECIMAL(19,4) NOT NULL CONSTRAINT DF_POL_Tax DEFAULT (0),
    NetAmount       DECIMAL(19,4) NOT NULL,
    CONSTRAINT PK_PurchaseOrderLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_PurchaseOrderLines_PO FOREIGN KEY (PurchaseOrderId) REFERENCES dbo.PurchaseOrders(Id),
    CONSTRAINT FK_PurchaseOrderLines_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_PurchaseOrderLines_ProductUnits FOREIGN KEY (ProductUnitId) REFERENCES dbo.ProductUnits(Id),
    CONSTRAINT CK_POL_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_POL_FreeQuantity CHECK (FreeQuantity >= 0),
    CONSTRAINT CK_POL_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT CK_POL_Discount CHECK (DiscountAmount >= 0),
    CONSTRAINT CK_POL_Tax CHECK (TaxAmount >= 0),
    CONSTRAINT CK_POL_NetAmount CHECK (NetAmount >= 0)
);
GO

IF OBJECT_ID(N'dbo.GoodsReceipts', N'U') IS NULL
CREATE TABLE dbo.GoodsReceipts (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    BranchId            BIGINT NOT NULL,
    WarehouseId         BIGINT NOT NULL,
    SupplierId          BIGINT NOT NULL,
    PurchaseOrderId     BIGINT NULL,
    GRNNumber           NVARCHAR(50) NOT NULL,
    ReceiptDate         DATETIME2 NOT NULL,
    Status              NVARCHAR(30) NOT NULL,
    InvoiceNumber       NVARCHAR(100) NULL,
    InvoiceDate         DATE NULL,
    Subtotal            DECIMAL(19,4) NOT NULL CONSTRAINT DF_GR_Subtotal DEFAULT (0),
    DiscountAmount      DECIMAL(19,4) NOT NULL CONSTRAINT DF_GR_Discount DEFAULT (0),
    TaxAmount           DECIMAL(19,4) NOT NULL CONSTRAINT DF_GR_Tax DEFAULT (0),
    NetAmount           DECIMAL(19,4) NOT NULL CONSTRAINT DF_GR_Net DEFAULT (0),
    ReceivedBy          BIGINT NULL,
    CreatedAt           DATETIME2 NOT NULL CONSTRAINT DF_GR_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_GoodsReceipts PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_GoodsReceipts_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_GoodsReceipts_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_GoodsReceipts_Suppliers FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_GoodsReceipts_PO FOREIGN KEY (PurchaseOrderId) REFERENCES dbo.PurchaseOrders(Id),
    CONSTRAINT UQ_GoodsReceipts_Branch_GRN UNIQUE (BranchId, GRNNumber),
    CONSTRAINT CK_GR_Status CHECK (Status IN (N'Draft', N'Posted', N'Cancelled')),
    CONSTRAINT CK_GR_Amounts CHECK (Subtotal >= 0 AND DiscountAmount >= 0 AND TaxAmount >= 0 AND NetAmount >= 0)
);
GO

IF OBJECT_ID(N'dbo.GoodsReceiptLines', N'U') IS NULL
CREATE TABLE dbo.GoodsReceiptLines (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    GoodsReceiptId      BIGINT NOT NULL,
    ProductId           BIGINT NOT NULL,
    ProductUnitId       BIGINT NOT NULL,
    OrderedQuantity     DECIMAL(19,6) NOT NULL CONSTRAINT DF_GRL_Ordered DEFAULT (0),
    ReceivedQuantity    DECIMAL(19,6) NOT NULL,
    FreeQuantity        DECIMAL(19,6) NOT NULL CONSTRAINT DF_GRL_Free DEFAULT (0),
    UnitCost            DECIMAL(19,4) NOT NULL,
    DiscountAmount      DECIMAL(19,4) NOT NULL CONSTRAINT DF_GRL_Discount DEFAULT (0),
    TaxAmount           DECIMAL(19,4) NOT NULL CONSTRAINT DF_GRL_Tax DEFAULT (0),
    NetCost             DECIMAL(19,4) NOT NULL,
    CONSTRAINT PK_GoodsReceiptLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_GoodsReceiptLines_GR FOREIGN KEY (GoodsReceiptId) REFERENCES dbo.GoodsReceipts(Id),
    CONSTRAINT FK_GoodsReceiptLines_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_GoodsReceiptLines_ProductUnits FOREIGN KEY (ProductUnitId) REFERENCES dbo.ProductUnits(Id),
    CONSTRAINT CK_GRL_ReceivedQuantity CHECK (ReceivedQuantity >= 0),
    CONSTRAINT CK_GRL_FreeQuantity CHECK (FreeQuantity >= 0),
    CONSTRAINT CK_GRL_UnitCost CHECK (UnitCost >= 0),
    CONSTRAINT CK_GRL_NetCost CHECK (NetCost >= 0)
);
GO

IF OBJECT_ID(N'dbo.GoodsReceiptLineBatches', N'U') IS NULL
CREATE TABLE dbo.GoodsReceiptLineBatches (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    GoodsReceiptLineId      BIGINT NOT NULL,
    BatchNumber             NVARCHAR(100) NOT NULL,
    ManufacturingDate       DATE NULL,
    ExpiryDate              DATE NOT NULL,
    MRP                     DECIMAL(19,4) NOT NULL,
    SalePrice               DECIMAL(19,4) NOT NULL,
    Quantity                DECIMAL(19,6) NOT NULL,
    FreeQuantity            DECIMAL(19,6) NOT NULL CONSTRAINT DF_GRLB_Free DEFAULT (0),
    WarehouseLocationId     BIGINT NOT NULL,
    CONSTRAINT PK_GoodsReceiptLineBatches PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_GRLB_GoodsReceiptLines FOREIGN KEY (GoodsReceiptLineId) REFERENCES dbo.GoodsReceiptLines(Id),
    CONSTRAINT FK_GRLB_WarehouseLocations FOREIGN KEY (WarehouseLocationId) REFERENCES dbo.WarehouseLocations(Id),
    CONSTRAINT CK_GRLB_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_GRLB_FreeQuantity CHECK (FreeQuantity >= 0),
    CONSTRAINT CK_GRLB_MRP CHECK (MRP >= 0),
    CONSTRAINT CK_GRLB_SalePrice CHECK (SalePrice >= 0)
);
GO

/* BatchId FK deferred until InventoryBatches exists (18) */
IF OBJECT_ID(N'dbo.SupplierReturns', N'U') IS NULL
CREATE TABLE dbo.SupplierReturns (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    SupplierId      BIGINT NOT NULL,
    BranchId        BIGINT NOT NULL,
    WarehouseId     BIGINT NOT NULL,
    ReturnNumber    NVARCHAR(50) NOT NULL,
    ReturnDate      DATETIME2 NOT NULL,
    Reason          NVARCHAR(500) NULL,
    Status          NVARCHAR(30) NOT NULL,
    TotalAmount     DECIMAL(19,4) NOT NULL CONSTRAINT DF_SR_Total DEFAULT (0),
    CONSTRAINT PK_SupplierReturns PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SupplierReturns_Suppliers FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_SupplierReturns_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_SupplierReturns_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT UQ_SupplierReturns_Branch_Number UNIQUE (BranchId, ReturnNumber),
    CONSTRAINT CK_SR_Status CHECK (Status IN (N'Draft', N'Posted', N'Cancelled')),
    CONSTRAINT CK_SR_TotalAmount CHECK (TotalAmount >= 0)
);
GO

IF OBJECT_ID(N'dbo.SupplierReturnLines', N'U') IS NULL
CREATE TABLE dbo.SupplierReturnLines (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    SupplierReturnId    BIGINT NOT NULL,
    ProductId           BIGINT NOT NULL,
    BatchId             BIGINT NOT NULL, /* FK deferred */
    GoodsReceiptLineId  BIGINT NULL,
    ProductUnitId       BIGINT NOT NULL,
    Quantity            DECIMAL(19,6) NOT NULL,
    UnitCost            DECIMAL(19,4) NOT NULL,
    CONSTRAINT PK_SupplierReturnLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SRL_SupplierReturns FOREIGN KEY (SupplierReturnId) REFERENCES dbo.SupplierReturns(Id),
    CONSTRAINT FK_SRL_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_SRL_GoodsReceiptLines FOREIGN KEY (GoodsReceiptLineId) REFERENCES dbo.GoodsReceiptLines(Id),
    CONSTRAINT FK_SRL_ProductUnits FOREIGN KEY (ProductUnitId) REFERENCES dbo.ProductUnits(Id),
    CONSTRAINT CK_SRL_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_SRL_UnitCost CHECK (UnitCost >= 0)
);
GO

/* PaymentMethodId FK deferred until PaymentMethods exists (18) */
IF OBJECT_ID(N'dbo.SupplierPayments', N'U') IS NULL
CREATE TABLE dbo.SupplierPayments (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    SupplierId          BIGINT NOT NULL,
    BranchId            BIGINT NOT NULL,
    PaymentMethodId     BIGINT NOT NULL, /* FK deferred */
    Amount              DECIMAL(19,4) NOT NULL,
    ReferenceNumber     NVARCHAR(100) NULL,
    PaymentDate         DATETIME2 NOT NULL,
    Remarks             NVARCHAR(500) NULL,
    PaidBy              BIGINT NULL,
    CONSTRAINT PK_SupplierPayments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SupplierPayments_Suppliers FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_SupplierPayments_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT CK_SupplierPayments_Amount CHECK (Amount > 0)
);
GO

IF OBJECT_ID(N'dbo.SupplierLedger', N'U') IS NULL
CREATE TABLE dbo.SupplierLedger (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    SupplierId          BIGINT NOT NULL,
    BranchId            BIGINT NOT NULL,
    TransactionDate     DATETIME2 NOT NULL,
    TransactionType     NVARCHAR(50) NOT NULL,
    ReferenceType       NVARCHAR(100) NULL,
    ReferenceId         BIGINT NULL,
    Debit               DECIMAL(19,4) NOT NULL CONSTRAINT DF_SL_Debit DEFAULT (0),
    Credit              DECIMAL(19,4) NOT NULL CONSTRAINT DF_SL_Credit DEFAULT (0),
    SequenceNo          BIGINT NOT NULL,
    Remarks             NVARCHAR(500) NULL,
    CONSTRAINT PK_SupplierLedger PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SupplierLedger_Suppliers FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_SupplierLedger_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT CK_SupplierLedger_Amounts CHECK (Debit >= 0 AND Credit >= 0),
    CONSTRAINT CK_SupplierLedger_DebitXorCredit CHECK (
        (Debit > 0 AND Credit = 0) OR (Credit > 0 AND Debit = 0) OR (Debit = 0 AND Credit = 0)
    )
);
GO
