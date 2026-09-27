
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Manufacturers', N'U') IS NULL
CREATE TABLE dbo.Manufacturers (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(200) NOT NULL,
    Code            NVARCHAR(50) NULL,
    LicenseNo       NVARCHAR(100) NULL,
    Country         NVARCHAR(100) NULL,
    Phone           NVARCHAR(50) NULL,
    Email           NVARCHAR(200) NULL,
    Address         NVARCHAR(500) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Manufacturers_IsActive DEFAULT (1),
    CONSTRAINT PK_Manufacturers PRIMARY KEY CLUSTERED (Id)
);
GO

IF OBJECT_ID(N'dbo.Brands', N'U') IS NULL
CREATE TABLE dbo.Brands (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    ManufacturerId  BIGINT NOT NULL,
    Name            NVARCHAR(200) NOT NULL,
    Code            NVARCHAR(50) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Brands_IsActive DEFAULT (1),
    CONSTRAINT PK_Brands PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Brands_Manufacturers FOREIGN KEY (ManufacturerId) REFERENCES dbo.Manufacturers(Id)
);
GO

IF OBJECT_ID(N'dbo.ProductCategories', N'U') IS NULL
CREATE TABLE dbo.ProductCategories (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    ParentCategoryId    BIGINT NULL,
    Name                NVARCHAR(150) NOT NULL,
    Code                NVARCHAR(50) NULL,
    IsActive            BIT NOT NULL CONSTRAINT DF_ProductCategories_IsActive DEFAULT (1),
    CONSTRAINT PK_ProductCategories PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_ProductCategories_Parent FOREIGN KEY (ParentCategoryId) REFERENCES dbo.ProductCategories(Id)
);
GO

IF OBJECT_ID(N'dbo.TherapeuticClasses', N'U') IS NULL
CREATE TABLE dbo.TherapeuticClasses (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    Code            NVARCHAR(50) NULL,
    Name            NVARCHAR(150) NOT NULL,
    Description     NVARCHAR(500) NULL,
    CONSTRAINT PK_TherapeuticClasses PRIMARY KEY CLUSTERED (Id)
);
GO

IF OBJECT_ID(N'dbo.Ingredients', N'U') IS NULL
CREATE TABLE dbo.Ingredients (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(200) NOT NULL,
    GenericName     NVARCHAR(200) NULL,
    Code            NVARCHAR(50) NULL,
    Description     NVARCHAR(500) NULL,
    CONSTRAINT PK_Ingredients PRIMARY KEY CLUSTERED (Id)
);
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
CREATE TABLE dbo.Products (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    TenantId                BIGINT NOT NULL,
    CategoryId              BIGINT NOT NULL,
    ManufacturerId          BIGINT NOT NULL,
    BrandId                 BIGINT NOT NULL,
    TherapeuticClassId      BIGINT NOT NULL,
    SKU                     NVARCHAR(100) NOT NULL,
    ProductCode             NVARCHAR(100) NULL,
    Name                    NVARCHAR(250) NOT NULL,
    GenericName             NVARCHAR(250) NULL,
    Form                    NVARCHAR(100) NULL,
    Strength                NVARCHAR(100) NULL,
    StrengthUnit            NVARCHAR(50) NULL,
    PackDescription         NVARCHAR(250) NULL,
    PrescriptionRequired    BIT NOT NULL CONSTRAINT DF_Products_RxReq DEFAULT (0),
    IsControlled            BIT NOT NULL CONSTRAINT DF_Products_Controlled DEFAULT (0),
    IsTemperatureSensitive  BIT NOT NULL CONSTRAINT DF_Products_TempSens DEFAULT (0),
    IsRefrigerated          BIT NOT NULL CONSTRAINT DF_Products_Refrig DEFAULT (0),
    IsReturnable            BIT NOT NULL CONSTRAINT DF_Products_Returnable DEFAULT (1),
    IsSaleable              BIT NOT NULL CONSTRAINT DF_Products_Saleable DEFAULT (1),
    IsActive                BIT NOT NULL CONSTRAINT DF_Products_IsActive DEFAULT (1),
    CreatedAt               DATETIME2 NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt               DATETIME2 NOT NULL CONSTRAINT DF_Products_UpdatedAt DEFAULT (SYSUTCDATETIME()),
    RowVersion              ROWVERSION NOT NULL,
    CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Products_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.ProductCategories(Id),
    CONSTRAINT FK_Products_Manufacturers FOREIGN KEY (ManufacturerId) REFERENCES dbo.Manufacturers(Id),
    CONSTRAINT FK_Products_Brands FOREIGN KEY (BrandId) REFERENCES dbo.Brands(Id),
    CONSTRAINT FK_Products_TherapeuticClasses FOREIGN KEY (TherapeuticClassId) REFERENCES dbo.TherapeuticClasses(Id),
    CONSTRAINT UQ_Products_Tenant_SKU UNIQUE (TenantId, SKU)
);
GO

IF OBJECT_ID(N'dbo.ProductIngredients', N'U') IS NULL
CREATE TABLE dbo.ProductIngredients (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    ProductId       BIGINT NOT NULL,
    IngredientId    BIGINT NOT NULL,
    Strength        DECIMAL(19,6) NULL,
    StrengthUnit    NVARCHAR(50) NULL,
    Percentage      DECIMAL(19,6) NULL,
    IsPrimary       BIT NOT NULL CONSTRAINT DF_ProductIngredients_IsPrimary DEFAULT (0),
    CONSTRAINT PK_ProductIngredients PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_ProductIngredients_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_ProductIngredients_Ingredients FOREIGN KEY (IngredientId) REFERENCES dbo.Ingredients(Id),
    CONSTRAINT CK_ProductIngredients_Strength CHECK (Strength IS NULL OR Strength >= 0),
    CONSTRAINT CK_ProductIngredients_Percentage CHECK (Percentage IS NULL OR (Percentage >= 0 AND Percentage <= 100))
);
GO

IF OBJECT_ID(N'dbo.ProductAliases', N'U') IS NULL
CREATE TABLE dbo.ProductAliases (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    ProductId       BIGINT NOT NULL,
    Alias           NVARCHAR(250) NOT NULL,
    AliasType       NVARCHAR(50) NULL,
    CONSTRAINT PK_ProductAliases PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_ProductAliases_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
);
GO

IF OBJECT_ID(N'dbo.ProductRegulatoryProfiles', N'U') IS NULL
CREATE TABLE dbo.ProductRegulatoryProfiles (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    ProductId               BIGINT NOT NULL,
    DrugRegistrationNo      NVARCHAR(100) NULL,
    DrugLicenseCategory     NVARCHAR(100) NULL,
    ScheduleCode            NVARCHAR(50) NULL,
    ControlledDrugClass     NVARCHAR(100) NULL,
    RequiresPrescription    BIT NOT NULL CONSTRAINT DF_PRP_RequiresPrescription DEFAULT (0),
    RequiresSpecialRecord   BIT NOT NULL CONSTRAINT DF_PRP_RequiresSpecialRecord DEFAULT (0),
    StorageCondition        NVARCHAR(250) NULL,
    TemperatureMin          DECIMAL(10,2) NULL,
    TemperatureMax          DECIMAL(10,2) NULL,
    EffectiveFrom           DATE NULL,
    EffectiveTo             DATE NULL,
    IsActive                BIT NOT NULL CONSTRAINT DF_PRP_IsActive DEFAULT (1),
    CONSTRAINT PK_ProductRegulatoryProfiles PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_ProductRegulatoryProfiles_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
);
GO
