USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE dbo.Users (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TenantId        BIGINT NOT NULL,
    Username        NVARCHAR(100) NOT NULL,
    Email           NVARCHAR(200) NULL,
    PasswordHash    NVARCHAR(500) NOT NULL,
    FullName        NVARCHAR(200) NOT NULL,
    Phone           NVARCHAR(50) NULL,
    EmployeeCode    NVARCHAR(50) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    LastLoginAt     DATETIME2 NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Users_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    RowVersion      ROWVERSION NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Users_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT UQ_Users_Tenant_Username UNIQUE (TenantId, Username)
);
GO

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
CREATE TABLE dbo.Roles (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TenantId        BIGINT NOT NULL,
    Name            NVARCHAR(100) NOT NULL,
    Description     NVARCHAR(500) NULL,
    IsSystemRole    BIT NOT NULL CONSTRAINT DF_Roles_IsSystemRole DEFAULT (0),
    CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Roles_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT UQ_Roles_Tenant_Name UNIQUE (TenantId, Name)
);
GO

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
CREATE TABLE dbo.Permissions (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    Code            NVARCHAR(100) NOT NULL,
    Name            NVARCHAR(150) NOT NULL,
    Module          NVARCHAR(100) NOT NULL,
    Description     NVARCHAR(500) NULL,
    CONSTRAINT PK_Permissions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Permissions_Code UNIQUE (Code)
);
GO

IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NULL
CREATE TABLE dbo.UserRoles (
    UserId          BIGINT NOT NULL,
    RoleId          BIGINT NOT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY CLUSTERED (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id)
);
GO

IF OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
CREATE TABLE dbo.RolePermissions (
    RoleId          BIGINT NOT NULL,
    PermissionId    BIGINT NOT NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY CLUSTERED (RoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id),
    CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions(Id)
);
GO

IF OBJECT_ID(N'dbo.UserBranches', N'U') IS NULL
CREATE TABLE dbo.UserBranches (
    UserId          BIGINT NOT NULL,
    BranchId        BIGINT NOT NULL,
    CONSTRAINT PK_UserBranches PRIMARY KEY CLUSTERED (UserId, BranchId),
    CONSTRAINT FK_UserBranches_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_UserBranches_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id)
);
GO

IF OBJECT_ID(N'dbo.LoginSessions', N'U') IS NULL
CREATE TABLE dbo.LoginSessions (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    UserId          BIGINT NOT NULL,
    TerminalId      BIGINT NOT NULL,
    StartedAt       DATETIME2 NOT NULL,
    EndedAt         DATETIME2 NULL,
    IPAddress       NVARCHAR(50) NULL,
    Status          NVARCHAR(30) NOT NULL,
    CONSTRAINT PK_LoginSessions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_LoginSessions_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_LoginSessions_POSTerminals FOREIGN KEY (TerminalId) REFERENCES dbo.POSTerminals(Id),
    CONSTRAINT CK_LoginSessions_Status CHECK (Status IN (N'Active', N'Ended', N'Expired', N'ForcedLogout'))
);
GO

/* Soft audit FKs — no FK constraints per PDF (durable audit) */
IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
CREATE TABLE dbo.AuditLogs (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TenantId        BIGINT NULL,
    BranchId        BIGINT NULL,
    UserId          BIGINT NULL,
    EntityName      NVARCHAR(150) NOT NULL,
    EntityId        BIGINT NULL,
    Action          NVARCHAR(50) NOT NULL,
    OldValues       NVARCHAR(MAX) NULL,
    NewValues       NVARCHAR(MAX) NULL,
    IPAddress       NVARCHAR(50) NULL,
    TerminalId      BIGINT NULL,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_AuditLogs PRIMARY KEY CLUSTERED (Id)
);
GO

IF OBJECT_ID(N'dbo.ApprovalRequests', N'U') IS NULL
CREATE TABLE dbo.ApprovalRequests (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    BranchId        BIGINT NULL,
    RequestType     NVARCHAR(100) NOT NULL,
    ReferenceType   NVARCHAR(100) NULL,
    ReferenceId     BIGINT NULL,
    RequestedBy     BIGINT NULL,
    ApprovedBy      BIGINT NULL,
    RejectedBy      BIGINT NULL,
    Status          NVARCHAR(30) NOT NULL,
    Reason          NVARCHAR(1000) NULL,
    RequestedAt     DATETIME2 NOT NULL,
    DecisionAt      DATETIME2 NULL,
    CONSTRAINT PK_ApprovalRequests PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_ApprovalRequests_Status CHECK (Status IN (N'Pending', N'Approved', N'Rejected', N'Cancelled'))
);
GO
