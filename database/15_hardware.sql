
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.DeviceTypes', N'U') IS NULL
CREATE TABLE dbo.DeviceTypes (
    Id      BIGINT IDENTITY(1,1) NOT NULL,
    Code    NVARCHAR(50) NOT NULL,
    Name    NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_DeviceTypes PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_DeviceTypes_Code UNIQUE (Code)
);
GO

IF OBJECT_ID(N'dbo.Devices', N'U') IS NULL
CREATE TABLE dbo.Devices (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    BranchId            BIGINT NOT NULL,
    CounterId           BIGINT NULL,
    DeviceTypeId        BIGINT NOT NULL,
    Name                NVARCHAR(150) NOT NULL,
    Manufacturer        NVARCHAR(150) NULL,
    Model               NVARCHAR(150) NULL,
    SerialNumber        NVARCHAR(150) NULL,
    ConnectionType      NVARCHAR(50) NULL,
    IP                  NVARCHAR(50) NULL,
    Port                INT NULL,
    COMPort             NVARCHAR(50) NULL,
    MacAddress          NVARCHAR(100) NULL,
    DriverName          NVARCHAR(150) NULL,
    DriverVersion       NVARCHAR(50) NULL,
    IsDefault           BIT NOT NULL CONSTRAINT DF_Devices_IsDefault DEFAULT (0),
    IsActive            BIT NOT NULL CONSTRAINT DF_Devices_IsActive DEFAULT (1),
    LastSeenAt          DATETIME2 NULL,
    CONSTRAINT PK_Devices PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Devices_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_Devices_Counters FOREIGN KEY (CounterId) REFERENCES dbo.Counters(Id),
    CONSTRAINT FK_Devices_DeviceTypes FOREIGN KEY (DeviceTypeId) REFERENCES dbo.DeviceTypes(Id)
);
GO

IF OBJECT_ID(N'dbo.DeviceAssignments', N'U') IS NULL
CREATE TABLE dbo.DeviceAssignments (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    DeviceId        BIGINT NOT NULL,
    TerminalId      BIGINT NOT NULL,
    AssignedFrom    DATETIME2 NOT NULL,
    AssignedTo      DATETIME2 NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_DeviceAssignments_IsActive DEFAULT (1),
    CONSTRAINT PK_DeviceAssignments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_DeviceAssignments_Devices FOREIGN KEY (DeviceId) REFERENCES dbo.Devices(Id),
    CONSTRAINT FK_DeviceAssignments_Terminals FOREIGN KEY (TerminalId) REFERENCES dbo.POSTerminals(Id)
);
GO

IF OBJECT_ID(N'dbo.DeviceSettings', N'U') IS NULL
CREATE TABLE dbo.DeviceSettings (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    DeviceId        BIGINT NOT NULL,
    SettingKey      NVARCHAR(100) NOT NULL,
    SettingValue    NVARCHAR(MAX) NULL,
    IsEncrypted     BIT NOT NULL CONSTRAINT DF_DeviceSettings_IsEncrypted DEFAULT (0),
    CONSTRAINT PK_DeviceSettings PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_DeviceSettings_Devices FOREIGN KEY (DeviceId) REFERENCES dbo.Devices(Id),
    CONSTRAINT UQ_DeviceSettings_Device_Key UNIQUE (DeviceId, SettingKey)
);
GO

IF OBJECT_ID(N'dbo.DeviceEvents', N'U') IS NULL
CREATE TABLE dbo.DeviceEvents (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    DeviceId        BIGINT NOT NULL,
    EventType       NVARCHAR(100) NOT NULL,
    Payload         NVARCHAR(MAX) NULL,
    Status          NVARCHAR(50) NOT NULL,
    ErrorMessage    NVARCHAR(2000) NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_DeviceEvents_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_DeviceEvents PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_DeviceEvents_Devices FOREIGN KEY (DeviceId) REFERENCES dbo.Devices(Id)
);
GO

/* PrintTemplates before BarcodePrintJobs to avoid circular FK — TemplateId FK added here */
IF OBJECT_ID(N'dbo.PrintTemplates', N'U') IS NULL
CREATE TABLE dbo.PrintTemplates (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            BIGINT NOT NULL,
    TemplateType        NVARCHAR(50) NOT NULL,
    Name                NVARCHAR(150) NOT NULL,
    TemplateContent     NVARCHAR(MAX) NOT NULL,
    PaperWidth          DECIMAL(10,2) NULL,
    IsDefault           BIT NOT NULL CONSTRAINT DF_PrintTemplates_IsDefault DEFAULT (0),
    IsActive            BIT NOT NULL CONSTRAINT DF_PrintTemplates_IsActive DEFAULT (1),
    CONSTRAINT PK_PrintTemplates PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_PrintTemplates_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)
);
GO

IF OBJECT_ID(N'dbo.BarcodePrintJobs', N'U') IS NULL
CREATE TABLE dbo.BarcodePrintJobs (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    BranchId            BIGINT NOT NULL,
    PrinterDeviceId     BIGINT NOT NULL,
    ProductId           BIGINT NOT NULL,
    BatchId             BIGINT NULL,
    Quantity            INT NOT NULL,
    TemplateId          BIGINT NOT NULL,
    Status              NVARCHAR(30) NOT NULL,
    CreatedBy           BIGINT NULL,
    CreatedAt           DATETIME2 NOT NULL CONSTRAINT DF_BPJ_CreatedAt DEFAULT (SYSUTCDATETIME()),
    PrintedAt           DATETIME2 NULL,
    CONSTRAINT PK_BarcodePrintJobs PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_BPJ_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_BPJ_Devices FOREIGN KEY (PrinterDeviceId) REFERENCES dbo.Devices(Id),
    CONSTRAINT FK_BPJ_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_BPJ_Batches FOREIGN KEY (BatchId) REFERENCES dbo.InventoryBatches(Id),
    CONSTRAINT FK_BPJ_Templates FOREIGN KEY (TemplateId) REFERENCES dbo.PrintTemplates(Id),
    CONSTRAINT CK_BPJ_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_BPJ_Status CHECK (Status IN (N'Queued', N'Printing', N'Printed', N'Failed', N'Cancelled'))
);
GO
