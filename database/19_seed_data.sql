
USE PharmacyManagement;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

SET NOCOUNT ON;

/* Units: Tablet / Strip / Box */
IF NOT EXISTS (SELECT 1 FROM dbo.Units WHERE ShortCode = N'TAB')
    INSERT INTO dbo.Units (Name, ShortCode, UnitType, DecimalAllowed)
    VALUES (N'Tablet', N'TAB', N'Discrete', 0);
IF NOT EXISTS (SELECT 1 FROM dbo.Units WHERE ShortCode = N'STP')
    INSERT INTO dbo.Units (Name, ShortCode, UnitType, DecimalAllowed)
    VALUES (N'Strip', N'STP', N'Pack', 0);
IF NOT EXISTS (SELECT 1 FROM dbo.Units WHERE ShortCode = N'BOX')
    INSERT INTO dbo.Units (Name, ShortCode, UnitType, DecimalAllowed)
    VALUES (N'Box', N'BOX', N'Pack', 0);
GO

/* Payment methods */
IF NOT EXISTS (SELECT 1 FROM dbo.PaymentMethods WHERE Code = N'CASH')
    INSERT INTO dbo.PaymentMethods (Name, Code, Type, IsActive) VALUES (N'Cash', N'CASH', N'Cash', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.PaymentMethods WHERE Code = N'CARD')
    INSERT INTO dbo.PaymentMethods (Name, Code, Type, IsActive) VALUES (N'Card', N'CARD', N'Card', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.PaymentMethods WHERE Code = N'BANK')
    INSERT INTO dbo.PaymentMethods (Name, Code, Type, IsActive) VALUES (N'Bank Transfer', N'BANK', N'BankTransfer', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.PaymentMethods WHERE Code = N'CREDIT')
    INSERT INTO dbo.PaymentMethods (Name, Code, Type, IsActive) VALUES (N'Customer Credit', N'CREDIT', N'Credit', 1);
GO

/* Account types */
IF NOT EXISTS (SELECT 1 FROM dbo.AccountTypes WHERE Name = N'Asset')
    INSERT INTO dbo.AccountTypes (Name) VALUES (N'Asset'), (N'Liability'), (N'Equity'), (N'Revenue'), (N'Expense');
GO

/* Device types */
IF NOT EXISTS (SELECT 1 FROM dbo.DeviceTypes WHERE Code = N'PRINTER')
    INSERT INTO dbo.DeviceTypes (Code, Name) VALUES
        (N'PRINTER', N'Receipt Printer'),
        (N'SCANNER', N'Barcode Scanner'),
        (N'CDRAW', N'Cash Drawer'),
        (N'DISPLAY', N'Customer Display'),
        (N'SCALE', N'Scale');
GO

/* Permissions (module-coded) */
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Code = N'ORG.VIEW')
INSERT INTO dbo.Permissions (Code, Name, Module, Description) VALUES
 (N'ORG.VIEW', N'View Organization', N'Organization', N'View tenants and branches'),
 (N'ORG.EDIT', N'Edit Organization', N'Organization', N'Manage branches and warehouses'),
 (N'SEC.USERS', N'Manage Users', N'Security', N'Create and update users'),
 (N'SEC.ROLES', N'Manage Roles', N'Security', N'Assign roles and permissions'),
 (N'PROD.VIEW', N'View Products', N'Product', N'View product master'),
 (N'PROD.EDIT', N'Edit Products', N'Product', N'Create and update products'),
 (N'INV.VIEW', N'View Inventory', N'Inventory', N'View stock and batches'),
 (N'INV.ADJUST', N'Adjust Inventory', N'Inventory', N'Stock adjustments and counts'),
 (N'INV.TRANSFER', N'Transfer Stock', N'Inventory', N'Inter-warehouse transfers'),
 (N'PROC.PO', N'Purchase Orders', N'Procurement', N'Create and approve POs'),
 (N'PROC.GRN', N'Goods Receipts', N'Procurement', N'Receive goods against POs'),
 (N'PROC.SUPPLIER_PAY', N'Supplier Payments', N'Procurement', N'Record supplier payments and view supplier ledger'),
 (N'PROC.SUPPLIER_RETURN', N'Supplier Returns', N'Procurement', N'Draft, post, and cancel supplier returns'),
 (N'POS.SALE', N'POS Sale', N'POS', N'Create sales invoices'),
 (N'POS.RETURN', N'POS Return', N'POS', N'Process sale returns'),
 (N'POS.HOLD', N'Hold Sales', N'POS', N'Hold and resume carts'),
 (N'POS.VOID', N'Void Sales', N'POS', N'Void completed sales'),
 (N'CUST.VIEW', N'View Customers', N'Customer', N'View customers and AR ledger'),
 (N'CUST.EDIT', N'Edit Customers', N'Customer', N'Create customers and record AR payments'),
 (N'RX.DISPENSE', N'Dispense Prescription', N'Prescription', N'Dispense Rx items'),
 (N'CTRL.MANAGE', N'Controlled Drugs', N'Controlled', N'Manage controlled registers'),
 (N'FISCAL.SUBMIT', N'Submit Fiscal', N'Fiscal', N'Create and submit FBR/fiscal documents'),
 (N'FIN.JOURNAL', N'Post Journals', N'Finance', N'Post journal entries'),
 (N'FIN.CASH', N'Cash Shift', N'Finance', N'Open and close cash shifts'),
 (N'FIN.EXPENSE', N'Manage Expenses', N'Finance', N'Create and list expenses and categories'),
 (N'RPT.VIEW', N'View Reports', N'Reports', N'Access operational reports');
GO

/* Idempotent add for databases seeded before Phase 2D customer permissions */
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Code = N'CUST.VIEW')
    INSERT INTO dbo.Permissions (Code, Name, Module, Description)
    VALUES (N'CUST.VIEW', N'View Customers', N'Customer', N'View customers and AR ledger');
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Code = N'CUST.EDIT')
    INSERT INTO dbo.Permissions (Code, Name, Module, Description)
    VALUES (N'CUST.EDIT', N'Edit Customers', N'Customer', N'Create customers and record AR payments');

/* Idempotent add for Phase 2E fiscal permission */
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Code = N'FISCAL.SUBMIT')
    INSERT INTO dbo.Permissions (Code, Name, Module, Description)
    VALUES (N'FISCAL.SUBMIT', N'Submit Fiscal', N'Fiscal', N'Create and submit FBR/fiscal documents');

/* Idempotent add for Phase 4 expense permission */
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Code = N'FIN.EXPENSE')
    INSERT INTO dbo.Permissions (Code, Name, Module, Description)
    VALUES (N'FIN.EXPENSE', N'Manage Expenses', N'Finance', N'Create and list expenses and categories');

/* Idempotent add for Phase 5 supplier AR permissions */
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Code = N'PROC.SUPPLIER_PAY')
    INSERT INTO dbo.Permissions (Code, Name, Module, Description)
    VALUES (N'PROC.SUPPLIER_PAY', N'Supplier Payments', N'Procurement', N'Record supplier payments and view supplier ledger');
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE Code = N'PROC.SUPPLIER_RETURN')
    INSERT INTO dbo.Permissions (Code, Name, Module, Description)
    VALUES (N'PROC.SUPPLIER_RETURN', N'Supplier Returns', N'Procurement', N'Draft, post, and cancel supplier returns');
GO

/* Minimal demo org for tests */
DECLARE @TenantId BIGINT, @BranchId BIGINT, @WhId BIGINT, @LocId BIGINT, @CounterId BIGINT, @TerminalId BIGINT, @UserId BIGINT, @RoleId BIGINT;

IF NOT EXISTS (SELECT 1 FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group')
BEGIN
    INSERT INTO dbo.Tenants (Name, LegalName, NTN, STRN, LicenseNo, Phone, Email, Address, IsActive)
    VALUES (N'Demo Pharmacy Group', N'Demo Pharmacy Group Pvt Ltd', N'1234567-8', N'STRN-DEMO', N'DL-001',
            N'+92-21-0000000', N'demo@pharmacy.local', N'Karachi, Pakistan', 1);
END
SELECT @TenantId = Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group';

IF NOT EXISTS (SELECT 1 FROM dbo.Branches WHERE TenantId = @TenantId AND Code = N'MAIN')
BEGIN
    INSERT INTO dbo.Branches (TenantId, Code, Name, BranchType, City, Province, IsActive)
    VALUES (@TenantId, N'MAIN', N'Main Branch', N'Retail', N'Karachi', N'Sindh', 1);
END
SELECT @BranchId = Id FROM dbo.Branches WHERE TenantId = @TenantId AND Code = N'MAIN';

IF NOT EXISTS (SELECT 1 FROM dbo.Warehouses WHERE BranchId = @BranchId AND Code = N'MAIN-WH')
BEGIN
    INSERT INTO dbo.Warehouses (BranchId, Code, Name, WarehouseType, IsMain, TemperatureControlled, IsActive)
    VALUES (@BranchId, N'MAIN-WH', N'Main Warehouse', N'Store', 1, 1, 1);
END
SELECT @WhId = Id FROM dbo.Warehouses WHERE BranchId = @BranchId AND Code = N'MAIN-WH';

IF NOT EXISTS (SELECT 1 FROM dbo.WarehouseLocations WHERE WarehouseId = @WhId AND Code = N'A-01')
BEGIN
    INSERT INTO dbo.WarehouseLocations (WarehouseId, Code, Name, RackNo, ShelfNo, BinNo, LocationType, IsActive)
    VALUES (@WhId, N'A-01', N'Rack A Shelf 1', N'A', N'01', N'01', N'Selling', 1);
END
SELECT @LocId = Id FROM dbo.WarehouseLocations WHERE WarehouseId = @WhId AND Code = N'A-01';

IF NOT EXISTS (SELECT 1 FROM dbo.WarehouseLocations WHERE WarehouseId = @WhId AND Code = N'Q-01')
BEGIN
    INSERT INTO dbo.WarehouseLocations (WarehouseId, Code, Name, LocationType, IsActive)
    VALUES (@WhId, N'Q-01', N'Quarantine Bin', N'Quarantine', 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.Counters WHERE BranchId = @BranchId AND Code = N'C1')
BEGIN
    INSERT INTO dbo.Counters (BranchId, Code, Name, CounterType, IsActive)
    VALUES (@BranchId, N'C1', N'Counter 1', N'POS', 1);
END
SELECT @CounterId = Id FROM dbo.Counters WHERE BranchId = @BranchId AND Code = N'C1';

IF NOT EXISTS (SELECT 1 FROM dbo.POSTerminals WHERE BranchId = @BranchId AND TerminalCode = N'T1')
BEGIN
    INSERT INTO dbo.POSTerminals (BranchId, CounterId, TerminalCode, ComputerName, IsPrimary, IsOnline, IsActive)
    VALUES (@BranchId, @CounterId, N'T1', N'DEMO-POS-01', 1, 1, 1);
END
SELECT @TerminalId = Id FROM dbo.POSTerminals WHERE BranchId = @BranchId AND TerminalCode = N'T1';

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE TenantId = @TenantId AND Username = N'admin')
BEGIN
    INSERT INTO dbo.Users (TenantId, Username, Email, PasswordHash, FullName, EmployeeCode, IsActive)
    VALUES (@TenantId, N'admin', N'admin@pharmacy.local', N'$2b$12$demo.hash.not.for.production', N'System Admin', N'EMP001', 1);
END
SELECT @UserId = Id FROM dbo.Users WHERE TenantId = @TenantId AND Username = N'admin';

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE TenantId = @TenantId AND Name = N'Administrator')
BEGIN
    INSERT INTO dbo.Roles (TenantId, Name, Description, IsSystemRole)
    VALUES (@TenantId, N'Administrator', N'Full access demo role', 1);
END
SELECT @RoleId = Id FROM dbo.Roles WHERE TenantId = @TenantId AND Name = N'Administrator';

IF NOT EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
    INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);

IF NOT EXISTS (SELECT 1 FROM dbo.UserBranches WHERE UserId = @UserId AND BranchId = @BranchId)
    INSERT INTO dbo.UserBranches (UserId, BranchId) VALUES (@UserId, @BranchId);

INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT @RoleId, p.Id
FROM dbo.Permissions p
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId = @RoleId AND rp.PermissionId = p.Id
);

/* Reason codes for demo tenant */
IF NOT EXISTS (SELECT 1 FROM dbo.ReasonCodes WHERE TenantId = @TenantId AND Code = N'DAMAGED')
INSERT INTO dbo.ReasonCodes (TenantId, ReasonType, Code, Name, Description, IsActive) VALUES
 (@TenantId, N'Return', N'DAMAGED', N'Damaged Goods', N'Product damaged on return', 1),
 (@TenantId, N'Return', N'WRONG', N'Wrong Item', N'Wrong item sold', 1),
 (@TenantId, N'Return', N'CUSTOMER', N'Customer Request', N'Customer changed mind', 1),
 (@TenantId, N'Adjustment', N'SHRINK', N'Shrinkage', N'Unexplained loss', 1),
 (@TenantId, N'Adjustment', N'FOUND', N'Stock Found', N'Found inventory', 1),
 (@TenantId, N'Count', N'VARIANCE', N'Count Variance', N'Physical count variance', 1),
 (@TenantId, N'Void', N'CASHIER', N'Cashier Error', N'Void due to cashier error', 1);

/* Number sequences */
IF NOT EXISTS (SELECT 1 FROM dbo.NumberSequences WHERE TenantId = @TenantId AND DocumentType = N'SALE' AND TerminalId = @TerminalId)
    INSERT INTO dbo.NumberSequences (TenantId, BranchId, TerminalId, DocumentType, Prefix, CurrentNumber, NumberLength, ResetPeriod)
    VALUES (@TenantId, @BranchId, @TerminalId, N'SALE', N'INV-', 0, 6, N'Never');

IF NOT EXISTS (SELECT 1 FROM dbo.NumberSequences WHERE TenantId = @TenantId AND DocumentType = N'PO' AND BranchId = @BranchId AND TerminalId IS NULL)
    INSERT INTO dbo.NumberSequences (TenantId, BranchId, TerminalId, DocumentType, Prefix, CurrentNumber, NumberLength, ResetPeriod)
    VALUES (@TenantId, @BranchId, NULL, N'PO', N'PO-', 0, 6, N'Never');

IF NOT EXISTS (SELECT 1 FROM dbo.NumberSequences WHERE TenantId = @TenantId AND DocumentType = N'GRN' AND BranchId = @BranchId AND TerminalId IS NULL)
    INSERT INTO dbo.NumberSequences (TenantId, BranchId, TerminalId, DocumentType, Prefix, CurrentNumber, NumberLength, ResetPeriod)
    VALUES (@TenantId, @BranchId, NULL, N'GRN', N'GRN-', 0, 6, N'Never');

IF NOT EXISTS (SELECT 1 FROM dbo.NumberSequences WHERE TenantId = @TenantId AND DocumentType = N'PRESCRIPTION' AND BranchId IS NULL AND TerminalId IS NULL)
    INSERT INTO dbo.NumberSequences (TenantId, BranchId, TerminalId, DocumentType, Prefix, CurrentNumber, NumberLength, ResetPeriod)
    VALUES (@TenantId, NULL, NULL, N'PRESCRIPTION', N'RX-', 0, 6, N'Never');

IF NOT EXISTS (SELECT 1 FROM dbo.NumberSequences WHERE TenantId = @TenantId AND DocumentType = N'CTRL_REGISTER' AND BranchId = @BranchId AND TerminalId IS NULL)
    INSERT INTO dbo.NumberSequences (TenantId, BranchId, TerminalId, DocumentType, Prefix, CurrentNumber, NumberLength, ResetPeriod)
    VALUES (@TenantId, @BranchId, NULL, N'CTRL_REGISTER', N'CDR-', 0, 6, N'Never');

IF NOT EXISTS (SELECT 1 FROM dbo.NumberSequences WHERE TenantId = @TenantId AND DocumentType = N'EXPENSE' AND BranchId = @BranchId AND TerminalId IS NULL)
    INSERT INTO dbo.NumberSequences (TenantId, BranchId, TerminalId, DocumentType, Prefix, CurrentNumber, NumberLength, ResetPeriod)
    VALUES (@TenantId, @BranchId, NULL, N'EXPENSE', N'EXP-', 0, 6, N'Never');

IF NOT EXISTS (SELECT 1 FROM dbo.NumberSequences WHERE TenantId = @TenantId AND DocumentType = N'SUPPLIER_RETURN' AND BranchId = @BranchId AND TerminalId IS NULL)
    INSERT INTO dbo.NumberSequences (TenantId, BranchId, TerminalId, DocumentType, Prefix, CurrentNumber, NumberLength, ResetPeriod)
    VALUES (@TenantId, @BranchId, NULL, N'SUPPLIER_RETURN', N'SR-', 0, 6, N'Never');

PRINT N'Seed data applied.';
GO
