
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.AccountTypes', N'U') IS NULL
CREATE TABLE dbo.AccountTypes (
    Id      BIGINT IDENTITY(1,1) NOT NULL,
    Name    NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_AccountTypes PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_AccountTypes_Name UNIQUE (Name)
);
GO

IF OBJECT_ID(N'dbo.ChartOfAccounts', N'U') IS NULL
CREATE TABLE dbo.ChartOfAccounts (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            BIGINT NOT NULL,
    ParentAccountId     BIGINT NULL,
    Code                NVARCHAR(50) NOT NULL,
    Name                NVARCHAR(150) NOT NULL,
    AccountTypeId       BIGINT NOT NULL,
    IsSystemAccount     BIT NOT NULL CONSTRAINT DF_COA_IsSystem DEFAULT (0),
    IsActive            BIT NOT NULL CONSTRAINT DF_COA_IsActive DEFAULT (1),
    CONSTRAINT PK_ChartOfAccounts PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_COA_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_COA_Parent FOREIGN KEY (ParentAccountId) REFERENCES dbo.ChartOfAccounts(Id),
    CONSTRAINT FK_COA_AccountTypes FOREIGN KEY (AccountTypeId) REFERENCES dbo.AccountTypes(Id),
    CONSTRAINT UQ_COA_Tenant_Code UNIQUE (TenantId, Code)
);
GO

IF OBJECT_ID(N'dbo.JournalEntries', N'U') IS NULL
CREATE TABLE dbo.JournalEntries (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    TenantId                BIGINT NOT NULL,
    BranchId                BIGINT NOT NULL,
    EntryNumber             NVARCHAR(50) NOT NULL,
    EntryDate               DATETIME2 NOT NULL,
    ReferenceType           NVARCHAR(100) NULL,
    ReferenceId             BIGINT NULL,
    Description             NVARCHAR(1000) NULL,
    Status                  NVARCHAR(30) NOT NULL,
    PostedBy                BIGINT NULL,
    PostedAt                DATETIME2 NULL,
    ReversalOfEntryId       BIGINT NULL,
    CONSTRAINT PK_JournalEntries PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_JE_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_JE_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_JE_Reversal FOREIGN KEY (ReversalOfEntryId) REFERENCES dbo.JournalEntries(Id),
    CONSTRAINT UQ_JE_Tenant_EntryNumber UNIQUE (TenantId, EntryNumber),
    CONSTRAINT CK_JE_Status CHECK (Status IN (N'Draft', N'Posted', N'Reversed'))
);
GO

IF OBJECT_ID(N'dbo.JournalLines', N'U') IS NULL
CREATE TABLE dbo.JournalLines (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    JournalEntryId      BIGINT NOT NULL,
    [LineNo]           INT NOT NULL,
    AccountId           BIGINT NOT NULL,
    Debit               DECIMAL(19,4) NOT NULL CONSTRAINT DF_JL_Debit DEFAULT (0),
    Credit              DECIMAL(19,4) NOT NULL CONSTRAINT DF_JL_Credit DEFAULT (0),
    Description         NVARCHAR(500) NULL,
    CONSTRAINT PK_JournalLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_JL_Entries FOREIGN KEY (JournalEntryId) REFERENCES dbo.JournalEntries(Id),
    CONSTRAINT FK_JL_Accounts FOREIGN KEY (AccountId) REFERENCES dbo.ChartOfAccounts(Id),
    CONSTRAINT UQ_JL_Entry_LineNo UNIQUE (JournalEntryId, [LineNo]),
    CONSTRAINT CK_JL_Amounts CHECK (Debit >= 0 AND Credit >= 0),
    CONSTRAINT CK_JL_DebitXorCredit CHECK (
        (Debit > 0 AND Credit = 0) OR (Credit > 0 AND Debit = 0)
    )
);
GO

IF OBJECT_ID(N'dbo.CashShifts', N'U') IS NULL
CREATE TABLE dbo.CashShifts (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    BranchId            BIGINT NOT NULL,
    CounterId           BIGINT NOT NULL,
    TerminalId          BIGINT NOT NULL,
    UserId              BIGINT NOT NULL,
    OpeningAmount       DECIMAL(19,4) NOT NULL,
    OpeningAt           DATETIME2 NOT NULL,
    ClosingAmount       DECIMAL(19,4) NULL,
    ExpectedAmount      DECIMAL(19,4) NULL,
    VarianceAmount      DECIMAL(19,4) NULL,
    ClosingAt           DATETIME2 NULL,
    Status              NVARCHAR(30) NOT NULL,
    CONSTRAINT PK_CashShifts PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_CashShifts_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_CashShifts_Counters FOREIGN KEY (CounterId) REFERENCES dbo.Counters(Id),
    CONSTRAINT FK_CashShifts_Terminals FOREIGN KEY (TerminalId) REFERENCES dbo.POSTerminals(Id),
    CONSTRAINT FK_CashShifts_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_CashShifts_Opening CHECK (OpeningAmount >= 0),
    CONSTRAINT CK_CashShifts_Status CHECK (Status IN (N'Open', N'Closed', N'Reconciled'))
);
GO

IF OBJECT_ID(N'dbo.CashTransactions', N'U') IS NULL
CREATE TABLE dbo.CashTransactions (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    CashShiftId         BIGINT NOT NULL,
    TransactionType     NVARCHAR(50) NOT NULL,
    ReferenceType       NVARCHAR(100) NULL,
    ReferenceId         BIGINT NULL,
    Amount              DECIMAL(19,4) NOT NULL,
    Remarks             NVARCHAR(500) NULL,
    CreatedBy           BIGINT NULL,
    CreatedAt           DATETIME2 NOT NULL CONSTRAINT DF_CashTxn_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_CashTransactions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_CashTransactions_Shifts FOREIGN KEY (CashShiftId) REFERENCES dbo.CashShifts(Id),
    CONSTRAINT CK_CashTransactions_Amount CHECK (Amount <> 0),
    CONSTRAINT CK_CashTransactions_Type CHECK (TransactionType IN (
        N'Sale', N'Refund', N'PayIn', N'PayOut', N'Expense', N'Opening', N'Closing'
    ))
);
GO

IF OBJECT_ID(N'dbo.ExpenseCategories', N'U') IS NULL
CREATE TABLE dbo.ExpenseCategories (
    Id      BIGINT IDENTITY(1,1) NOT NULL,
    Name    NVARCHAR(150) NOT NULL,
    Code    NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_ExpenseCategories PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_ExpenseCategories_Code UNIQUE (Code)
);
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NULL
CREATE TABLE dbo.Expenses (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    BranchId            BIGINT NOT NULL,
    CategoryId          BIGINT NOT NULL,
    ExpenseNumber       NVARCHAR(50) NOT NULL,
    ExpenseDate         DATETIME2 NOT NULL,
    Amount              DECIMAL(19,4) NOT NULL,
    PaymentMethodId     BIGINT NOT NULL,
    Description         NVARCHAR(1000) NULL,
    CreatedBy           BIGINT NULL,
    ApprovedBy          BIGINT NULL,
    CONSTRAINT PK_Expenses PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Expenses_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_Expenses_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.ExpenseCategories(Id),
    CONSTRAINT FK_Expenses_PaymentMethods FOREIGN KEY (PaymentMethodId) REFERENCES dbo.PaymentMethods(Id),
    CONSTRAINT UQ_Expenses_Branch_Number UNIQUE (BranchId, ExpenseNumber),
    CONSTRAINT CK_Expenses_Amount CHECK (Amount > 0)
);
GO
