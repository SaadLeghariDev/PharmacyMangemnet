
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* CustomerId FK deferred until Customers exists (18) */
IF OBJECT_ID(N'dbo.Sales', N'U') IS NULL
CREATE TABLE dbo.Sales (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    BranchId            BIGINT NOT NULL,
    CounterId           BIGINT NOT NULL,
    TerminalId          BIGINT NOT NULL,
    UserId              BIGINT NOT NULL,
    CustomerId          BIGINT NULL, /* FK deferred */
    InvoiceNumber       NVARCHAR(50) NOT NULL,
    SaleDate            DATETIME2 NOT NULL,
    SaleType            NVARCHAR(30) NOT NULL,
    Status              NVARCHAR(30) NOT NULL,
    CurrencyCode        CHAR(3) NOT NULL CONSTRAINT DF_Sales_Currency DEFAULT ('PKR'),
    Subtotal            DECIMAL(19,4) NOT NULL,
    DiscountAmount      DECIMAL(19,4) NOT NULL CONSTRAINT DF_Sales_Discount DEFAULT (0),
    TaxAmount           DECIMAL(19,4) NOT NULL CONSTRAINT DF_Sales_Tax DEFAULT (0),
    RoundOff            DECIMAL(19,4) NOT NULL CONSTRAINT DF_Sales_RoundOff DEFAULT (0),
    NetAmount           DECIMAL(19,4) NOT NULL,
    PaidAmount          DECIMAL(19,4) NOT NULL CONSTRAINT DF_Sales_Paid DEFAULT (0),
    DueAmount           DECIMAL(19,4) NOT NULL CONSTRAINT DF_Sales_Due DEFAULT (0),
    ChangeAmount        DECIMAL(19,4) NOT NULL CONSTRAINT DF_Sales_Change DEFAULT (0),
    PaymentStatus       NVARCHAR(30) NOT NULL,
    FBRStatus           NVARCHAR(30) NULL, /* kept on Sale; fiscal truth in FiscalDocuments */
    CreatedAt           DATETIME2 NOT NULL CONSTRAINT DF_Sales_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt           DATETIME2 NOT NULL CONSTRAINT DF_Sales_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    RowVersion          ROWVERSION NOT NULL,
    CONSTRAINT PK_Sales PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Sales_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_Sales_Counters FOREIGN KEY (CounterId) REFERENCES dbo.Counters(Id),
    CONSTRAINT FK_Sales_Terminals FOREIGN KEY (TerminalId) REFERENCES dbo.POSTerminals(Id),
    CONSTRAINT FK_Sales_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT UQ_Sales_Branch_Invoice UNIQUE (BranchId, InvoiceNumber),
    CONSTRAINT CK_Sales_Status CHECK (Status IN (N'Draft', N'Completed', N'Voided', N'Returned', N'PartiallyReturned')),
    CONSTRAINT CK_Sales_SaleType CHECK (SaleType IN (N'Retail', N'Credit', N'Wholesale', N'Return')),
    CONSTRAINT CK_Sales_PaymentStatus CHECK (PaymentStatus IN (N'Unpaid', N'Partial', N'Paid', N'Refunded')),
    CONSTRAINT CK_Sales_Amounts CHECK (
        Subtotal >= 0 AND DiscountAmount >= 0 AND TaxAmount >= 0
        AND NetAmount >= 0 AND PaidAmount >= 0 AND DueAmount >= 0 AND ChangeAmount >= 0
    )
);
GO

/* PrescriptionItemId FK deferred (18) */
IF OBJECT_ID(N'dbo.SaleLines', N'U') IS NULL
CREATE TABLE dbo.SaleLines (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    SaleId                  BIGINT NOT NULL,
    ProductId               BIGINT NOT NULL,
    ProductUnitId           BIGINT NOT NULL,
    Quantity                DECIMAL(19,6) NOT NULL,
    BaseQuantity            DECIMAL(19,6) NOT NULL,
    ConversionFactor        DECIMAL(19,6) NOT NULL,
    UnitPrice               DECIMAL(19,4) NOT NULL,
    MRP                     DECIMAL(19,4) NOT NULL,
    DiscountAmount          DECIMAL(19,4) NOT NULL CONSTRAINT DF_SaleLines_Discount DEFAULT (0),
    TaxAmount               DECIMAL(19,4) NOT NULL CONSTRAINT DF_SaleLines_Tax DEFAULT (0),
    NetAmount               DECIMAL(19,4) NOT NULL,
    PrescriptionItemId      BIGINT NULL, /* FK deferred */
    CONSTRAINT PK_SaleLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SaleLines_Sales FOREIGN KEY (SaleId) REFERENCES dbo.Sales(Id),
    CONSTRAINT FK_SaleLines_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_SaleLines_ProductUnits FOREIGN KEY (ProductUnitId) REFERENCES dbo.ProductUnits(Id),
    CONSTRAINT CK_SaleLines_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_SaleLines_BaseQuantity CHECK (BaseQuantity > 0),
    CONSTRAINT CK_SaleLines_Conversion CHECK (ConversionFactor > 0),
    CONSTRAINT CK_SaleLines_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT CK_SaleLines_MRP CHECK (MRP >= 0),
    CONSTRAINT CK_SaleLines_NetAmount CHECK (NetAmount >= 0)
);
GO

IF OBJECT_ID(N'dbo.SaleLineBatches', N'U') IS NULL
CREATE TABLE dbo.SaleLineBatches (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    SaleLineId      BIGINT NOT NULL,
    BatchId         BIGINT NOT NULL,
    Quantity        DECIMAL(19,6) NOT NULL,
    BaseQuantity    DECIMAL(19,6) NOT NULL,
    UnitCost        DECIMAL(19,4) NOT NULL,
    CONSTRAINT PK_SaleLineBatches PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SLB_SaleLines FOREIGN KEY (SaleLineId) REFERENCES dbo.SaleLines(Id),
    CONSTRAINT FK_SLB_Batches FOREIGN KEY (BatchId) REFERENCES dbo.InventoryBatches(Id),
    CONSTRAINT CK_SLB_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_SLB_BaseQuantity CHECK (BaseQuantity > 0),
    CONSTRAINT CK_SLB_UnitCost CHECK (UnitCost >= 0)
);
GO

IF OBJECT_ID(N'dbo.PaymentMethods', N'U') IS NULL
CREATE TABLE dbo.PaymentMethods (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(100) NOT NULL,
    Code            NVARCHAR(50) NOT NULL,
    Type            NVARCHAR(30) NOT NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_PaymentMethods_IsActive DEFAULT (1),
    CONSTRAINT PK_PaymentMethods PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_PaymentMethods_Code UNIQUE (Code),
    CONSTRAINT CK_PaymentMethods_Type CHECK (Type IN (N'Cash', N'Card', N'BankTransfer', N'Wallet', N'Credit', N'Other'))
);
GO

IF OBJECT_ID(N'dbo.SalePayments', N'U') IS NULL
CREATE TABLE dbo.SalePayments (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    SaleId              BIGINT NOT NULL,
    PaymentMethodId     BIGINT NOT NULL,
    Amount              DECIMAL(19,4) NOT NULL,
    ReferenceNumber     NVARCHAR(150) NULL,
    PaymentDate         DATETIME2 NOT NULL,
    Status              NVARCHAR(30) NOT NULL,
    ParentPaymentId     BIGINT NULL,
    TransactionType     NVARCHAR(30) NOT NULL,
    CONSTRAINT PK_SalePayments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SalePayments_Sales FOREIGN KEY (SaleId) REFERENCES dbo.Sales(Id),
    CONSTRAINT FK_SalePayments_Methods FOREIGN KEY (PaymentMethodId) REFERENCES dbo.PaymentMethods(Id),
    CONSTRAINT FK_SalePayments_Parent FOREIGN KEY (ParentPaymentId) REFERENCES dbo.SalePayments(Id),
    CONSTRAINT CK_SalePayments_Amount CHECK (Amount > 0),
    CONSTRAINT CK_SalePayments_Status CHECK (Status IN (N'Pending', N'Completed', N'Failed', N'Refunded', N'Voided')),
    CONSTRAINT CK_SalePayments_TxnType CHECK (TransactionType IN (N'Payment', N'Refund', N'Change'))
);
GO

IF OBJECT_ID(N'dbo.HeldSales', N'U') IS NULL
CREATE TABLE dbo.HeldSales (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    BranchId        BIGINT NOT NULL,
    TerminalId      BIGINT NOT NULL,
    UserId          BIGINT NOT NULL,
    CustomerId      BIGINT NULL, /* FK deferred */
    CartData        NVARCHAR(MAX) NOT NULL,
    TotalAmount     DECIMAL(19,4) NOT NULL,
    HeldAt          DATETIME2 NOT NULL,
    ExpiresAt       DATETIME2 NULL,
    Status          NVARCHAR(30) NOT NULL,
    CONSTRAINT PK_HeldSales PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_HeldSales_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_HeldSales_Terminals FOREIGN KEY (TerminalId) REFERENCES dbo.POSTerminals(Id),
    CONSTRAINT FK_HeldSales_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_HeldSales_Total CHECK (TotalAmount >= 0),
    CONSTRAINT CK_HeldSales_Status CHECK (Status IN (N'Held', N'Resumed', N'Expired', N'Cancelled'))
);
GO

IF OBJECT_ID(N'dbo.SaleReturns', N'U') IS NULL
CREATE TABLE dbo.SaleReturns (
    Id                          BIGINT IDENTITY(1,1) NOT NULL,
    SaleId                      BIGINT NOT NULL,
    BranchId                    BIGINT NOT NULL,
    ReturnNumber                NVARCHAR(50) NOT NULL,
    ReturnDate                  DATETIME2 NOT NULL,
    Reason                      NVARCHAR(500) NULL,
    Status                      NVARCHAR(30) NOT NULL,
    RefundAmount                DECIMAL(19,4) NOT NULL,
    RefundPaymentMethodId       BIGINT NULL,
    CreatedBy                   BIGINT NULL,
    ApprovedBy                  BIGINT NULL,
    CONSTRAINT PK_SaleReturns PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SaleReturns_Sales FOREIGN KEY (SaleId) REFERENCES dbo.Sales(Id),
    CONSTRAINT FK_SaleReturns_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_SaleReturns_PaymentMethods FOREIGN KEY (RefundPaymentMethodId) REFERENCES dbo.PaymentMethods(Id),
    CONSTRAINT UQ_SaleReturns_Branch_Number UNIQUE (BranchId, ReturnNumber),
    CONSTRAINT CK_SaleReturns_Refund CHECK (RefundAmount >= 0),
    CONSTRAINT CK_SaleReturns_Status CHECK (Status IN (N'Draft', N'Approved', N'Posted', N'Cancelled'))
);
GO

IF OBJECT_ID(N'dbo.SaleReturnLines', N'U') IS NULL
CREATE TABLE dbo.SaleReturnLines (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    SaleReturnId            BIGINT NOT NULL,
    SaleLineId              BIGINT NOT NULL,
    ProductId               BIGINT NOT NULL,
    BatchId                 BIGINT NOT NULL,
    ProductUnitId           BIGINT NOT NULL,
    Quantity                DECIMAL(19,6) NOT NULL,
    RefundPrice             DECIMAL(19,4) NOT NULL,
    Condition               NVARCHAR(50) NULL,
    ReturnToStock           BIT NOT NULL CONSTRAINT DF_SRL_ReturnToStock DEFAULT (1),
    DestinationLocationId   BIGINT NULL,
    CONSTRAINT PK_SaleReturnLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SaleReturnLines_Returns FOREIGN KEY (SaleReturnId) REFERENCES dbo.SaleReturns(Id),
    CONSTRAINT FK_SaleReturnLines_SaleLines FOREIGN KEY (SaleLineId) REFERENCES dbo.SaleLines(Id),
    CONSTRAINT FK_SaleReturnLines_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_SaleReturnLines_Batches FOREIGN KEY (BatchId) REFERENCES dbo.InventoryBatches(Id),
    CONSTRAINT FK_SaleReturnLines_ProductUnits FOREIGN KEY (ProductUnitId) REFERENCES dbo.ProductUnits(Id),
    CONSTRAINT FK_SaleReturnLines_Locations FOREIGN KEY (DestinationLocationId) REFERENCES dbo.WarehouseLocations(Id),
    CONSTRAINT CK_SaleReturnLines_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_SaleReturnLines_RefundPrice CHECK (RefundPrice >= 0)
);
GO
