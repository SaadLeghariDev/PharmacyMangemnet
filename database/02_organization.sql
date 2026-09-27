USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Tenants', N'U') IS NULL
CREATE TABLE dbo.Tenants (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(200) NOT NULL,
    LegalName       NVARCHAR(250) NULL,
    NTN             NVARCHAR(50) NULL,
    STRN            NVARCHAR(50) NULL,
    LicenseNo       NVARCHAR(100) NULL,
    Phone           NVARCHAR(50) NULL,
    Email           NVARCHAR(200) NULL,
    Address         NVARCHAR(500) NULL,
    LogoUrl         NVARCHAR(500) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Tenants_IsActive DEFAULT (1),
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Tenants_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Tenants_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    RowVersion      ROWVERSION NOT NULL,
    CONSTRAINT PK_Tenants PRIMARY KEY CLUSTERED (Id)
);
GO

IF OBJECT_ID(N'dbo.Branches', N'U') IS NULL
CREATE TABLE dbo.Branches (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TenantId        BIGINT NOT NULL,
    Code            NVARCHAR(50) NOT NULL,
    Name            NVARCHAR(200) NOT NULL,
    BranchType      NVARCHAR(50) NULL,
    NTN             NVARCHAR(50) NULL,
    STRN            NVARCHAR(50) NULL,
    DrugLicenseNo   NVARCHAR(100) NULL,
    Phone           NVARCHAR(50) NULL,
    Email           NVARCHAR(200) NULL,
    Address         NVARCHAR(500) NULL,
    City            NVARCHAR(100) NULL,
    Province        NVARCHAR(100) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Branches_IsActive DEFAULT (1),
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Branches_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Branches_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    RowVersion      ROWVERSION NOT NULL,
    CONSTRAINT PK_Branches PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Branches_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT UQ_Branches_Tenant_Code UNIQUE (TenantId, Code)
);
GO

IF OBJECT_ID(N'dbo.Warehouses', N'U') IS NULL
CREATE TABLE dbo.Warehouses (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    BranchId                BIGINT NOT NULL,
    Code                    NVARCHAR(50) NOT NULL,
    Name                    NVARCHAR(200) NOT NULL,
    WarehouseType           NVARCHAR(50) NULL,
    IsMain                  BIT NOT NULL CONSTRAINT DF_Warehouses_IsMain DEFAULT (0),
    TemperatureControlled   BIT NOT NULL CONSTRAINT DF_Warehouses_Temp DEFAULT (0),
    IsActive                BIT NOT NULL CONSTRAINT DF_Warehouses_IsActive DEFAULT (1),
    CONSTRAINT PK_Warehouses PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Warehouses_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT UQ_Warehouses_Branch_Code UNIQUE (BranchId, Code)
);
GO

IF OBJECT_ID(N'dbo.WarehouseLocations', N'U') IS NULL
CREATE TABLE dbo.WarehouseLocations (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    WarehouseId     BIGINT NOT NULL,
    Code            NVARCHAR(50) NOT NULL,
    Name            NVARCHAR(150) NOT NULL,
    RackNo          NVARCHAR(50) NULL,
    ShelfNo         NVARCHAR(50) NULL,
    BinNo           NVARCHAR(50) NULL,
    LocationType    NVARCHAR(50) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_WarehouseLocations_IsActive DEFAULT (1),
    CONSTRAINT PK_WarehouseLocations PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_WarehouseLocations_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT UQ_WarehouseLocations_Warehouse_Code UNIQUE (WarehouseId, Code)
);
GO

IF OBJECT_ID(N'dbo.Counters', N'U') IS NULL
CREATE TABLE dbo.Counters (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    BranchId        BIGINT NOT NULL,
    Code            NVARCHAR(50) NOT NULL,
    Name            NVARCHAR(100) NOT NULL,
    CounterType     NVARCHAR(50) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Counters_IsActive DEFAULT (1),
    CONSTRAINT PK_Counters PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Counters_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT UQ_Counters_Branch_Code UNIQUE (BranchId, Code)
);
GO

IF OBJECT_ID(N'dbo.POSTerminals', N'U') IS NULL
CREATE TABLE dbo.POSTerminals (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    BranchId            BIGINT NOT NULL,
    CounterId           BIGINT NOT NULL,
    TerminalCode        NVARCHAR(50) NOT NULL,
    ComputerName        NVARCHAR(150) NULL,
    MacAddress          NVARCHAR(100) NULL,
    IPAddress           NVARCHAR(50) NULL,
    SerialNumber        NVARCHAR(100) NULL,
    IsPrimary           BIT NOT NULL CONSTRAINT DF_POSTerminals_IsPrimary DEFAULT (0),
    IsOnline            BIT NOT NULL CONSTRAINT DF_POSTerminals_IsOnline DEFAULT (0),
    LastSyncAt          DATETIME2 NULL,
    LastHeartbeatAt     DATETIME2 NULL,
    IsActive            BIT NOT NULL CONSTRAINT DF_POSTerminals_IsActive DEFAULT (1),
    CONSTRAINT PK_POSTerminals PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_POSTerminals_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_POSTerminals_Counters FOREIGN KEY (CounterId) REFERENCES dbo.Counters(Id),
    CONSTRAINT UQ_POSTerminals_Branch_Code UNIQUE (BranchId, TerminalCode)
);
GO
