
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.FiscalConfigurations', N'U') IS NULL
CREATE TABLE dbo.FiscalConfigurations (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    TenantId                BIGINT NOT NULL,
    BranchId                BIGINT NULL,
    IntegrationType         NVARCHAR(50) NOT NULL,
    ProviderName            NVARCHAR(100) NOT NULL,
    NTN                     NVARCHAR(50) NULL,
    STRN                    NVARCHAR(50) NULL,
    POSId                   NVARCHAR(100) NULL,
    ApiBaseUrl              NVARCHAR(500) NULL,
    CredentialsReference    NVARCHAR(500) NULL,
    Environment             NVARCHAR(30) NULL,
    IsEnabled               BIT NOT NULL CONSTRAINT DF_FiscalConfig_IsEnabled DEFAULT (0),
    CONSTRAINT PK_FiscalConfigurations PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_FiscalConfig_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_FiscalConfig_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id)
);
GO

IF OBJECT_ID(N'dbo.FiscalDocuments', N'U') IS NULL
CREATE TABLE dbo.FiscalDocuments (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    SaleId                  BIGINT NOT NULL,
    Provider                NVARCHAR(100) NOT NULL,
    DocumentType            NVARCHAR(50) NOT NULL,
    InternalInvoiceNumber   NVARCHAR(100) NULL,
    ExternalInvoiceNumber   NVARCHAR(100) NULL,
    FBRInvoiceNumber        NVARCHAR(100) NULL,
    SubmissionStatus        NVARCHAR(50) NOT NULL,
    SubmittedAt             DATETIME2 NULL,
    ResponseAt              DATETIME2 NULL,
    QRData                  NVARCHAR(MAX) NULL,
    VerificationUrl         NVARCHAR(1000) NULL,
    RawRequest              NVARCHAR(MAX) NULL,
    RawResponse             NVARCHAR(MAX) NULL,
    CONSTRAINT PK_FiscalDocuments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_FiscalDocuments_Sales FOREIGN KEY (SaleId) REFERENCES dbo.Sales(Id),
    CONSTRAINT CK_FiscalDocuments_Status CHECK (SubmissionStatus IN (
        N'Pending', N'Submitted', N'Accepted', N'Rejected', N'Failed', N'Retrying'
    ))
);
GO

IF OBJECT_ID(N'dbo.FiscalDocumentLines', N'U') IS NULL
CREATE TABLE dbo.FiscalDocumentLines (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    FiscalDocumentId    BIGINT NOT NULL,
    SaleLineId          BIGINT NOT NULL,
    TaxableAmount       DECIMAL(19,4) NOT NULL,
    TaxRate             DECIMAL(9,4) NOT NULL,
    TaxAmount           DECIMAL(19,4) NOT NULL,
    CONSTRAINT PK_FiscalDocumentLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_FDL_Documents FOREIGN KEY (FiscalDocumentId) REFERENCES dbo.FiscalDocuments(Id),
    CONSTRAINT FK_FDL_SaleLines FOREIGN KEY (SaleLineId) REFERENCES dbo.SaleLines(Id),
    CONSTRAINT CK_FDL_Amounts CHECK (TaxableAmount >= 0 AND TaxRate >= 0 AND TaxAmount >= 0)
);
GO

IF OBJECT_ID(N'dbo.FiscalSubmissions', N'U') IS NULL
CREATE TABLE dbo.FiscalSubmissions (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    FiscalDocumentId    BIGINT NOT NULL,
    AttemptNo           INT NOT NULL,
    RequestId           NVARCHAR(150) NULL,
    Status              NVARCHAR(50) NOT NULL,
    HttpStatusCode      INT NULL,
    RequestPayload      NVARCHAR(MAX) NULL,
    ResponsePayload     NVARCHAR(MAX) NULL,
    ErrorCode           NVARCHAR(100) NULL,
    ErrorMessage        NVARCHAR(2000) NULL,
    SubmittedAt         DATETIME2 NOT NULL,
    CONSTRAINT PK_FiscalSubmissions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_FS_Documents FOREIGN KEY (FiscalDocumentId) REFERENCES dbo.FiscalDocuments(Id),
    CONSTRAINT UQ_FS_Document_Attempt UNIQUE (FiscalDocumentId, AttemptNo),
    CONSTRAINT CK_FS_AttemptNo CHECK (AttemptNo > 0),
    CONSTRAINT CK_FS_Status CHECK (Status IN (N'Success', N'Failed', N'Timeout', N'Rejected'))
);
GO

IF OBJECT_ID(N'dbo.IntegrationLogs', N'U') IS NULL
CREATE TABLE dbo.IntegrationLogs (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    IntegrationType     NVARCHAR(50) NOT NULL,
    ReferenceType       NVARCHAR(100) NULL,
    ReferenceId         BIGINT NULL,
    Request             NVARCHAR(MAX) NULL,
    Response            NVARCHAR(MAX) NULL,
    Status              NVARCHAR(50) NOT NULL,
    ErrorMessage        NVARCHAR(2000) NULL,
    CreatedAt           DATETIME2 NOT NULL CONSTRAINT DF_IntegrationLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_IntegrationLogs PRIMARY KEY CLUSTERED (Id)
);
GO
