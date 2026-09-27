
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.AlertRules', N'U') IS NULL
CREATE TABLE dbo.AlertRules (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    TenantId            BIGINT NOT NULL,
    BranchId            BIGINT NULL,
    AlertType           NVARCHAR(50) NOT NULL,
    Threshold           DECIMAL(19,6) NULL,
    DaysBeforeExpiry    INT NULL,
    IsActive            BIT NOT NULL CONSTRAINT DF_AlertRules_IsActive DEFAULT (1),
    CONSTRAINT PK_AlertRules PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_AlertRules_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_AlertRules_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id)
);
GO

IF OBJECT_ID(N'dbo.Alerts', N'U') IS NULL
CREATE TABLE dbo.Alerts (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    AlertRuleId     BIGINT NOT NULL,
    BranchId        BIGINT NOT NULL,
    ProductId       BIGINT NULL,
    BatchId         BIGINT NULL,
    Severity        NVARCHAR(30) NOT NULL,
    Title           NVARCHAR(200) NOT NULL,
    Message         NVARCHAR(1000) NULL,
    Status          NVARCHAR(30) NOT NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Alerts_CreatedAt DEFAULT (SYSUTCDATETIME()),
    ResolvedAt      DATETIME2 NULL,
    ResolvedBy      BIGINT NULL,
    CONSTRAINT PK_Alerts PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Alerts_Rules FOREIGN KEY (AlertRuleId) REFERENCES dbo.AlertRules(Id),
    CONSTRAINT FK_Alerts_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_Alerts_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_Alerts_Batches FOREIGN KEY (BatchId) REFERENCES dbo.InventoryBatches(Id),
    CONSTRAINT CK_Alerts_Severity CHECK (Severity IN (N'Info', N'Warning', N'Critical')),
    CONSTRAINT CK_Alerts_Status CHECK (Status IN (N'Open', N'Acknowledged', N'Resolved', N'Dismissed'))
);
GO

IF OBJECT_ID(N'dbo.NotificationTemplates', N'U') IS NULL
CREATE TABLE dbo.NotificationTemplates (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TenantId        BIGINT NOT NULL,
    Code            NVARCHAR(100) NOT NULL,
    Channel         NVARCHAR(30) NOT NULL,
    Subject         NVARCHAR(250) NULL,
    Body            NVARCHAR(MAX) NOT NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_NotificationTemplates_IsActive DEFAULT (1),
    CONSTRAINT PK_NotificationTemplates PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_NotificationTemplates_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT UQ_NotificationTemplates_Tenant_Code UNIQUE (TenantId, Code)
);
GO

IF OBJECT_ID(N'dbo.NotificationLogs', N'U') IS NULL
CREATE TABLE dbo.NotificationLogs (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TemplateId      BIGINT NOT NULL,
    Recipient       NVARCHAR(250) NOT NULL,
    Channel         NVARCHAR(30) NOT NULL,
    ReferenceType   NVARCHAR(100) NULL,
    ReferenceId     BIGINT NULL,
    Status          NVARCHAR(30) NOT NULL,
    SentAt          DATETIME2 NULL,
    ErrorMessage    NVARCHAR(2000) NULL,
    CONSTRAINT PK_NotificationLogs PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_NotificationLogs_Templates FOREIGN KEY (TemplateId) REFERENCES dbo.NotificationTemplates(Id),
    CONSTRAINT CK_NotificationLogs_Status CHECK (Status IN (N'Pending', N'Sent', N'Failed'))
);
GO

IF OBJECT_ID(N'dbo.SyncNodes', N'U') IS NULL
CREATE TABLE dbo.SyncNodes (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TenantId        BIGINT NOT NULL,
    BranchId        BIGINT NOT NULL,
    TerminalId      BIGINT NOT NULL,
    NodeCode        NVARCHAR(100) NOT NULL,
    LastSyncAt      DATETIME2 NULL,
    LastSequence    BIGINT NOT NULL CONSTRAINT DF_SyncNodes_LastSequence DEFAULT (0),
    IsActive        BIT NOT NULL CONSTRAINT DF_SyncNodes_IsActive DEFAULT (1),
    CONSTRAINT PK_SyncNodes PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SyncNodes_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_SyncNodes_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_SyncNodes_Terminals FOREIGN KEY (TerminalId) REFERENCES dbo.POSTerminals(Id),
    CONSTRAINT UQ_SyncNodes_NodeCode UNIQUE (NodeCode)
);
GO

IF OBJECT_ID(N'dbo.SyncBatches', N'U') IS NULL
CREATE TABLE dbo.SyncBatches (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    SyncNodeId      BIGINT NOT NULL,
    BatchNumber     NVARCHAR(100) NOT NULL,
    StartedAt       DATETIME2 NOT NULL,
    CompletedAt     DATETIME2 NULL,
    Status          NVARCHAR(30) NOT NULL,
    CONSTRAINT PK_SyncBatches PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SyncBatches_Nodes FOREIGN KEY (SyncNodeId) REFERENCES dbo.SyncNodes(Id),
    CONSTRAINT UQ_SyncBatches_Node_BatchNumber UNIQUE (SyncNodeId, BatchNumber),
    CONSTRAINT CK_SyncBatches_Status CHECK (Status IN (N'InProgress', N'Completed', N'Failed', N'Partial'))
);
GO

IF OBJECT_ID(N'dbo.SyncItems', N'U') IS NULL
CREATE TABLE dbo.SyncItems (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    SyncBatchId     BIGINT NOT NULL,
    EntityName      NVARCHAR(150) NOT NULL,
    EntityId        BIGINT NOT NULL,
    Operation       NVARCHAR(30) NOT NULL,
    Payload         NVARCHAR(MAX) NULL,
    Version         BIGINT NOT NULL,
    ProcessedAt     DATETIME2 NULL,
    Status          NVARCHAR(30) NOT NULL,
    ErrorMessage    NVARCHAR(2000) NULL,
    CONSTRAINT PK_SyncItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SyncItems_Batches FOREIGN KEY (SyncBatchId) REFERENCES dbo.SyncBatches(Id),
    CONSTRAINT CK_SyncItems_Operation CHECK (Operation IN (N'Insert', N'Update', N'Delete')),
    CONSTRAINT CK_SyncItems_Status CHECK (Status IN (N'Pending', N'Processed', N'Failed', N'Skipped'))
);
GO

IF OBJECT_ID(N'dbo.IdempotencyKeys', N'U') IS NULL
CREATE TABLE dbo.IdempotencyKeys (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TerminalId      BIGINT NOT NULL,
    [Key]           NVARCHAR(150) NOT NULL,
    EntityType      NVARCHAR(100) NOT NULL,
    EntityId        BIGINT NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_IdempotencyKeys_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_IdempotencyKeys PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_IdempotencyKeys_Terminals FOREIGN KEY (TerminalId) REFERENCES dbo.POSTerminals(Id),
    CONSTRAINT UQ_IdempotencyKeys_Terminal_Key UNIQUE (TerminalId, [Key])
);
GO
