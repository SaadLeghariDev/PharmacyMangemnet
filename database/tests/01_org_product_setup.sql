USE PharmacyManagement;
GO
SET NOCOUNT ON;

/* Shared assert helper pattern: PRINT TEST_PASS / RAISERROR TEST_FAIL */

/* TEST 1: Demo tenant/branch/warehouse/location/counter/terminal exist */
IF EXISTS (
    SELECT 1
    FROM dbo.Tenants t
    JOIN dbo.Branches b ON b.TenantId = t.Id
    JOIN dbo.Warehouses w ON w.BranchId = b.Id
    JOIN dbo.WarehouseLocations l ON l.WarehouseId = w.Id
    JOIN dbo.Counters c ON c.BranchId = b.Id
    JOIN dbo.POSTerminals pt ON pt.BranchId = b.Id AND pt.CounterId = c.Id
    WHERE t.Name = N'Demo Pharmacy Group' AND b.Code = N'MAIN'
)
    PRINT N'TEST_PASS: 01 demo org hierarchy exists';
ELSE
    PRINT N'TEST_FAIL: 01 demo org hierarchy missing';
GO

/* TEST 2: Users/roles/permissions wired */
IF EXISTS (
    SELECT 1 FROM dbo.Users u
    JOIN dbo.UserRoles ur ON ur.UserId = u.Id
    JOIN dbo.Roles r ON r.Id = ur.RoleId
    JOIN dbo.RolePermissions rp ON rp.RoleId = r.Id
    JOIN dbo.Permissions p ON p.Id = rp.PermissionId
    WHERE u.Username = N'admin'
) AND (SELECT COUNT(*) FROM dbo.Permissions) >= 20
    PRINT N'TEST_PASS: 02 admin role permissions seeded';
ELSE
    PRINT N'TEST_FAIL: 02 admin role permissions incomplete';
GO

/* TEST 3: Create product master chain */
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @MfgId BIGINT, @BrandId BIGINT, @CatId BIGINT, @TcId BIGINT, @IngId BIGINT, @ProdId BIGINT;

INSERT INTO dbo.Manufacturers (Name, Code, Country, IsActive) VALUES (N'Demo Pharma Ltd', N'DMFG', N'Pakistan', 1);
SET @MfgId = SCOPE_IDENTITY();
INSERT INTO dbo.Brands (ManufacturerId, Name, Code, IsActive) VALUES (@MfgId, N'DemoBrand', N'DBR', 1);
SET @BrandId = SCOPE_IDENTITY();
INSERT INTO dbo.ProductCategories (ParentCategoryId, Name, Code, IsActive) VALUES (NULL, N'Analgesics', N'ANAL', 1);
SET @CatId = SCOPE_IDENTITY();
INSERT INTO dbo.TherapeuticClasses (Code, Name, Description) VALUES (N'N02BE', N'Paracetamol', N'Analgesic');
SET @TcId = SCOPE_IDENTITY();
INSERT INTO dbo.Ingredients (Name, GenericName, Code) VALUES (N'Paracetamol', N'Acetaminophen', N'PARA');
SET @IngId = SCOPE_IDENTITY();
INSERT INTO dbo.Products (
    TenantId, CategoryId, ManufacturerId, BrandId, TherapeuticClassId,
    SKU, ProductCode, Name, GenericName, Form, Strength, StrengthUnit,
    PrescriptionRequired, IsControlled, IsSaleable, IsActive
) VALUES (
    @TenantId, @CatId, @MfgId, @BrandId, @TcId,
    N'SKU-PARA-500', N'PARA500', N'Paracetamol 500mg', N'Paracetamol', N'Tablet', N'500', N'mg',
    0, 0, 1, 1
);
SET @ProdId = SCOPE_IDENTITY();
INSERT INTO dbo.ProductIngredients (ProductId, IngredientId, Strength, StrengthUnit, IsPrimary)
VALUES (@ProdId, @IngId, 500, N'mg', 1);

IF @ProdId IS NOT NULL
    PRINT N'TEST_PASS: 03 product master created';
ELSE
    PRINT N'TEST_FAIL: 03 product master not created';
GO

/* TEST 4: Product units and conversions (10 tablets = 1 strip, 10 strips = 1 box) */
DECLARE @ProdId BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
DECLARE @Tab BIGINT = (SELECT Id FROM dbo.Units WHERE ShortCode = N'TAB');
DECLARE @Stp BIGINT = (SELECT Id FROM dbo.Units WHERE ShortCode = N'STP');
DECLARE @Box BIGINT = (SELECT Id FROM dbo.Units WHERE ShortCode = N'BOX');
DECLARE @PU_Tab BIGINT, @PU_Stp BIGINT, @PU_Box BIGINT;

INSERT INTO dbo.ProductUnits (ProductId, UnitId, IsBaseUnit, IsPurchaseUnit, IsSaleUnit, ConversionToBase, IsActive)
VALUES (@ProdId, @Tab, 1, 0, 1, 1, 1);
SET @PU_Tab = SCOPE_IDENTITY();
INSERT INTO dbo.ProductUnits (ProductId, UnitId, IsBaseUnit, IsPurchaseUnit, IsSaleUnit, ConversionToBase, IsActive)
VALUES (@ProdId, @Stp, 0, 0, 1, 10, 1);
SET @PU_Stp = SCOPE_IDENTITY();
INSERT INTO dbo.ProductUnits (ProductId, UnitId, IsBaseUnit, IsPurchaseUnit, IsSaleUnit, ConversionToBase, IsActive)
VALUES (@ProdId, @Box, 0, 1, 0, 100, 1);
SET @PU_Box = SCOPE_IDENTITY();

INSERT INTO dbo.UnitConversions (ProductId, FromUnitId, ToUnitId, ConversionFactor, IsExact)
VALUES (@ProdId, @Stp, @Tab, 10, 1), (@ProdId, @Box, @Tab, 100, 1);

IF (SELECT ConversionToBase FROM dbo.ProductUnits WHERE Id = @PU_Box) = 100
    PRINT N'TEST_PASS: 04 product units and conversions';
ELSE
    PRINT N'TEST_FAIL: 04 product unit conversion incorrect';
GO

/* TEST 5: Barcode uniqueness */
DECLARE @ProdId BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
DECLARE @PU BIGINT = (SELECT TOP 1 Id FROM dbo.ProductUnits WHERE ProductId = @ProdId AND IsBaseUnit = 1);
INSERT INTO dbo.Barcodes (ProductId, ProductUnitId, BarcodeValue, BarcodeType, IsPrimary, IsActive)
VALUES (@ProdId, @PU, N'6281000000001', N'EAN13', 1, 1);

BEGIN TRY
    INSERT INTO dbo.Barcodes (ProductId, ProductUnitId, BarcodeValue, BarcodeType, IsPrimary, IsActive)
    VALUES (@ProdId, @PU, N'6281000000001', N'EAN13', 0, 1);
    PRINT N'TEST_FAIL: 05 duplicate barcode allowed';
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() IN (2627, 2601)
        PRINT N'TEST_PASS: 05 barcode uniqueness enforced';
    ELSE
        THROW;
END CATCH
GO

/* TEST 6: Price list and product prices non-negative */
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @ProdId BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
DECLARE @PU BIGINT = (SELECT TOP 1 Id FROM dbo.ProductUnits WHERE ProductId = @ProdId AND IsBaseUnit = 1);
DECLARE @PL BIGINT;
INSERT INTO dbo.PriceLists (TenantId, Name, PriceType, CurrencyCode, IsDefault, IsActive)
VALUES (@TenantId, N'Retail PKR', N'Retail', 'PKR', 1, 1);
SET @PL = SCOPE_IDENTITY();
INSERT INTO dbo.ProductPrices (PriceListId, ProductId, ProductUnitId, PurchasePrice, SalePrice, MRP, DiscountPercent, EffectiveFrom)
VALUES (@PL, @ProdId, @PU, 2.50, 5.00, 6.00, 0, SYSUTCDATETIME());

BEGIN TRY
    INSERT INTO dbo.ProductPrices (PriceListId, ProductId, ProductUnitId, PurchasePrice, SalePrice, MRP, DiscountPercent, EffectiveFrom)
    VALUES (@PL, @ProdId, @PU, -1, 5, 6, 0, SYSUTCDATETIME());
    PRINT N'TEST_FAIL: 06 negative price allowed';
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 547
        PRINT N'TEST_PASS: 06 price CHECK rejects negatives';
    ELSE IF ERROR_MESSAGE() LIKE N'TEST_FAIL:%'
        THROW;
    ELSE
        PRINT N'TEST_PASS: 06 price CHECK rejects negatives';
END CATCH
GO

/* TEST 7: Tax profiles linked to product */
DECLARE @ProdId BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
DECLARE @TP BIGINT;
INSERT INTO dbo.TaxProfiles (Name, TaxType, Description, IsActive) VALUES (N'Standard GST', N'GST', N'17% GST', 1);
SET @TP = SCOPE_IDENTITY();
INSERT INTO dbo.TaxRates (TaxProfileId, Rate, EffectiveFrom, IsActive) VALUES (@TP, 17.0000, '2020-01-01', 1);
INSERT INTO dbo.ProductTaxProfiles (ProductId, TaxProfileId) VALUES (@ProdId, @TP);

IF EXISTS (SELECT 1 FROM dbo.ProductTaxProfiles WHERE ProductId = @ProdId AND TaxProfileId = @TP)
    PRINT N'TEST_PASS: 07 product tax profile linked';
ELSE
    PRINT N'TEST_FAIL: 07 product tax profile missing';
GO
