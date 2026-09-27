
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
CREATE TABLE dbo.Customers (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TenantId        BIGINT NOT NULL,
    CustomerCode    NVARCHAR(50) NOT NULL,
    Name            NVARCHAR(200) NOT NULL,
    CNIC            NVARCHAR(50) NULL,
    Phone           NVARCHAR(50) NULL,
    Email           NVARCHAR(200) NULL,
    Address         NVARCHAR(500) NULL,
    DateOfBirth     DATE NULL,
    Gender          NVARCHAR(30) NULL,
    CreditLimit     DECIMAL(19,4) NOT NULL CONSTRAINT DF_Customers_CreditLimit DEFAULT (0),
    IsPatient       BIT NOT NULL CONSTRAINT DF_Customers_IsPatient DEFAULT (0),
    IsActive        BIT NOT NULL CONSTRAINT DF_Customers_IsActive DEFAULT (1),
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Customers_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Customers_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Customers PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Customers_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT UQ_Customers_Tenant_Code UNIQUE (TenantId, CustomerCode),
    CONSTRAINT CK_Customers_CreditLimit CHECK (CreditLimit >= 0)
);
GO

IF OBJECT_ID(N'dbo.CustomerPayments', N'U') IS NULL
CREATE TABLE dbo.CustomerPayments (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    CustomerId          BIGINT NOT NULL,
    BranchId            BIGINT NOT NULL,
    PaymentMethodId     BIGINT NOT NULL,
    Amount              DECIMAL(19,4) NOT NULL,
    ReferenceNumber     NVARCHAR(100) NULL,
    PaymentDate         DATETIME2 NOT NULL,
    Remarks             NVARCHAR(500) NULL,
    ReceivedBy          BIGINT NULL,
    CONSTRAINT PK_CustomerPayments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_CustomerPayments_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
    CONSTRAINT FK_CustomerPayments_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_CustomerPayments_Methods FOREIGN KEY (PaymentMethodId) REFERENCES dbo.PaymentMethods(Id),
    CONSTRAINT CK_CustomerPayments_Amount CHECK (Amount > 0)
);
GO

IF OBJECT_ID(N'dbo.CustomerLedger', N'U') IS NULL
CREATE TABLE dbo.CustomerLedger (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    CustomerId          BIGINT NOT NULL,
    BranchId            BIGINT NOT NULL,
    TransactionDate     DATETIME2 NOT NULL,
    TransactionType     NVARCHAR(50) NOT NULL,
    ReferenceType       NVARCHAR(100) NULL,
    ReferenceId         BIGINT NULL,
    Debit               DECIMAL(19,4) NOT NULL CONSTRAINT DF_CL_Debit DEFAULT (0),
    Credit              DECIMAL(19,4) NOT NULL CONSTRAINT DF_CL_Credit DEFAULT (0),
    SequenceNo          BIGINT NOT NULL,
    Remarks             NVARCHAR(500) NULL,
    CONSTRAINT PK_CustomerLedger PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_CustomerLedger_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
    CONSTRAINT FK_CustomerLedger_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT CK_CustomerLedger_Amounts CHECK (Debit >= 0 AND Credit >= 0),
    CONSTRAINT CK_CustomerLedger_DebitXorCredit CHECK (
        (Debit > 0 AND Credit = 0) OR (Credit > 0 AND Debit = 0) OR (Debit = 0 AND Credit = 0)
    )
);
GO
