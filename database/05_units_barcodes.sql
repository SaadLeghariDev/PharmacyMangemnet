
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Units', N'U') IS NULL
CREATE TABLE dbo.Units (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(100) NOT NULL,
    ShortCode       NVARCHAR(30) NOT NULL,
    UnitType        NVARCHAR(50) NULL,
    DecimalAllowed  BIT NOT NULL CONSTRAINT DF_Units_DecimalAllowed DEFAULT (0),
    CONSTRAINT PK_Units PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Units_ShortCode UNIQUE (ShortCode)
);
GO

IF OBJECT_ID(N'dbo.ProductUnits', N'U') IS NULL
CREATE TABLE dbo.ProductUnits (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    ProductId           BIGINT NOT NULL,
    UnitId              BIGINT NOT NULL,
    IsBaseUnit          BIT NOT NULL CONSTRAINT DF_ProductUnits_IsBaseUnit DEFAULT (0),
    IsPurchaseUnit      BIT NOT NULL CONSTRAINT DF_ProductUnits_IsPurchaseUnit DEFAULT (0),
    IsSaleUnit          BIT NOT NULL CONSTRAINT DF_ProductUnits_IsSaleUnit DEFAULT (0),
    ConversionToBase    DECIMAL(19,6) NOT NULL,
    IsActive            BIT NOT NULL CONSTRAINT DF_ProductUnits_IsActive DEFAULT (1),
    CONSTRAINT PK_ProductUnits PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_ProductUnits_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_ProductUnits_Units FOREIGN KEY (UnitId) REFERENCES dbo.Units(Id),
    CONSTRAINT CK_ProductUnits_Conversion CHECK (ConversionToBase > 0),
    CONSTRAINT UQ_ProductUnits_Product_Unit UNIQUE (ProductId, UnitId)
);
GO

IF OBJECT_ID(N'dbo.UnitConversions', N'U') IS NULL
CREATE TABLE dbo.UnitConversions (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    ProductId           BIGINT NOT NULL,
    FromUnitId          BIGINT NOT NULL,
    ToUnitId            BIGINT NOT NULL,
    ConversionFactor    DECIMAL(19,6) NOT NULL,
    IsExact             BIT NOT NULL CONSTRAINT DF_UnitConversions_IsExact DEFAULT (1),
    CONSTRAINT PK_UnitConversions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_UnitConversions_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_UnitConversions_FromUnit FOREIGN KEY (FromUnitId) REFERENCES dbo.Units(Id),
    CONSTRAINT FK_UnitConversions_ToUnit FOREIGN KEY (ToUnitId) REFERENCES dbo.Units(Id),
    CONSTRAINT CK_UnitConversions_Factor CHECK (ConversionFactor > 0)
);
GO

IF OBJECT_ID(N'dbo.Barcodes', N'U') IS NULL
CREATE TABLE dbo.Barcodes (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    ProductId       BIGINT NOT NULL,
    ProductUnitId   BIGINT NOT NULL,
    BarcodeValue    NVARCHAR(150) NOT NULL,
    BarcodeType     NVARCHAR(50) NULL,
    GTIN            NVARCHAR(50) NULL,
    SerialNumber    NVARCHAR(100) NULL,
    IsPrimary       BIT NOT NULL CONSTRAINT DF_Barcodes_IsPrimary DEFAULT (0),
    IsActive        BIT NOT NULL CONSTRAINT DF_Barcodes_IsActive DEFAULT (1),
    CONSTRAINT PK_Barcodes PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Barcodes_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_Barcodes_ProductUnits FOREIGN KEY (ProductUnitId) REFERENCES dbo.ProductUnits(Id),
    CONSTRAINT UQ_Barcodes_BarcodeValue UNIQUE (BarcodeValue)
);
GO
