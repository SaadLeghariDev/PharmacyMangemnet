
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.ControlledDrugRegisters', N'U') IS NULL
CREATE TABLE dbo.ControlledDrugRegisters (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    BranchId            BIGINT NOT NULL,
    ProductId           BIGINT NOT NULL,
    RegisterNumber      NVARCHAR(100) NOT NULL,
    OpeningBalance      DECIMAL(19,6) NOT NULL,
    CurrentBalance      DECIMAL(19,6) NOT NULL,
    IsActive            BIT NOT NULL CONSTRAINT DF_CDR_IsActive DEFAULT (1),
    CONSTRAINT PK_ControlledDrugRegisters PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_CDR_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_CDR_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT UQ_CDR_Branch_Register UNIQUE (BranchId, RegisterNumber),
    CONSTRAINT CK_CDR_Opening CHECK (OpeningBalance >= 0),
    CONSTRAINT CK_CDR_Current CHECK (CurrentBalance >= 0)
);
GO

IF OBJECT_ID(N'dbo.ControlledDrugTransactions', N'U') IS NULL
CREATE TABLE dbo.ControlledDrugTransactions (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    RegisterId          BIGINT NOT NULL,
    TransactionType     NVARCHAR(50) NOT NULL,
    ReferenceType       NVARCHAR(100) NULL,
    ReferenceId         BIGINT NULL,
    Quantity            DECIMAL(19,6) NOT NULL,
    BalanceBefore       DECIMAL(19,6) NOT NULL,
    BalanceAfter        DECIMAL(19,6) NOT NULL,
    PrescriptionId      BIGINT NULL,
    DoctorId            BIGINT NULL,
    PerformedBy         BIGINT NULL,
    WitnessedBy         BIGINT NULL,
    TransactionDate     DATETIME2 NOT NULL,
    Remarks             NVARCHAR(1000) NULL,
    CONSTRAINT PK_ControlledDrugTransactions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_CDT_Registers FOREIGN KEY (RegisterId) REFERENCES dbo.ControlledDrugRegisters(Id),
    CONSTRAINT FK_CDT_Prescriptions FOREIGN KEY (PrescriptionId) REFERENCES dbo.Prescriptions(Id),
    CONSTRAINT FK_CDT_Doctors FOREIGN KEY (DoctorId) REFERENCES dbo.Doctors(Id),
    CONSTRAINT CK_CDT_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_CDT_Balances CHECK (BalanceBefore >= 0 AND BalanceAfter >= 0),
    CONSTRAINT CK_CDT_Type CHECK (TransactionType IN (
        N'Receipt', N'Dispense', N'Return', N'Adjustment', N'Destruction', N'TransferIn', N'TransferOut'
    ))
);
GO
