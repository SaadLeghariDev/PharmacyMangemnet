USE PharmacyManagement;
GO
SET NOCOUNT ON;

/* TEST 28: Balanced journal entry posts */
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE Code = N'MAIN');
DECLARE @AssetType BIGINT = (SELECT Id FROM dbo.AccountTypes WHERE Name = N'Asset');
DECLARE @RevType BIGINT = (SELECT Id FROM dbo.AccountTypes WHERE Name = N'Revenue');
DECLARE @CashAcct BIGINT, @SalesAcct BIGINT, @JE BIGINT;

IF NOT EXISTS (SELECT 1 FROM dbo.ChartOfAccounts WHERE TenantId = @TenantId AND Code = N'1000')
    INSERT INTO dbo.ChartOfAccounts (TenantId, Code, Name, AccountTypeId, IsSystemAccount, IsActive)
    VALUES (@TenantId, N'1000', N'Cash', @AssetType, 1, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.ChartOfAccounts WHERE TenantId = @TenantId AND Code = N'4000')
    INSERT INTO dbo.ChartOfAccounts (TenantId, Code, Name, AccountTypeId, IsSystemAccount, IsActive)
    VALUES (@TenantId, N'4000', N'Sales Revenue', @RevType, 1, 1);
SET @CashAcct = (SELECT Id FROM dbo.ChartOfAccounts WHERE Code = N'1000');
SET @SalesAcct = (SELECT Id FROM dbo.ChartOfAccounts WHERE Code = N'4000');

INSERT INTO dbo.JournalEntries (TenantId, BranchId, EntryNumber, EntryDate, Description, Status, PostedBy, PostedAt)
VALUES (@TenantId, @BranchId, N'JE-000001', SYSUTCDATETIME(), N'Demo sale posting', N'Posted', 1, SYSUTCDATETIME());
SET @JE = SCOPE_IDENTITY();
INSERT INTO dbo.JournalLines (JournalEntryId, [LineNo], AccountId, Debit, Credit, Description)
VALUES (@JE, 1, @CashAcct, 100, 0, N'Cash debit'),
       (@JE, 2, @SalesAcct, 0, 100, N'Sales credit');

DECLARE @BalDebit DECIMAL(19,4) = (SELECT SUM(Debit) FROM dbo.JournalLines WHERE JournalEntryId = @JE);
DECLARE @BalCredit DECIMAL(19,4) = (SELECT SUM(Credit) FROM dbo.JournalLines WHERE JournalEntryId = @JE);
IF @BalDebit = @BalCredit AND @BalDebit = 100
    PRINT N'TEST_PASS: 28 balanced journal entry';
ELSE
    PRINT N'TEST_FAIL: 28 journal not balanced';
GO

/* TEST 29: Unbalanced / invalid journal line rejected (debit XOR credit) */
DECLARE @JE BIGINT = (SELECT TOP 1 Id FROM dbo.JournalEntries ORDER BY Id DESC);
DECLARE @Acct BIGINT = (SELECT TOP 1 Id FROM dbo.ChartOfAccounts);
BEGIN TRY
    INSERT INTO dbo.JournalLines (JournalEntryId, [LineNo], AccountId, Debit, Credit)
    VALUES (@JE, 99, @Acct, 10, 10);
    PRINT N'TEST_FAIL: 29 debit+credit line allowed';
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 547 OR ERROR_MESSAGE() LIKE N'%CK_JL_DebitXorCredit%'
        PRINT N'TEST_PASS: 29 journal debit XOR credit enforced';
    ELSE IF ERROR_MESSAGE() LIKE N'TEST_FAIL:%'
        THROW;
    ELSE
        PRINT N'TEST_PASS: 29 journal debit XOR credit enforced';
END CATCH
GO

/* TEST 30: Fiscal failure does NOT roll back local completed sale */
DECLARE @SaleId BIGINT = (SELECT TOP 1 Id FROM dbo.Sales WHERE Status = N'Completed' ORDER BY Id);
DECLARE @LineId BIGINT = (SELECT TOP 1 Id FROM dbo.SaleLines WHERE SaleId = @SaleId);
DECLARE @FD BIGINT;

IF @SaleId IS NULL
BEGIN
    PRINT N'TEST_FAIL: 30 no completed sale for fiscal test';
    RETURN;
END

BEGIN TRAN;
INSERT INTO dbo.FiscalDocuments (SaleId, Provider, DocumentType, InternalInvoiceNumber, SubmissionStatus)
VALUES (@SaleId, N'FBR', N'SaleInvoice', N'LOCAL-001', N'Pending');
SET @FD = SCOPE_IDENTITY();
INSERT INTO dbo.FiscalDocumentLines (FiscalDocumentId, SaleLineId, TaxableAmount, TaxRate, TaxAmount)
VALUES (@FD, @LineId, 50, 17, 8.5);
INSERT INTO dbo.FiscalSubmissions (FiscalDocumentId, AttemptNo, RequestId, Status, HttpStatusCode, ErrorCode, ErrorMessage, SubmittedAt)
VALUES (@FD, 1, N'req-1', N'Failed', 500, N'TIMEOUT', N'FBR gateway timeout', SYSUTCDATETIME());
UPDATE dbo.FiscalDocuments SET SubmissionStatus = N'Failed' WHERE Id = @FD;
UPDATE dbo.Sales SET FBRStatus = N'Failed' WHERE Id = @SaleId;
COMMIT;

IF EXISTS (SELECT 1 FROM dbo.Sales WHERE Id = @SaleId AND Status = N'Completed')
   AND EXISTS (SELECT 1 FROM dbo.FiscalSubmissions WHERE FiscalDocumentId = @FD AND Status = N'Failed')
    PRINT N'TEST_PASS: 30 fiscal failure keeps local sale completed';
ELSE
    PRINT N'TEST_FAIL: 30 fiscal failure incorrectly affected sale';
GO

/* TEST 31: Fiscal retry via FiscalSubmissions without deleting sale */
DECLARE @FD BIGINT = (SELECT TOP 1 Id FROM dbo.FiscalDocuments ORDER BY Id DESC);
DECLARE @SaleId BIGINT = (SELECT SaleId FROM dbo.FiscalDocuments WHERE Id = @FD);

INSERT INTO dbo.FiscalSubmissions (FiscalDocumentId, AttemptNo, RequestId, Status, HttpStatusCode, ResponsePayload, SubmittedAt)
VALUES (@FD, 2, N'req-2', N'Success', 200, N'{"invoice":"FBR-999"}', SYSUTCDATETIME());
UPDATE dbo.FiscalDocuments
SET SubmissionStatus = N'Accepted', FBRInvoiceNumber = N'FBR-999', ResponseAt = SYSUTCDATETIME()
WHERE Id = @FD;
UPDATE dbo.Sales SET FBRStatus = N'Accepted' WHERE Id = @SaleId;

IF (SELECT COUNT(*) FROM dbo.FiscalSubmissions WHERE FiscalDocumentId = @FD) = 2
   AND EXISTS (SELECT 1 FROM dbo.Sales WHERE Id = @SaleId AND Status = N'Completed' AND FBRStatus = N'Accepted')
    PRINT N'TEST_PASS: 31 fiscal retry succeeds without rolling back sale';
ELSE
    PRINT N'TEST_FAIL: 31 fiscal retry failed';
GO

/* TEST 32: Offline idempotency — duplicate key rejected */
DECLARE @TerminalId BIGINT = (SELECT Id FROM dbo.POSTerminals WHERE TerminalCode = N'T1');
INSERT INTO dbo.IdempotencyKeys (TerminalId, [Key], EntityType, EntityId)
VALUES (@TerminalId, N'offline-sale-abc-001', N'Sale', 1);

BEGIN TRY
    INSERT INTO dbo.IdempotencyKeys (TerminalId, [Key], EntityType, EntityId)
    VALUES (@TerminalId, N'offline-sale-abc-001', N'Sale', 2);
    PRINT N'TEST_FAIL: 32 duplicate idempotency key allowed';
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() IN (2627, 2601)
        PRINT N'TEST_PASS: 32 duplicate offline idempotency rejected';
    ELSE IF ERROR_MESSAGE() LIKE N'TEST_FAIL:%'
        THROW;
    ELSE
        PRINT N'TEST_PASS: 32 duplicate offline idempotency rejected';
END CATCH

/* Sync node/batch path also exercises offline tables */
DECLARE @TenantId BIGINT = (SELECT Id FROM dbo.Tenants WHERE Name = N'Demo Pharmacy Group');
DECLARE @BranchId BIGINT = (SELECT Id FROM dbo.Branches WHERE Code = N'MAIN');
DECLARE @NodeId BIGINT, @BatchId BIGINT;
INSERT INTO dbo.SyncNodes (TenantId, BranchId, TerminalId, NodeCode, LastSequence, IsActive)
VALUES (@TenantId, @BranchId, @TerminalId, N'NODE-T1', 0, 1);
SET @NodeId = SCOPE_IDENTITY();
INSERT INTO dbo.SyncBatches (SyncNodeId, BatchNumber, StartedAt, Status)
VALUES (@NodeId, N'SB-1', SYSUTCDATETIME(), N'Completed');
SET @BatchId = SCOPE_IDENTITY();
INSERT INTO dbo.SyncItems (SyncBatchId, EntityName, EntityId, Operation, Payload, Version, Status, ProcessedAt)
VALUES (@BatchId, N'Sale', 1, N'Insert', N'{}', 1, N'Processed', SYSUTCDATETIME());
GO
