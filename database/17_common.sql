
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Attachments', N'U') IS NULL
CREATE TABLE dbo.Attachments (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TenantId        BIGINT NOT NULL,
    FileName        NVARCHAR(250) NOT NULL,
    StoragePath     NVARCHAR(1000) NOT NULL,
    ContentType     NVARCHAR(150) NULL,
    FileSize        BIGINT NOT NULL,
    Hash            NVARCHAR(128) NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Attachments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Attachments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Attachments_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT CK_Attachments_FileSize CHECK (FileSize >= 0)
);
GO

IF OBJECT_ID(N'dbo.EntityAttachments', N'U') IS NULL
CREATE TABLE dbo.EntityAttachments (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    AttachmentId    BIGINT NOT NULL,
    EntityName      NVARCHAR(150) NOT NULL,
    EntityId        BIGINT NOT NULL,
    CONSTRAINT PK_EntityAttachments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_EntityAttachments_Attachments FOREIGN KEY (AttachmentId) REFERENCES dbo.Attachments(Id)
);
GO

IF OBJECT_ID(N'dbo.TenantSettings', N'U') IS NULL
CREATE TABLE dbo.TenantSettings (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TenantId        BIGINT NOT NULL,
    SettingKey      NVARCHAR(150) NOT NULL,
    SettingValue    NVARCHAR(MAX) NULL,
    IsEncrypted     BIT NOT NULL CONSTRAINT DF_TenantSettings_IsEncrypted DEFAULT (0),
    CONSTRAINT PK_TenantSettings PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_TenantSettings_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT UQ_TenantSettings_Tenant_Key UNIQUE (TenantId, SettingKey)
);
GO

IF OBJECT_ID(N'dbo.BranchSettings', N'U') IS NULL
CREATE TABLE dbo.BranchSettings (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    BranchId        BIGINT NOT NULL,
    SettingKey      NVARCHAR(150) NOT NULL,
    SettingValue    NVARCHAR(MAX) NULL,
    IsEncrypted     BIT NOT NULL CONSTRAINT DF_BranchSettings_IsEncrypted DEFAULT (0),
    CONSTRAINT PK_BranchSettings PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_BranchSettings_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT UQ_BranchSettings_Branch_Key UNIQUE (BranchId, SettingKey)
);
GO

IF OBJECT_ID(N'dbo.NumberSequences', N'U') IS NULL
CREATE TABLE dbo.NumberSequences (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            BIGINT NOT NULL,
    BranchId            BIGINT NULL,
    TerminalId          BIGINT NULL,
    DocumentType        NVARCHAR(50) NOT NULL,
    Prefix              NVARCHAR(30) NULL,
    CurrentNumber       BIGINT NOT NULL CONSTRAINT DF_NumberSequences_Current DEFAULT (0),
    NumberLength        INT NOT NULL CONSTRAINT DF_NumberSequences_Length DEFAULT (6),
    ResetPeriod         NVARCHAR(30) NULL,
    CONSTRAINT PK_NumberSequences PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_NumberSequences_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_NumberSequences_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_NumberSequences_Terminals FOREIGN KEY (TerminalId) REFERENCES dbo.POSTerminals(Id),
    CONSTRAINT CK_NumberSequences_Current CHECK (CurrentNumber >= 0),
    CONSTRAINT CK_NumberSequences_Length CHECK (NumberLength > 0)
);
GO

IF OBJECT_ID(N'dbo.ReasonCodes', N'U') IS NULL
CREATE TABLE dbo.ReasonCodes (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TenantId        BIGINT NOT NULL,
    ReasonType      NVARCHAR(50) NOT NULL,
    Code            NVARCHAR(50) NOT NULL,
    Name            NVARCHAR(150) NOT NULL,
    Description     NVARCHAR(500) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_ReasonCodes_IsActive DEFAULT (1),
    CONSTRAINT PK_ReasonCodes PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_ReasonCodes_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT UQ_ReasonCodes_Tenant_Type_Code UNIQUE (TenantId, ReasonType, Code),
    CONSTRAINT CK_ReasonCodes_Type CHECK (ReasonType IN (N'Return', N'Adjustment', N'Count', N'Void', N'Other'))
);
GO
