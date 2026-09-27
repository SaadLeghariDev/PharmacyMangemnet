USE PharmacyManagement;
GO
SET NOCOUNT ON;

/* TEST 23: Prescription + dispensing linked to sale */
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE Code = N'MAIN');
DECLARE @CounterId BIGINT = (SELECT Id FROM dbo.Counters WHERE Code = N'C1');
DECLARE @TerminalId BIGINT = (SELECT Id FROM dbo.POSTerminals WHERE TerminalCode = N'T1');
DECLARE @UserId BIGINT = (SELECT Id FROM dbo.Users WHERE Username = N'admin');
DECLARE @ProdId BIGINT = (SELECT Id FROM dbo.Products WHERE SKU = N'SKU-PARA-500');
DECLARE @PU BIGINT = (SELECT TOP 1 Id FROM dbo.ProductUnits WHERE ProductId = @ProdId AND IsBaseUnit = 1);
DECLARE @BatchId BIGINT = (SELECT TOP 1 Id FROM dbo.InventoryBatches WHERE BatchStatus = N'Available' ORDER BY ExpiryDate DESC);
DECLARE @CustId BIGINT, @DocId BIGINT, @RxId BIGINT, @RxItemId BIGINT, @SaleId BIGINT, @DispId BIGINT;

IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE CustomerCode = N'C-001')
    INSERT INTO dbo.Customers (TenantId, CustomerCode, Name, Phone, IsPatient, IsActive)
    VALUES (@TenantId, N'C-001', N'Ali Patient', N'03001234567', 1, 1);
SET @CustId = (SELECT Id FROM dbo.Customers WHERE CustomerCode = N'C-001');

INSERT INTO dbo.Doctors (Name, PMDCNumber, Specialization, IsActive)
VALUES (N'Dr. Sara Khan', N'PMDC-123', N'General', 1);
SET @DocId = SCOPE_IDENTITY();

INSERT INTO dbo.Prescriptions (CustomerId, DoctorId, PrescriptionNumber, PrescriptionDate, Status, CreatedBy)
VALUES (@CustId, @DocId, N'RX-000001', CAST(SYSUTCDATETIME() AS DATE), N'Active', @UserId);
SET @RxId = SCOPE_IDENTITY();
INSERT INTO dbo.PrescriptionItems (PrescriptionId, ProductId, DosageAmount, DosageUnit, FrequencyCode, Quantity, RefillAllowed, RefillCount)
VALUES (@RxId, @ProdId, 1, N'tab', N'BID', 20, 1, 1);
SET @RxItemId = SCOPE_IDENTITY();

INSERT INTO dbo.Sales (BranchId, CounterId, TerminalId, UserId, CustomerId, InvoiceNumber, SaleDate, SaleType, Status,
    Subtotal, NetAmount, PaidAmount, DueAmount, ChangeAmount, PaymentStatus, FBRStatus)
VALUES (@BranchId, @CounterId, @TerminalId, @UserId, @CustId, N'INV-RX-0001', SYSUTCDATETIME(), N'Retail', N'Completed',
    100, 100, 100, 0, 0, N'Paid', N'Pending');
SET @SaleId = SCOPE_IDENTITY();
INSERT INTO dbo.SaleLines (SaleId, ProductId, ProductUnitId, Quantity, BaseQuantity, ConversionFactor, UnitPrice, MRP, NetAmount, PrescriptionItemId)
VALUES (@SaleId, @ProdId, @PU, 20, 20, 1, 5, 6, 100, @RxItemId);

INSERT INTO dbo.DispensingRecords (PrescriptionId, SaleId, CustomerId, DispensedBy, DispensedAt)
VALUES (@RxId, @SaleId, @CustId, @UserId, SYSUTCDATETIME());
SET @DispId = SCOPE_IDENTITY();
INSERT INTO dbo.DispensingItems (DispensingRecordId, PrescriptionItemId, ProductId, BatchId, Quantity)
VALUES (@DispId, @RxItemId, @ProdId, @BatchId, 20);
UPDATE dbo.Prescriptions SET Status = N'Dispensed' WHERE Id = @RxId;

IF EXISTS (SELECT 1 FROM dbo.DispensingRecords WHERE SaleId = @SaleId AND PrescriptionId = @RxId)
    PRINT N'TEST_PASS: 23 prescription dispensing linked to sale';
ELSE
    PRINT N'TEST_FAIL: 23 dispensing record missing';
GO

/* TEST 24: Controlled drug register balance trail */
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE Code = N'MAIN');
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @MfgId BIGINT = (SELECT TOP 1 Id FROM dbo.Manufacturers);
DECLARE @BrandId BIGINT = (SELECT TOP 1 Id FROM dbo.Brands);
DECLARE @CatId BIGINT = (SELECT TOP 1 Id FROM dbo.ProductCategories);
DECLARE @TcId BIGINT = (SELECT TOP 1 Id FROM dbo.TherapeuticClasses);
DECLARE @CtrlProd BIGINT, @RegId BIGINT, @Before DECIMAL(19,6), @After DECIMAL(19,6);
DECLARE @RxId BIGINT = (SELECT Id FROM dbo.Prescriptions WHERE PrescriptionNumber = N'RX-000001');
DECLARE @DocId BIGINT = (SELECT TOP 1 Id FROM dbo.Doctors);

INSERT INTO dbo.Products (TenantId, CategoryId, ManufacturerId, BrandId, TherapeuticClassId, SKU, Name, IsControlled, PrescriptionRequired, IsSaleable, IsActive)
VALUES (@TenantId, @CatId, @MfgId, @BrandId, @TcId, N'SKU-CTRL-001', N'Morphine 10mg', 1, 1, 1, 1);
SET @CtrlProd = SCOPE_IDENTITY();

INSERT INTO dbo.ControlledDrugRegisters (BranchId, ProductId, RegisterNumber, OpeningBalance, CurrentBalance, IsActive)
VALUES (@BranchId, @CtrlProd, N'CDR-001', 100, 100, 1);
SET @RegId = SCOPE_IDENTITY();
SET @Before = 100;

BEGIN TRAN;
INSERT INTO dbo.ControlledDrugTransactions (RegisterId, TransactionType, ReferenceType, ReferenceId, Quantity,
    BalanceBefore, BalanceAfter, PrescriptionId, DoctorId, PerformedBy, WitnessedBy, TransactionDate, Remarks)
VALUES (@RegId, N'Dispense', N'Sale', NULL, 5, @Before, @Before - 5, @RxId, @DocId, 1, 1, SYSUTCDATETIME(), N'Test dispense');
UPDATE dbo.ControlledDrugRegisters SET CurrentBalance = CurrentBalance - 5 WHERE Id = @RegId;
COMMIT;

SELECT @After = CurrentBalance FROM dbo.ControlledDrugRegisters WHERE Id = @RegId;
IF @After = 95 AND EXISTS (
    SELECT 1 FROM dbo.ControlledDrugTransactions WHERE RegisterId = @RegId AND BalanceBefore = 100 AND BalanceAfter = 95
)
    PRINT N'TEST_PASS: 24 controlled drug balance trail';
ELSE
    PRINT N'TEST_FAIL: 24 controlled balance trail incorrect';
GO

/* TEST 25: Cash shift open/close with variance */
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE Code = N'MAIN');
DECLARE @CounterId BIGINT = (SELECT Id FROM dbo.Counters WHERE Code = N'C1');
DECLARE @TerminalId BIGINT = (SELECT Id FROM dbo.POSTerminals WHERE TerminalCode = N'T1');
DECLARE @UserId BIGINT = (SELECT Id FROM dbo.Users WHERE Username = N'admin');
DECLARE @ShiftId BIGINT;

INSERT INTO dbo.CashShifts (BranchId, CounterId, TerminalId, UserId, OpeningAmount, OpeningAt, Status)
VALUES (@BranchId, @CounterId, @TerminalId, @UserId, 1000, SYSUTCDATETIME(), N'Open');
SET @ShiftId = SCOPE_IDENTITY();
INSERT INTO dbo.CashTransactions (CashShiftId, TransactionType, ReferenceType, ReferenceId, Amount, CreatedBy)
VALUES (@ShiftId, N'Sale', N'Sale', 1, 250, @UserId);
UPDATE dbo.CashShifts
SET ClosingAmount = 1240, ExpectedAmount = 1250, VarianceAmount = -10, ClosingAt = SYSUTCDATETIME(), Status = N'Closed'
WHERE Id = @ShiftId;

IF EXISTS (SELECT 1 FROM dbo.CashShifts WHERE Id = @ShiftId AND Status = N'Closed' AND VarianceAmount = -10)
    PRINT N'TEST_PASS: 25 cash shift open/close variance';
ELSE
    PRINT N'TEST_FAIL: 25 cash shift failed';
GO

/* TEST 26: Customer ledger on credit sale */
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE Code = N'MAIN');
DECLARE @CounterId BIGINT = (SELECT Id FROM dbo.Counters WHERE Code = N'C1');
DECLARE @TerminalId BIGINT = (SELECT Id FROM dbo.POSTerminals WHERE TerminalCode = N'T1');
DECLARE @UserId BIGINT = (SELECT Id FROM dbo.Users WHERE Username = N'admin');
DECLARE @CustId BIGINT = (SELECT Id FROM dbo.Customers WHERE CustomerCode = N'C-001');
DECLARE @CreditPm BIGINT = (SELECT Id FROM dbo.PaymentMethods WHERE Code = N'CREDIT');
DECLARE @SaleId BIGINT;

UPDATE dbo.Customers SET CreditLimit = 5000 WHERE Id = @CustId;
INSERT INTO dbo.Sales (BranchId, CounterId, TerminalId, UserId, CustomerId, InvoiceNumber, SaleDate, SaleType, Status,
    Subtotal, NetAmount, PaidAmount, DueAmount, ChangeAmount, PaymentStatus, FBRStatus)
VALUES (@BranchId, @CounterId, @TerminalId, @UserId, @CustId, N'INV-CR-0001', SYSUTCDATETIME(), N'Credit', N'Completed',
    200, 200, 0, 200, 0, N'Unpaid', N'Pending');
SET @SaleId = SCOPE_IDENTITY();
INSERT INTO dbo.SalePayments (SaleId, PaymentMethodId, Amount, PaymentDate, Status, TransactionType)
VALUES (@SaleId, @CreditPm, 200, SYSUTCDATETIME(), N'Completed', N'Payment');
INSERT INTO dbo.CustomerLedger (CustomerId, BranchId, TransactionDate, TransactionType, ReferenceType, ReferenceId, Debit, Credit, SequenceNo)
VALUES (@CustId, @BranchId, SYSUTCDATETIME(), N'Sale', N'Sale', @SaleId, 200, 0, 1);

IF EXISTS (SELECT 1 FROM dbo.CustomerLedger WHERE CustomerId = @CustId AND Debit = 200 AND ReferenceId = @SaleId)
    PRINT N'TEST_PASS: 26 customer ledger credit sale';
ELSE
    PRINT N'TEST_FAIL: 26 customer ledger missing';
GO

/* TEST 27: Supplier ledger after payment */
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE Code = N'MAIN');
DECLARE @SupplierId BIGINT = (SELECT Id FROM dbo.Suppliers WHERE Code = N'SUP01');
DECLARE @CashId BIGINT = (SELECT Id FROM dbo.PaymentMethods WHERE Code = N'CASH');
DECLARE @PayId BIGINT;

INSERT INTO dbo.SupplierPayments (SupplierId, BranchId, PaymentMethodId, Amount, PaymentDate, Remarks, PaidBy)
VALUES (@SupplierId, @BranchId, @CashId, 500, SYSUTCDATETIME(), N'Partial payment', 1);
SET @PayId = SCOPE_IDENTITY();
INSERT INTO dbo.SupplierLedger (SupplierId, BranchId, TransactionDate, TransactionType, ReferenceType, ReferenceId, Debit, Credit, SequenceNo)
VALUES (@SupplierId, @BranchId, SYSUTCDATETIME(), N'Payment', N'SupplierPayment', @PayId, 0, 500, 2);

IF EXISTS (SELECT 1 FROM dbo.SupplierLedger WHERE SupplierId = @SupplierId AND Credit = 500 AND ReferenceId = @PayId)
    PRINT N'TEST_PASS: 27 supplier ledger payment';
ELSE
    PRINT N'TEST_FAIL: 27 supplier ledger payment missing';
GO
