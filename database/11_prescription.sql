
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Doctors', N'U') IS NULL
CREATE TABLE dbo.Doctors (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(200) NOT NULL,
    PMDCNumber      NVARCHAR(100) NULL,
    Specialization  NVARCHAR(150) NULL,
    Phone           NVARCHAR(50) NULL,
    Email           NVARCHAR(200) NULL,
    ClinicName      NVARCHAR(200) NULL,
    Address         NVARCHAR(500) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Doctors_IsActive DEFAULT (1),
    CONSTRAINT PK_Doctors PRIMARY KEY CLUSTERED (Id)
);
GO

/* PrescriptionAttachmentId FK deferred until Attachments (18) */
IF OBJECT_ID(N'dbo.Prescriptions', N'U') IS NULL
CREATE TABLE dbo.Prescriptions (
    Id                          BIGINT IDENTITY(1,1) NOT NULL,
    CustomerId                  BIGINT NOT NULL,
    DoctorId                    BIGINT NOT NULL,
    PrescriptionNumber          NVARCHAR(100) NOT NULL,
    PrescriptionDate            DATE NOT NULL,
    DiagnosisNotes              NVARCHAR(2000) NULL,
    PrescriptionAttachmentId    BIGINT NULL, /* FK deferred */
    Status                      NVARCHAR(30) NOT NULL,
    CreatedBy                   BIGINT NULL,
    CreatedAt                   DATETIME2 NOT NULL CONSTRAINT DF_Prescriptions_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Prescriptions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Prescriptions_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
    CONSTRAINT FK_Prescriptions_Doctors FOREIGN KEY (DoctorId) REFERENCES dbo.Doctors(Id),
    CONSTRAINT UQ_Prescriptions_Number UNIQUE (PrescriptionNumber),
    CONSTRAINT CK_Prescriptions_Status CHECK (Status IN (N'Active', N'PartiallyDispensed', N'Dispensed', N'Expired', N'Cancelled'))
);
GO

IF OBJECT_ID(N'dbo.PrescriptionItems', N'U') IS NULL
CREATE TABLE dbo.PrescriptionItems (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    PrescriptionId      BIGINT NOT NULL,
    ProductId           BIGINT NOT NULL,
    DosageAmount        DECIMAL(19,6) NULL,
    DosageUnit          NVARCHAR(50) NULL,
    FrequencyCode       NVARCHAR(50) NULL,
    Route               NVARCHAR(50) NULL,
    DurationValue       DECIMAL(19,6) NULL,
    DurationUnit        NVARCHAR(30) NULL,
    Quantity            DECIMAL(19,6) NOT NULL,
    Instructions        NVARCHAR(1000) NULL,
    RefillAllowed       BIT NOT NULL CONSTRAINT DF_PI_RefillAllowed DEFAULT (0),
    RefillCount         INT NOT NULL CONSTRAINT DF_PI_RefillCount DEFAULT (0),
    CONSTRAINT PK_PrescriptionItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_PrescriptionItems_Prescriptions FOREIGN KEY (PrescriptionId) REFERENCES dbo.Prescriptions(Id),
    CONSTRAINT FK_PrescriptionItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT CK_PI_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_PI_RefillCount CHECK (RefillCount >= 0)
);
GO

IF OBJECT_ID(N'dbo.DispensingRecords', N'U') IS NULL
CREATE TABLE dbo.DispensingRecords (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    PrescriptionId  BIGINT NOT NULL,
    SaleId          BIGINT NOT NULL,
    CustomerId      BIGINT NOT NULL,
    DispensedBy     BIGINT NULL,
    DispensedAt     DATETIME2 NOT NULL,
    CONSTRAINT PK_DispensingRecords PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_DispensingRecords_Prescriptions FOREIGN KEY (PrescriptionId) REFERENCES dbo.Prescriptions(Id),
    CONSTRAINT FK_DispensingRecords_Sales FOREIGN KEY (SaleId) REFERENCES dbo.Sales(Id),
    CONSTRAINT FK_DispensingRecords_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id)
);
GO

IF OBJECT_ID(N'dbo.DispensingItems', N'U') IS NULL
CREATE TABLE dbo.DispensingItems (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    DispensingRecordId      BIGINT NOT NULL,
    PrescriptionItemId      BIGINT NOT NULL,
    ProductId               BIGINT NOT NULL,
    BatchId                 BIGINT NOT NULL,
    Quantity                DECIMAL(19,6) NOT NULL,
    CONSTRAINT PK_DispensingItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_DispensingItems_Records FOREIGN KEY (DispensingRecordId) REFERENCES dbo.DispensingRecords(Id),
    CONSTRAINT FK_DispensingItems_Items FOREIGN KEY (PrescriptionItemId) REFERENCES dbo.PrescriptionItems(Id),
    CONSTRAINT FK_DispensingItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_DispensingItems_Batches FOREIGN KEY (BatchId) REFERENCES dbo.InventoryBatches(Id),
    CONSTRAINT CK_DispensingItems_Quantity CHECK (Quantity > 0)
);
GO

IF OBJECT_ID(N'dbo.Refills', N'U') IS NULL
CREATE TABLE dbo.Refills (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    PrescriptionItemId      BIGINT NOT NULL,
    RefillDate              DATETIME2 NOT NULL,
    Quantity                DECIMAL(19,6) NOT NULL,
    SaleId                  BIGINT NOT NULL,
    DispensedBy             BIGINT NULL,
    Notes                   NVARCHAR(1000) NULL,
    CONSTRAINT PK_Refills PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Refills_PrescriptionItems FOREIGN KEY (PrescriptionItemId) REFERENCES dbo.PrescriptionItems(Id),
    CONSTRAINT FK_Refills_Sales FOREIGN KEY (SaleId) REFERENCES dbo.Sales(Id),
    CONSTRAINT CK_Refills_Quantity CHECK (Quantity > 0)
);
GO
