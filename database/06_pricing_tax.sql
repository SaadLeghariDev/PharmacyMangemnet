
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.PriceLists', N'U') IS NULL
CREATE TABLE dbo.PriceLists (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TenantId        BIGINT NOT NULL,
    Name            NVARCHAR(150) NOT NULL,
    PriceType       NVARCHAR(50) NULL,
    CurrencyCode    CHAR(3) NOT NULL CONSTRAINT DF_PriceLists_Currency DEFAULT ('PKR'),
    IsDefault       BIT NOT NULL CONSTRAINT DF_PriceLists_IsDefault DEFAULT (0),
    IsActive        BIT NOT NULL CONSTRAINT DF_PriceLists_IsActive DEFAULT (1),
    CONSTRAINT PK_PriceLists PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_PriceLists_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)
);
GO

IF OBJECT_ID(N'dbo.ProductPrices', N'U') IS NULL
CREATE TABLE dbo.ProductPrices (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    PriceListId         BIGINT NOT NULL,
    ProductId           BIGINT NOT NULL,
    ProductUnitId       BIGINT NOT NULL,
    PurchasePrice       DECIMAL(19,4) NOT NULL,
    SalePrice           DECIMAL(19,4) NOT NULL,
    MRP                 DECIMAL(19,4) NOT NULL,
    DiscountPercent     DECIMAL(9,4) NOT NULL CONSTRAINT DF_ProductPrices_Discount DEFAULT (0),
    EffectiveFrom       DATETIME2 NOT NULL,
    EffectiveTo         DATETIME2 NULL,
    CONSTRAINT PK_ProductPrices PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_ProductPrices_PriceLists FOREIGN KEY (PriceListId) REFERENCES dbo.PriceLists(Id),
    CONSTRAINT FK_ProductPrices_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_ProductPrices_ProductUnits FOREIGN KEY (ProductUnitId) REFERENCES dbo.ProductUnits(Id),
    CONSTRAINT CK_ProductPrices_Purchase CHECK (PurchasePrice >= 0),
    CONSTRAINT CK_ProductPrices_Sale CHECK (SalePrice >= 0),
    CONSTRAINT CK_ProductPrices_MRP CHECK (MRP >= 0),
    CONSTRAINT CK_ProductPrices_Discount CHECK (DiscountPercent >= 0 AND DiscountPercent <= 100)
);
GO

IF OBJECT_ID(N'dbo.TaxProfiles', N'U') IS NULL
CREATE TABLE dbo.TaxProfiles (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(150) NOT NULL,
    TaxType         NVARCHAR(50) NULL,
    Description     NVARCHAR(500) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_TaxProfiles_IsActive DEFAULT (1),
    CONSTRAINT PK_TaxProfiles PRIMARY KEY CLUSTERED (Id)
);
GO

IF OBJECT_ID(N'dbo.TaxRates', N'U') IS NULL
CREATE TABLE dbo.TaxRates (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    TaxProfileId    BIGINT NOT NULL,
    Rate            DECIMAL(9,4) NOT NULL,
    EffectiveFrom   DATE NOT NULL,
    EffectiveTo     DATE NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_TaxRates_IsActive DEFAULT (1),
    CONSTRAINT PK_TaxRates PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_TaxRates_TaxProfiles FOREIGN KEY (TaxProfileId) REFERENCES dbo.TaxProfiles(Id),
    CONSTRAINT CK_TaxRates_Rate CHECK (Rate >= 0)
);
GO

IF OBJECT_ID(N'dbo.ProductTaxProfiles', N'U') IS NULL
CREATE TABLE dbo.ProductTaxProfiles (
    ProductId       BIGINT NOT NULL,
    TaxProfileId    BIGINT NOT NULL,
    CONSTRAINT PK_ProductTaxProfiles PRIMARY KEY CLUSTERED (ProductId, TaxProfileId),
    CONSTRAINT FK_ProductTaxProfiles_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_ProductTaxProfiles_TaxProfiles FOREIGN KEY (TaxProfileId) REFERENCES dbo.TaxProfiles(Id)
);
GO

/* InvoiceTaxes FKs to Sales/SaleLines deferred to 18_constraints_indexes.sql */
IF OBJECT_ID(N'dbo.InvoiceTaxes', N'U') IS NULL
CREATE TABLE dbo.InvoiceTaxes (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    SaleId          BIGINT NOT NULL,
    SaleLineId      BIGINT NULL,
    TaxProfileId    BIGINT NOT NULL,
    TaxRate         DECIMAL(9,4) NOT NULL,
    TaxableAmount   DECIMAL(19,4) NOT NULL,
    TaxAmount       DECIMAL(19,4) NOT NULL,
    CONSTRAINT PK_InvoiceTaxes PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_InvoiceTaxes_TaxProfiles FOREIGN KEY (TaxProfileId) REFERENCES dbo.TaxProfiles(Id),
    CONSTRAINT CK_InvoiceTaxes_TaxRate CHECK (TaxRate >= 0),
    CONSTRAINT CK_InvoiceTaxes_Taxable CHECK (TaxableAmount >= 0),
    CONSTRAINT CK_InvoiceTaxes_TaxAmount CHECK (TaxAmount >= 0)
);
GO
