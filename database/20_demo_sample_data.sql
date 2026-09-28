/*
  20_demo_sample_data.sql
  ----------------------------------------------------------
  One-shot DEMO sample data for local testing (SSMS / sqlcmd).

  Prerequisite: run database/19_seed_data.sql first
    (Demo Pharmacy Group, Units TAB/STP/BOX, admin user, permissions).

  Safe to re-run: skips rows that already exist (IF NOT EXISTS / SKU checks).

  sqlcmd example:
    sqlcmd -S "DESKTOP-H9TF8EF\SQLEXPRESS" -E -C -I -d PharmacyManagement ^
      -i "F:\Pharmacymanagemnt\database\20_demo_sample_data.sql"
*/
USE PharmacyManagement;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ---------- Resolve demo org ---------- */
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @BranchId BIGINT;
DECLARE @WhId BIGINT;
DECLARE @LocId BIGINT;
DECLARE @UserId BIGINT;
DECLARE @RoleId BIGINT;
DECLARE @TabUnit BIGINT = (SELECT Id FROM dbo.Units WHERE ShortCode = N'TAB');
DECLARE @StpUnit BIGINT = (SELECT Id FROM dbo.Units WHERE ShortCode = N'STP');
DECLARE @BoxUnit BIGINT = (SELECT Id FROM dbo.Units WHERE ShortCode = N'BOX');
DECLARE @CashPm BIGINT = (SELECT Id FROM dbo.PaymentMethods WHERE Code = N'CASH');

IF @TenantId IS NULL
BEGIN
    RAISERROR(N'Demo tenant missing. Run database/19_seed_data.sql first.', 16, 1);
    RETURN;
END

SELECT @BranchId = Id FROM dbo.Branches WHERE TenantId = @TenantId AND Code = N'MAIN';
SELECT @WhId = Id FROM dbo.Warehouses WHERE BranchId = @BranchId AND Code = N'MAIN-WH';
SELECT @LocId = Id FROM dbo.WarehouseLocations WHERE WarehouseId = @WhId AND Code = N'A-01';
SELECT @UserId = Id FROM dbo.Users WHERE TenantId = @TenantId AND Username = N'admin';
SELECT @RoleId = Id FROM dbo.Roles WHERE TenantId = @TenantId AND Name = N'Administrator';

IF @BranchId IS NULL OR @WhId IS NULL OR @LocId IS NULL OR @UserId IS NULL OR @RoleId IS NULL
   OR @TabUnit IS NULL OR @StpUnit IS NULL OR @BoxUnit IS NULL
BEGIN
    RAISERROR(N'Demo org / units incomplete. Run database/19_seed_data.sql first.', 16, 1);
    RETURN;
END

/* ---------- Ensure Administrator has ALL permissions ---------- */
INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT @RoleId, p.Id
FROM dbo.Permissions p
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp
    WHERE rp.RoleId = @RoleId AND rp.PermissionId = p.Id
);

PRINT N'Administrator permissions synced: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' new links.';

/* ---------- Manufacturers / brands / categories / classes ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Manufacturers WHERE Code = N'DEMO-MFG')
    INSERT INTO dbo.Manufacturers (Name, Code, Country, IsActive)
    VALUES (N'Demo Pharma Ltd', N'DEMO-MFG', N'Pakistan', 1);

DECLARE @MfgId BIGINT = (SELECT Id FROM dbo.Manufacturers WHERE Code = N'DEMO-MFG');

IF NOT EXISTS (SELECT 1 FROM dbo.Brands WHERE ManufacturerId = @MfgId AND Code = N'DEMO-BR')
    INSERT INTO dbo.Brands (ManufacturerId, Name, Code, IsActive)
    VALUES (@MfgId, N'DemoCare', N'DEMO-BR', 1);

DECLARE @BrandId BIGINT = (SELECT Id FROM dbo.Brands WHERE ManufacturerId = @MfgId AND Code = N'DEMO-BR');

IF NOT EXISTS (SELECT 1 FROM dbo.ProductCategories WHERE Code = N'DEMO-ANAL')
    INSERT INTO dbo.ProductCategories (ParentCategoryId, Name, Code, IsActive)
    VALUES (NULL, N'Analgesics', N'DEMO-ANAL', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.ProductCategories WHERE Code = N'DEMO-AB')
    INSERT INTO dbo.ProductCategories (ParentCategoryId, Name, Code, IsActive)
    VALUES (NULL, N'Antibiotics', N'DEMO-AB', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.ProductCategories WHERE Code = N'DEMO-GI')
    INSERT INTO dbo.ProductCategories (ParentCategoryId, Name, Code, IsActive)
    VALUES (NULL, N'Gastro / OTC', N'DEMO-GI', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.ProductCategories WHERE Code = N'DEMO-VIT')
    INSERT INTO dbo.ProductCategories (ParentCategoryId, Name, Code, IsActive)
    VALUES (NULL, N'Vitamins', N'DEMO-VIT', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.TherapeuticClasses WHERE Code = N'DEMO-N02')
    INSERT INTO dbo.TherapeuticClasses (Code, Name, Description)
    VALUES (N'DEMO-N02', N'Analgesics demo', N'Demo class');
IF NOT EXISTS (SELECT 1 FROM dbo.TherapeuticClasses WHERE Code = N'DEMO-J01')
    INSERT INTO dbo.TherapeuticClasses (Code, Name, Description)
    VALUES (N'DEMO-J01', N'Antibacterials demo', N'Demo class');
IF NOT EXISTS (SELECT 1 FROM dbo.TherapeuticClasses WHERE Code = N'DEMO-A02')
    INSERT INTO dbo.TherapeuticClasses (Code, Name, Description)
    VALUES (N'DEMO-A02', N'Acid related demo', N'Demo class');
IF NOT EXISTS (SELECT 1 FROM dbo.TherapeuticClasses WHERE Code = N'DEMO-A11')
    INSERT INTO dbo.TherapeuticClasses (Code, Name, Description)
    VALUES (N'DEMO-A11', N'Vitamins demo', N'Demo class');

DECLARE @CatAnal BIGINT = (SELECT Id FROM dbo.ProductCategories WHERE Code = N'DEMO-ANAL');
DECLARE @CatAb BIGINT = (SELECT Id FROM dbo.ProductCategories WHERE Code = N'DEMO-AB');
DECLARE @CatGi BIGINT = (SELECT Id FROM dbo.ProductCategories WHERE Code = N'DEMO-GI');
DECLARE @CatVit BIGINT = (SELECT Id FROM dbo.ProductCategories WHERE Code = N'DEMO-VIT');
DECLARE @TcAnal BIGINT = (SELECT Id FROM dbo.TherapeuticClasses WHERE Code = N'DEMO-N02');
DECLARE @TcAb BIGINT = (SELECT Id FROM dbo.TherapeuticClasses WHERE Code = N'DEMO-J01');
DECLARE @TcGi BIGINT = (SELECT Id FROM dbo.TherapeuticClasses WHERE Code = N'DEMO-A02');
DECLARE @TcVit BIGINT = (SELECT Id FROM dbo.TherapeuticClasses WHERE Code = N'DEMO-A11');

/* ---------- Price list + tax ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.PriceLists WHERE TenantId = @TenantId AND Name = N'Demo Retail PKR')
    INSERT INTO dbo.PriceLists (TenantId, Name, PriceType, CurrencyCode, IsDefault, IsActive)
    VALUES (@TenantId, N'Demo Retail PKR', N'Retail', N'PKR', 1, 1);

DECLARE @PriceListId BIGINT = (SELECT Id FROM dbo.PriceLists WHERE TenantId = @TenantId AND Name = N'Demo Retail PKR');

IF NOT EXISTS (SELECT 1 FROM dbo.TaxProfiles WHERE Name = N'Demo GST 17%')
    INSERT INTO dbo.TaxProfiles (Name, TaxType, Description, IsActive)
    VALUES (N'Demo GST 17%', N'GST', N'Demo standard GST', 1);

DECLARE @TaxProfileId BIGINT = (SELECT Id FROM dbo.TaxProfiles WHERE Name = N'Demo GST 17%');

IF NOT EXISTS (SELECT 1 FROM dbo.TaxRates WHERE TaxProfileId = @TaxProfileId AND Rate = 17.0000)
    INSERT INTO dbo.TaxRates (TaxProfileId, Rate, EffectiveFrom, IsActive)
    VALUES (@TaxProfileId, 17.0000, '2020-01-01', 1);

/* ---------- Suppliers ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Suppliers WHERE TenantId = @TenantId AND Code = N'DEMO-SUP-01')
    INSERT INTO dbo.Suppliers (TenantId, Code, Name, CompanyName, Phone, CreditLimit, PaymentTermsDays, IsActive)
    VALUES (@TenantId, N'DEMO-SUP-01', N'Karachi Med Distributors', N'KMD Pvt Ltd', N'021-111000111', 500000, 30, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Suppliers WHERE TenantId = @TenantId AND Code = N'DEMO-SUP-02')
    INSERT INTO dbo.Suppliers (TenantId, Code, Name, CompanyName, Phone, CreditLimit, PaymentTermsDays, IsActive)
    VALUES (@TenantId, N'DEMO-SUP-02', N'Lahore Pharma Wholesale', N'LPW Traders', N'042-111000222', 300000, 15, 1);

DECLARE @SupplierId BIGINT = (SELECT Id FROM dbo.Suppliers WHERE TenantId = @TenantId AND Code = N'DEMO-SUP-01');

/* ---------- Customers ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE TenantId = @TenantId AND CustomerCode = N'CUST-001')
    INSERT INTO dbo.Customers (TenantId, CustomerCode, Name, Phone, Email, CreditLimit, IsPatient, IsActive)
    VALUES (@TenantId, N'CUST-001', N'Ali Khan', N'0300-1112233', N'ali@demo.local', 10000, 1, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE TenantId = @TenantId AND CustomerCode = N'CUST-002')
    INSERT INTO dbo.Customers (TenantId, CustomerCode, Name, Phone, CreditLimit, IsPatient, IsActive)
    VALUES (@TenantId, N'CUST-002', N'Sara Ahmed', N'0301-4445566', 5000, 1, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE TenantId = @TenantId AND CustomerCode = N'CUST-003')
    INSERT INTO dbo.Customers (TenantId, CustomerCode, Name, Phone, CreditLimit, IsPatient, IsActive)
    VALUES (@TenantId, N'CUST-003', N'Walk-in Cash', N'0302-7778899', 0, 0, 1);

/* ---------- Expense categories ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.ExpenseCategories WHERE Code = N'DEMO-RENT')
    INSERT INTO dbo.ExpenseCategories (Name, Code) VALUES (N'Rent', N'DEMO-RENT');
IF NOT EXISTS (SELECT 1 FROM dbo.ExpenseCategories WHERE Code = N'DEMO-UTIL')
    INSERT INTO dbo.ExpenseCategories (Name, Code) VALUES (N'Utilities', N'DEMO-UTIL');
IF NOT EXISTS (SELECT 1 FROM dbo.ExpenseCategories WHERE Code = N'DEMO-MISC')
    INSERT INTO dbo.ExpenseCategories (Name, Code) VALUES (N'Misc / Petty', N'DEMO-MISC');

/* ---------- Product catalog (SKU list) ---------- */
DECLARE @Products TABLE (
    Sku NVARCHAR(100) PRIMARY KEY,
    ProductCode NVARCHAR(100),
    Name NVARCHAR(250),
    GenericName NVARCHAR(250),
    Form NVARCHAR(100),
    Strength NVARCHAR(100),
    StrengthUnit NVARCHAR(50),
    CategoryId BIGINT,
    TherapeuticClassId BIGINT,
    PurchasePrice DECIMAL(19,4),
    SalePrice DECIMAL(19,4),
    Mrp DECIMAL(19,4),
    StockQty DECIMAL(19,6),
    NearExpiry BIT,
    BarcodeSuffix CHAR(4)
);

INSERT INTO @Products (Sku, ProductCode, Name, GenericName, Form, Strength, StrengthUnit, CategoryId, TherapeuticClassId, PurchasePrice, SalePrice, Mrp, StockQty, NearExpiry, BarcodeSuffix)
VALUES
 (N'DEMO-PARA-500',  N'PARA500',  N'Paracetamol 500mg Tab',     N'Paracetamol',     N'Tablet', N'500', N'mg', @CatAnal, @TcAnal, 1.50, 3.00, 4.00, 400, 0, N'0001'),
 (N'DEMO-IBU-400',   N'IBU400',   N'Ibuprofen 400mg Tab',       N'Ibuprofen',       N'Tablet', N'400', N'mg', @CatAnal, @TcAnal, 2.00, 4.50, 5.00, 250, 0, N'0002'),
 (N'DEMO-ASP-75',    N'ASP75',    N'Aspirin 75mg Tab',          N'Aspirin',         N'Tablet', N'75',  N'mg', @CatAnal, @TcAnal, 0.80, 2.00, 2.50, 300, 1, N'0003'),
 (N'DEMO-AMOX-500',  N'AMOX500',  N'Amoxicillin 500mg Cap',     N'Amoxicillin',     N'Capsule',N'500', N'mg', @CatAb,   @TcAb,   5.00, 10.00,12.00,180, 0, N'0004'),
 (N'DEMO-AZITH-250', N'AZI250',   N'Azithromycin 250mg Tab',    N'Azithromycin',    N'Tablet', N'250', N'mg', @CatAb,   @TcAb,   18.00,35.00,40.00,120, 0, N'0005'),
 (N'DEMO-CIPRO-500', N'CIP500',   N'Ciprofloxacin 500mg Tab',   N'Ciprofloxacin',   N'Tablet', N'500', N'mg', @CatAb,   @TcAb,   8.00, 16.00,18.00,150, 1, N'0006'),
 (N'DEMO-OMEP-20',   N'OME20',    N'Omeprazole 20mg Cap',       N'Omeprazole',      N'Capsule',N'20',  N'mg', @CatGi,   @TcGi,   3.00, 7.00, 8.00, 220, 0, N'0007'),
 (N'DEMO-RANI-150',  N'RAN150',   N'Ranitidine 150mg Tab',      N'Ranitidine',      N'Tablet', N'150', N'mg', @CatGi,   @TcGi,   1.20, 3.00, 3.50, 200, 0, N'0008'),
 (N'DEMO-ORS-1',     N'ORS1',     N'ORS Sachet',                N'ORS',             N'Sachet', N'1',   N'sach',@CatGi,   @TcGi,   8.00, 15.00,18.00,100, 0, N'0009'),
 (N'DEMO-VITC-500',  N'VITC500',  N'Vitamin C 500mg Tab',       N'Ascorbic acid',   N'Tablet', N'500', N'mg', @CatVit,  @TcVit,  2.50, 5.00, 6.00, 350, 0, N'0010'),
 (N'DEMO-VITD-1K',   N'VITD1K',   N'Vitamin D3 1000 IU',        N'Cholecalciferol', N'Tablet', N'1000',N'IU', @CatVit,  @TcVit,  4.00, 9.00, 10.00,160, 0, N'0011'),
 (N'DEMO-MULTI',     N'MULTI',    N'Multivitamin Adult Tab',    N'Multivitamin',    N'Tablet', N'1',   N'tab', @CatVit,  @TcVit,  3.50, 8.00, 9.00, 140, 1, N'0012'),
 (N'DEMO-CETIR-10',  N'CET10',    N'Cetirizine 10mg Tab',       N'Cetirizine',      N'Tablet', N'10',  N'mg', @CatAnal, @TcAnal, 1.00, 2.50, 3.00, 280, 0, N'0013'),
 (N'DEMO-METF-500',  N'METF500',  N'Metformin 500mg Tab',       N'Metformin',       N'Tablet', N'500', N'mg', @CatGi,   @TcGi,   1.80, 4.00, 5.00, 320, 0, N'0014'),
 (N'DEMO-LOSAR-50',  N'LOS50',    N'Losartan 50mg Tab',         N'Losartan',        N'Tablet', N'50',  N'mg', @CatGi,   @TcGi,   6.00, 12.00,14.00,110, 0, N'0015');

/* Insert products + units + barcode + price + tax */
DECLARE @Sku NVARCHAR(100), @PCode NVARCHAR(100), @PName NVARCHAR(250), @GName NVARCHAR(250),
        @Form NVARCHAR(100), @Str NVARCHAR(100), @StrU NVARCHAR(50),
        @CatId BIGINT, @TcId BIGINT, @Pur DECIMAL(19,4), @Sale DECIMAL(19,4), @Mrp DECIMAL(19,4),
        @Qty DECIMAL(19,6), @Near BIT, @BcSuffix CHAR(4);
DECLARE @ProdId BIGINT, @PuTab BIGINT, @PuStp BIGINT, @PuBox BIGINT, @Barcode NVARCHAR(30);

DECLARE prod_cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT Sku, ProductCode, Name, GenericName, Form, Strength, StrengthUnit,
           CategoryId, TherapeuticClassId, PurchasePrice, SalePrice, Mrp, StockQty, NearExpiry, BarcodeSuffix
    FROM @Products;

OPEN prod_cur;
FETCH NEXT FROM prod_cur INTO @Sku, @PCode, @PName, @GName, @Form, @Str, @StrU,
    @CatId, @TcId, @Pur, @Sale, @Mrp, @Qty, @Near, @BcSuffix;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE TenantId = @TenantId AND SKU = @Sku)
    BEGIN
        INSERT INTO dbo.Products (
            TenantId, CategoryId, ManufacturerId, BrandId, TherapeuticClassId,
            SKU, ProductCode, Name, GenericName, Form, Strength, StrengthUnit,
            PrescriptionRequired, IsControlled, IsSaleable, IsActive)
        VALUES (
            @TenantId, @CatId, @MfgId, @BrandId, @TcId,
            @Sku, @PCode, @PName, @GName, @Form, @Str, @StrU,
            0, 0, 1, 1);
    END

    SELECT @ProdId = Id FROM dbo.Products WHERE TenantId = @TenantId AND SKU = @Sku;

    IF NOT EXISTS (SELECT 1 FROM dbo.ProductUnits WHERE ProductId = @ProdId AND UnitId = @TabUnit)
        INSERT INTO dbo.ProductUnits (ProductId, UnitId, IsBaseUnit, IsPurchaseUnit, IsSaleUnit, ConversionToBase, IsActive)
        VALUES (@ProdId, @TabUnit, 1, 0, 1, 1, 1);
    IF NOT EXISTS (SELECT 1 FROM dbo.ProductUnits WHERE ProductId = @ProdId AND UnitId = @StpUnit)
        INSERT INTO dbo.ProductUnits (ProductId, UnitId, IsBaseUnit, IsPurchaseUnit, IsSaleUnit, ConversionToBase, IsActive)
        VALUES (@ProdId, @StpUnit, 0, 0, 1, 10, 1);
    IF NOT EXISTS (SELECT 1 FROM dbo.ProductUnits WHERE ProductId = @ProdId AND UnitId = @BoxUnit)
        INSERT INTO dbo.ProductUnits (ProductId, UnitId, IsBaseUnit, IsPurchaseUnit, IsSaleUnit, ConversionToBase, IsActive)
        VALUES (@ProdId, @BoxUnit, 0, 1, 0, 100, 1);

    SELECT @PuTab = Id FROM dbo.ProductUnits WHERE ProductId = @ProdId AND UnitId = @TabUnit;
    SELECT @PuStp = Id FROM dbo.ProductUnits WHERE ProductId = @ProdId AND UnitId = @StpUnit;
    SELECT @PuBox = Id FROM dbo.ProductUnits WHERE ProductId = @ProdId AND UnitId = @BoxUnit;

    IF NOT EXISTS (SELECT 1 FROM dbo.UnitConversions WHERE ProductId = @ProdId AND FromUnitId = @StpUnit AND ToUnitId = @TabUnit)
        INSERT INTO dbo.UnitConversions (ProductId, FromUnitId, ToUnitId, ConversionFactor, IsExact)
        VALUES (@ProdId, @StpUnit, @TabUnit, 10, 1);
    IF NOT EXISTS (SELECT 1 FROM dbo.UnitConversions WHERE ProductId = @ProdId AND FromUnitId = @BoxUnit AND ToUnitId = @TabUnit)
        INSERT INTO dbo.UnitConversions (ProductId, FromUnitId, ToUnitId, ConversionFactor, IsExact)
        VALUES (@ProdId, @BoxUnit, @TabUnit, 100, 1);

    SET @Barcode = N'6281999' + @BcSuffix;
    IF NOT EXISTS (SELECT 1 FROM dbo.Barcodes WHERE BarcodeValue = @Barcode)
        INSERT INTO dbo.Barcodes (ProductId, ProductUnitId, BarcodeValue, BarcodeType, IsPrimary, IsActive)
        VALUES (@ProdId, @PuTab, @Barcode, N'EAN13', 1, 1);

    IF NOT EXISTS (SELECT 1 FROM dbo.ProductPrices WHERE PriceListId = @PriceListId AND ProductId = @ProdId AND ProductUnitId = @PuTab)
        INSERT INTO dbo.ProductPrices (PriceListId, ProductId, ProductUnitId, PurchasePrice, SalePrice, MRP, DiscountPercent, EffectiveFrom)
        VALUES (@PriceListId, @ProdId, @PuTab, @Pur, @Sale, @Mrp, 0, SYSUTCDATETIME());

    IF NOT EXISTS (SELECT 1 FROM dbo.ProductTaxProfiles WHERE ProductId = @ProdId AND TaxProfileId = @TaxProfileId)
        INSERT INTO dbo.ProductTaxProfiles (ProductId, TaxProfileId)
        VALUES (@ProdId, @TaxProfileId);

    FETCH NEXT FROM prod_cur INTO @Sku, @PCode, @PName, @GName, @Form, @Str, @StrU,
        @CatId, @TcId, @Pur, @Sale, @Mrp, @Qty, @Near, @BcSuffix;
END
CLOSE prod_cur;
DEALLOCATE prod_cur;

PRINT N'Products / units / prices seeded.';

/* ---------- Stock via one demo GRN (required FK path) ---------- */
DECLARE @GrnNumber NVARCHAR(50) = N'DEMO-GRN-SAMPLE-001';
DECLARE @GrId BIGINT = (SELECT Id FROM dbo.GoodsReceipts WHERE BranchId = @BranchId AND GRNNumber = @GrnNumber);

IF @GrId IS NULL
BEGIN
    BEGIN TRAN;

    INSERT INTO dbo.GoodsReceipts (
        BranchId, WarehouseId, SupplierId, PurchaseOrderId, GRNNumber, ReceiptDate, Status,
        InvoiceNumber, Subtotal, DiscountAmount, TaxAmount, NetAmount, ReceivedBy)
    VALUES (
        @BranchId, @WhId, @SupplierId, NULL, @GrnNumber, SYSUTCDATETIME(), N'Posted',
        N'DEMO-INV-001', 0, 0, 0, 0, @UserId);
    SET @GrId = SCOPE_IDENTITY();

    DECLARE @LineSubtotal DECIMAL(19,4) = 0;
    DECLARE @GRLId BIGINT, @BatchId BIGINT, @BatchNo NVARCHAR(100), @Exp DATE, @Mfg DATE;
    DECLARE @UnitCost DECIMAL(19,4);

    DECLARE stock_cur CURSOR LOCAL FAST_FORWARD FOR
        SELECT p.Id, pr.StockQty, pr.PurchasePrice, pr.SalePrice, pr.Mrp, pr.NearExpiry, pu.Id
        FROM @Products pr
        JOIN dbo.Products p ON p.TenantId = @TenantId AND p.SKU = pr.Sku
        JOIN dbo.ProductUnits pu ON pu.ProductId = p.Id AND pu.IsBaseUnit = 1;

    DECLARE @StockProdId BIGINT, @StockQty DECIMAL(19,6), @SPurchase DECIMAL(19,4),
            @SSale DECIMAL(19,4), @SMrp DECIMAL(19,4), @SNear BIT, @SPu BIGINT;

    OPEN stock_cur;
    FETCH NEXT FROM stock_cur INTO @StockProdId, @StockQty, @SPurchase, @SSale, @SMrp, @SNear, @SPu;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF NOT EXISTS (
            SELECT 1 FROM dbo.InventoryBatches b
            WHERE b.ProductId = @StockProdId AND b.BatchNumber LIKE N'DEMO-B-%'
        )
        BEGIN
            SET @UnitCost = @SPurchase;
            SET @Mfg = DATEADD(MONTH, -6, CAST(SYSUTCDATETIME() AS DATE));
            SET @Exp = CASE WHEN @SNear = 1
                            THEN DATEADD(DAY, 45, CAST(SYSUTCDATETIME() AS DATE))
                            ELSE DATEADD(YEAR, 2, CAST(SYSUTCDATETIME() AS DATE)) END;
            SET @BatchNo = N'DEMO-B-' + CAST(@StockProdId AS NVARCHAR(20));

            INSERT INTO dbo.GoodsReceiptLines (
                GoodsReceiptId, ProductId, ProductUnitId, OrderedQuantity, ReceivedQuantity,
                FreeQuantity, UnitCost, DiscountAmount, TaxAmount, NetCost)
            VALUES (
                @GrId, @StockProdId, @SPu, @StockQty, @StockQty,
                0, @UnitCost, 0, 0, @UnitCost * @StockQty);
            SET @GRLId = SCOPE_IDENTITY();

            INSERT INTO dbo.GoodsReceiptLineBatches (
                GoodsReceiptLineId, BatchNumber, ManufacturingDate, ExpiryDate, MRP, SalePrice,
                Quantity, FreeQuantity, WarehouseLocationId)
            VALUES (
                @GRLId, @BatchNo, @Mfg, @Exp, @SMrp, @SSale,
                @StockQty, 0, @LocId);

            INSERT INTO dbo.InventoryBatches (
                ProductId, GoodsReceiptLineId, SupplierId, WarehouseId, BatchNumber,
                ManufacturingDate, ExpiryDate, QuantityReceived, FreeQuantity,
                PurchaseCost, MRP, SalePrice, BatchStatus, IsRecalled)
            VALUES (
                @StockProdId, @GRLId, @SupplierId, @WhId, @BatchNo,
                @Mfg, @Exp, @StockQty, 0,
                @UnitCost, @SMrp, @SSale, N'Available', 0);
            SET @BatchId = SCOPE_IDENTITY();

            INSERT INTO dbo.InventoryBatchLocations (BatchId, WarehouseLocationId, QuantityOnHand, ReservedQuantity)
            VALUES (@BatchId, @LocId, @StockQty, 0);

            INSERT INTO dbo.InventoryMovements (
                BranchId, WarehouseId, WarehouseLocationId, ProductId, BatchId, ProductUnitId,
                MovementType, ReferenceType, ReferenceId, Quantity, UnitCost, TotalCost,
                BalanceBefore, BalanceAfter, MovementDate, PerformedBy)
            VALUES (
                @BranchId, @WhId, @LocId, @StockProdId, @BatchId, @SPu,
                N'Receipt', N'GoodsReceipt', @GrId, @StockQty, @UnitCost, @UnitCost * @StockQty,
                0, @StockQty, SYSUTCDATETIME(), @UserId);

            SET @LineSubtotal += @UnitCost * @StockQty;
        END

        FETCH NEXT FROM stock_cur INTO @StockProdId, @StockQty, @SPurchase, @SSale, @SMrp, @SNear, @SPu;
    END
    CLOSE stock_cur;
    DEALLOCATE stock_cur;

    UPDATE dbo.GoodsReceipts
    SET Subtotal = @LineSubtotal, NetAmount = @LineSubtotal
    WHERE Id = @GrId;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.SupplierLedger
        WHERE SupplierId = @SupplierId AND ReferenceType = N'GoodsReceipt' AND ReferenceId = @GrId
    )
        INSERT INTO dbo.SupplierLedger (
            SupplierId, BranchId, TransactionDate, TransactionType, ReferenceType, ReferenceId,
            Debit, Credit, SequenceNo, Remarks)
        VALUES (
            @SupplierId, @BranchId, SYSUTCDATETIME(), N'Purchase', N'GoodsReceipt', @GrId,
            @LineSubtotal, 0, 1, N'Demo sample GRN');

    COMMIT;
    PRINT N'Demo GRN + stock batches created.';
END
ELSE
    PRINT N'Demo GRN already exists — stock insert skipped.';

/* ---------- Reorder rules (dashboard low-stock testing) ---------- */
INSERT INTO dbo.ReorderRules (
    BranchId, WarehouseId, ProductId, MinimumStock, MaximumStock, ReorderPoint, ReorderQuantity, PreferredSupplierId, IsActive)
SELECT @BranchId, @WhId, p.Id, 50, 500, 80, 100, @SupplierId, 1
FROM dbo.Products p
WHERE p.TenantId = @TenantId
  AND p.SKU IN (N'DEMO-ASP-75', N'DEMO-CIPRO-500', N'DEMO-MULTI', N'DEMO-PARA-500')
  AND NOT EXISTS (
      SELECT 1 FROM dbo.ReorderRules rr
      WHERE rr.BranchId = @BranchId AND rr.WarehouseId = @WhId AND rr.ProductId = p.Id
  );

/* ---------- Sample expense ---------- */
IF @CashPm IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.Expenses WHERE BranchId = @BranchId AND ExpenseNumber = N'DEMO-EXP-001')
BEGIN
    DECLARE @ExpCat BIGINT = (SELECT Id FROM dbo.ExpenseCategories WHERE Code = N'DEMO-UTIL');
    IF @ExpCat IS NOT NULL
        INSERT INTO dbo.Expenses (BranchId, CategoryId, ExpenseNumber, ExpenseDate, Amount, PaymentMethodId, Description, CreatedBy)
        VALUES (@BranchId, @ExpCat, N'DEMO-EXP-001', SYSUTCDATETIME(), 2500.00, @CashPm, N'Demo electricity bill', @UserId);
END

PRINT N'----- Demo sample data complete -----';
DECLARE @ProductCount INT = (SELECT COUNT(*) FROM dbo.Products WHERE TenantId = @TenantId AND SKU LIKE N'DEMO-%');
DECLARE @BatchCount INT = (SELECT COUNT(*) FROM dbo.InventoryBatches WHERE BatchNumber LIKE N'DEMO-B-%');
DECLARE @CustomerCount INT = (SELECT COUNT(*) FROM dbo.Customers WHERE TenantId = @TenantId AND CustomerCode LIKE N'CUST-%');
PRINT N'Products: ' + CAST(@ProductCount AS NVARCHAR(10));
PRINT N'Stock batches: ' + CAST(@BatchCount AS NVARCHAR(10));
PRINT N'Customers: ' + CAST(@CustomerCount AS NVARCHAR(10));
PRINT N'Re-login as admin / Admin@12345 so JWT picks up any new permissions.';
GO
