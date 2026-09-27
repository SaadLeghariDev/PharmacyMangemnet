USE PharmacyManagement;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

/* Wipe transactional / dependent data (children first), keep reference + demo org from seed re-run */
BEGIN TRAN;

DELETE FROM dbo.FiscalSubmissions;
DELETE FROM dbo.FiscalDocumentLines;
DELETE FROM dbo.FiscalDocuments;
DELETE FROM dbo.IntegrationLogs;
DELETE FROM dbo.SyncItems;
DELETE FROM dbo.SyncBatches;
DELETE FROM dbo.SyncNodes;
DELETE FROM dbo.IdempotencyKeys;
DELETE FROM dbo.NotificationLogs;
DELETE FROM dbo.Alerts;
DELETE FROM dbo.BarcodePrintJobs;
DELETE FROM dbo.DeviceEvents;
DELETE FROM dbo.DeviceSettings;
DELETE FROM dbo.DeviceAssignments;
DELETE FROM dbo.Devices;
DELETE FROM dbo.PrintTemplates;
DELETE FROM dbo.CashTransactions;
DELETE FROM dbo.CashShifts;
DELETE FROM dbo.Expenses;
DELETE FROM dbo.JournalLines;
DELETE FROM dbo.JournalEntries;
DELETE FROM dbo.ControlledDrugTransactions;
DELETE FROM dbo.ControlledDrugRegisters;
DELETE FROM dbo.Refills;
DELETE FROM dbo.DispensingItems;
DELETE FROM dbo.DispensingRecords;
DELETE FROM dbo.SaleReturnLines;
DELETE FROM dbo.SaleReturns;
DELETE FROM dbo.SalePayments;
DELETE FROM dbo.SaleLineBatches;
DELETE FROM dbo.InvoiceTaxes;
DELETE FROM dbo.SaleLines;
DELETE FROM dbo.HeldSales;
DELETE FROM dbo.Sales;
DELETE FROM dbo.PrescriptionItems;
DELETE FROM dbo.Prescriptions;
DELETE FROM dbo.CustomerLedger;
DELETE FROM dbo.CustomerPayments;
DELETE FROM dbo.StockAdjustmentLines;
DELETE FROM dbo.StockAdjustments;
DELETE FROM dbo.StockCountLines;
DELETE FROM dbo.StockCounts;
DELETE FROM dbo.StockTransferLines;
DELETE FROM dbo.StockTransfers;
DELETE FROM dbo.InventoryMovements;
DELETE FROM dbo.InventoryBatchLocations;
DELETE FROM dbo.SupplierReturnLines;
DELETE FROM dbo.SupplierReturns;
DELETE FROM dbo.SupplierPayments;
DELETE FROM dbo.SupplierLedger;
DELETE FROM dbo.ReorderRules;
/* Batches reference GoodsReceiptLines */
DELETE FROM dbo.InventoryBatches;
DELETE FROM dbo.GoodsReceiptLineBatches;
DELETE FROM dbo.GoodsReceiptLines;
DELETE FROM dbo.GoodsReceipts;
DELETE FROM dbo.PurchaseOrderLines;
DELETE FROM dbo.PurchaseOrders;
DELETE FROM dbo.SupplierContacts;
DELETE FROM dbo.ProductPrices;
DELETE FROM dbo.ProductTaxProfiles;
DELETE FROM dbo.Barcodes;
DELETE FROM dbo.UnitConversions;
DELETE FROM dbo.ProductUnits;
DELETE FROM dbo.ProductRegulatoryProfiles;
DELETE FROM dbo.ProductAliases;
DELETE FROM dbo.ProductIngredients;
DELETE FROM dbo.Products;
DELETE FROM dbo.Ingredients;
DELETE FROM dbo.TherapeuticClasses;
DELETE FROM dbo.Brands;
DELETE FROM dbo.ProductCategories;
DELETE FROM dbo.Manufacturers;
DELETE FROM dbo.TaxRates;
DELETE FROM dbo.TaxProfiles;
DELETE FROM dbo.PriceLists;
DELETE FROM dbo.Suppliers;
DELETE FROM dbo.Customers;
DELETE FROM dbo.Doctors;
DELETE FROM dbo.ChartOfAccounts;
DELETE FROM dbo.FiscalConfigurations;
DELETE FROM dbo.AlertRules;
DELETE FROM dbo.NotificationTemplates;
DELETE FROM dbo.EntityAttachments;
DELETE FROM dbo.Attachments;
DELETE FROM dbo.ApprovalRequests;
DELETE FROM dbo.AuditLogs;
DELETE FROM dbo.LoginSessions;
DELETE FROM dbo.ExpenseCategories;

COMMIT;
GO

/* Re-apply seed (idempotent) */
PRINT N'Transactional data wiped. Seed re-applied by runner.';
GO
