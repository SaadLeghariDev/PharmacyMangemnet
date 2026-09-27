using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Persistence;

public partial class PharmacyManagementDbContext : DbContext
{
    public PharmacyManagementDbContext(DbContextOptions<PharmacyManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccountType> AccountTypes { get; set; }

    public virtual DbSet<Alert> Alerts { get; set; }

    public virtual DbSet<AlertRule> AlertRules { get; set; }

    public virtual DbSet<ApprovalRequest> ApprovalRequests { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Barcode> Barcodes { get; set; }

    public virtual DbSet<BarcodePrintJob> BarcodePrintJobs { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<BranchSetting> BranchSettings { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<CashShift> CashShifts { get; set; }

    public virtual DbSet<CashTransaction> CashTransactions { get; set; }

    public virtual DbSet<ChartOfAccount> ChartOfAccounts { get; set; }

    public virtual DbSet<ControlledDrugRegister> ControlledDrugRegisters { get; set; }

    public virtual DbSet<ControlledDrugTransaction> ControlledDrugTransactions { get; set; }

    public virtual DbSet<Counter> Counters { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerLedger> CustomerLedgers { get; set; }

    public virtual DbSet<CustomerPayment> CustomerPayments { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceAssignment> DeviceAssignments { get; set; }

    public virtual DbSet<DeviceEvent> DeviceEvents { get; set; }

    public virtual DbSet<DeviceSetting> DeviceSettings { get; set; }

    public virtual DbSet<DeviceType> DeviceTypes { get; set; }

    public virtual DbSet<DispensingItem> DispensingItems { get; set; }

    public virtual DbSet<DispensingRecord> DispensingRecords { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<EntityAttachment> EntityAttachments { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<ExpenseCategory> ExpenseCategories { get; set; }

    public virtual DbSet<FiscalConfiguration> FiscalConfigurations { get; set; }

    public virtual DbSet<FiscalDocument> FiscalDocuments { get; set; }

    public virtual DbSet<FiscalDocumentLine> FiscalDocumentLines { get; set; }

    public virtual DbSet<FiscalSubmission> FiscalSubmissions { get; set; }

    public virtual DbSet<GoodsReceipt> GoodsReceipts { get; set; }

    public virtual DbSet<GoodsReceiptLine> GoodsReceiptLines { get; set; }

    public virtual DbSet<GoodsReceiptLineBatch> GoodsReceiptLineBatches { get; set; }

    public virtual DbSet<HeldSale> HeldSales { get; set; }

    public virtual DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<IntegrationLog> IntegrationLogs { get; set; }

    public virtual DbSet<InventoryBatch> InventoryBatches { get; set; }

    public virtual DbSet<InventoryBatchLocation> InventoryBatchLocations { get; set; }

    public virtual DbSet<InventoryMovement> InventoryMovements { get; set; }

    public virtual DbSet<InvoiceTaxis> InvoiceTaxes { get; set; }

    public virtual DbSet<JournalEntry> JournalEntries { get; set; }

    public virtual DbSet<JournalLine> JournalLines { get; set; }

    public virtual DbSet<LoginSession> LoginSessions { get; set; }

    public virtual DbSet<Manufacturer> Manufacturers { get; set; }

    public virtual DbSet<NotificationLog> NotificationLogs { get; set; }

    public virtual DbSet<NotificationTemplate> NotificationTemplates { get; set; }

    public virtual DbSet<NumberSequence> NumberSequences { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Posterminal> Posterminals { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<PrescriptionItem> PrescriptionItems { get; set; }

    public virtual DbSet<PriceList> PriceLists { get; set; }

    public virtual DbSet<PrintTemplate> PrintTemplates { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAlias> ProductAliases { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductIngredient> ProductIngredients { get; set; }

    public virtual DbSet<ProductPrice> ProductPrices { get; set; }

    public virtual DbSet<ProductRegulatoryProfile> ProductRegulatoryProfiles { get; set; }

    public virtual DbSet<ProductUnit> ProductUnits { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }

    public virtual DbSet<ReasonCode> ReasonCodes { get; set; }

    public virtual DbSet<Refill> Refills { get; set; }

    public virtual DbSet<ReorderRule> ReorderRules { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<SaleLine> SaleLines { get; set; }

    public virtual DbSet<SaleLineBatch> SaleLineBatches { get; set; }

    public virtual DbSet<SalePayment> SalePayments { get; set; }

    public virtual DbSet<SaleReturn> SaleReturns { get; set; }

    public virtual DbSet<SaleReturnLine> SaleReturnLines { get; set; }

    public virtual DbSet<StockAdjustment> StockAdjustments { get; set; }

    public virtual DbSet<StockAdjustmentLine> StockAdjustmentLines { get; set; }

    public virtual DbSet<StockCount> StockCounts { get; set; }

    public virtual DbSet<StockCountLine> StockCountLines { get; set; }

    public virtual DbSet<StockTransfer> StockTransfers { get; set; }

    public virtual DbSet<StockTransferLine> StockTransferLines { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<SupplierContact> SupplierContacts { get; set; }

    public virtual DbSet<SupplierLedger> SupplierLedgers { get; set; }

    public virtual DbSet<SupplierPayment> SupplierPayments { get; set; }

    public virtual DbSet<SupplierReturn> SupplierReturns { get; set; }

    public virtual DbSet<SupplierReturnLine> SupplierReturnLines { get; set; }

    public virtual DbSet<SyncBatch> SyncBatches { get; set; }

    public virtual DbSet<SyncItem> SyncItems { get; set; }

    public virtual DbSet<SyncNode> SyncNodes { get; set; }

    public virtual DbSet<TaxProfile> TaxProfiles { get; set; }

    public virtual DbSet<TaxRate> TaxRates { get; set; }

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<TenantSetting> TenantSettings { get; set; }

    public virtual DbSet<TherapeuticClass> TherapeuticClasses { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<UnitConversion> UnitConversions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    public virtual DbSet<WarehouseLocation> WarehouseLocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountType>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_AccountTypes_Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.Severity).HasMaxLength(30);
            entity.Property(e => e.Status).HasMaxLength(30);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.AlertRule).WithMany(p => p.Alerts)
                .HasForeignKey(d => d.AlertRuleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Alerts_Rules");

            entity.HasOne(d => d.Batch).WithMany(p => p.Alerts)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("FK_Alerts_Batches");

            entity.HasOne(d => d.Branch).WithMany(p => p.Alerts)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Alerts_Branches");

            entity.HasOne(d => d.Product).WithMany(p => p.Alerts)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Alerts_Products");
        });

        modelBuilder.Entity<AlertRule>(entity =>
        {
            entity.Property(e => e.AlertType).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Threshold).HasColumnType("decimal(19, 6)");

            entity.HasOne(d => d.Branch).WithMany(p => p.AlertRules)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK_AlertRules_Branches");

            entity.HasOne(d => d.Tenant).WithMany(p => p.AlertRules)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AlertRules_Tenants");
        });

        modelBuilder.Entity<ApprovalRequest>(entity =>
        {
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.ReferenceType).HasMaxLength(100);
            entity.Property(e => e.RequestType).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(30);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.Property(e => e.ContentType).HasMaxLength(150);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FileName).HasMaxLength(250);
            entity.Property(e => e.Hash).HasMaxLength(128);
            entity.Property(e => e.StoragePath).HasMaxLength(1000);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attachments_Tenants");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => new { e.EntityName, e.EntityId, e.CreatedAt }, "IX_AuditLogs_Entity");

            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EntityName).HasMaxLength(150);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
        });

        modelBuilder.Entity<Barcode>(entity =>
        {
            entity.HasIndex(e => e.BarcodeValue, "IX_Barcodes_BarcodeValue").HasFilter("([IsActive]=(1))");

            entity.HasIndex(e => e.ProductId, "IX_Barcodes_ProductId");

            entity.HasIndex(e => e.BarcodeValue, "UQ_Barcodes_BarcodeValue").IsUnique();

            entity.Property(e => e.BarcodeType).HasMaxLength(50);
            entity.Property(e => e.BarcodeValue).HasMaxLength(150);
            entity.Property(e => e.Gtin)
                .HasMaxLength(50)
                .HasColumnName("GTIN");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SerialNumber).HasMaxLength(100);

            entity.HasOne(d => d.Product).WithMany(p => p.Barcodes)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Barcodes_Products");

            entity.HasOne(d => d.ProductUnit).WithMany(p => p.Barcodes)
                .HasForeignKey(d => d.ProductUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Barcodes_ProductUnits");
        });

        modelBuilder.Entity<BarcodePrintJob>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.Batch).WithMany(p => p.BarcodePrintJobs)
                .HasForeignKey(d => d.BatchId)
                .HasConstraintName("FK_BPJ_Batches");

            entity.HasOne(d => d.Branch).WithMany(p => p.BarcodePrintJobs)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BPJ_Branches");

            entity.HasOne(d => d.PrinterDevice).WithMany(p => p.BarcodePrintJobs)
                .HasForeignKey(d => d.PrinterDeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BPJ_Devices");

            entity.HasOne(d => d.Product).WithMany(p => p.BarcodePrintJobs)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BPJ_Products");

            entity.HasOne(d => d.Template).WithMany(p => p.BarcodePrintJobs)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BPJ_Templates");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasIndex(e => e.TenantId, "IX_Branches_TenantId").HasFilter("([IsActive]=(1))");

            entity.HasIndex(e => new { e.TenantId, e.Code }, "UQ_Branches_Tenant_Code").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.BranchType).HasMaxLength(50);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DrugLicenseNo).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Ntn)
                .HasMaxLength(50)
                .HasColumnName("NTN");
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Strn)
                .HasMaxLength(50)
                .HasColumnName("STRN");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Branches)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Branches_Tenants");
        });

        modelBuilder.Entity<BranchSetting>(entity =>
        {
            entity.HasIndex(e => new { e.BranchId, e.SettingKey }, "UQ_BranchSettings_Branch_Key").IsUnique();

            entity.Property(e => e.SettingKey).HasMaxLength(150);

            entity.HasOne(d => d.Branch).WithMany(p => p.BranchSettings)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BranchSettings_Branches");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.Brands)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Brands_Manufacturers");
        });

        modelBuilder.Entity<CashShift>(entity =>
        {
            entity.Property(e => e.ClosingAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.ExpectedAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.OpeningAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Status).HasMaxLength(30);
            entity.Property(e => e.VarianceAmount).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Branch).WithMany(p => p.CashShifts)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CashShifts_Branches");

            entity.HasOne(d => d.Counter).WithMany(p => p.CashShifts)
                .HasForeignKey(d => d.CounterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CashShifts_Counters");

            entity.HasOne(d => d.Terminal).WithMany(p => p.CashShifts)
                .HasForeignKey(d => d.TerminalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CashShifts_Terminals");

            entity.HasOne(d => d.User).WithMany(p => p.CashShifts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CashShifts_Users");
        });

        modelBuilder.Entity<CashTransaction>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ReferenceType).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.CashShift).WithMany(p => p.CashTransactions)
                .HasForeignKey(d => d.CashShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CashTransactions_Shifts");
        });

        modelBuilder.Entity<ChartOfAccount>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Code }, "UQ_COA_Tenant_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.AccountType).WithMany(p => p.ChartOfAccounts)
                .HasForeignKey(d => d.AccountTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_COA_AccountTypes");

            entity.HasOne(d => d.ParentAccount).WithMany(p => p.InverseParentAccount)
                .HasForeignKey(d => d.ParentAccountId)
                .HasConstraintName("FK_COA_Parent");

            entity.HasOne(d => d.Tenant).WithMany(p => p.ChartOfAccounts)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_COA_Tenants");
        });

        modelBuilder.Entity<ControlledDrugRegister>(entity =>
        {
            entity.HasIndex(e => new { e.BranchId, e.RegisterNumber }, "UQ_CDR_Branch_Register").IsUnique();

            entity.Property(e => e.CurrentBalance).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OpeningBalance).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.RegisterNumber).HasMaxLength(100);

            entity.HasOne(d => d.Branch).WithMany(p => p.ControlledDrugRegisters)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CDR_Branches");

            entity.HasOne(d => d.Product).WithMany(p => p.ControlledDrugRegisters)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CDR_Products");
        });

        modelBuilder.Entity<ControlledDrugTransaction>(entity =>
        {
            entity.HasIndex(e => new { e.RegisterId, e.TransactionDate }, "IX_ControlledDrugTransactions_Register");

            entity.Property(e => e.BalanceAfter).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.BalanceBefore).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.ReferenceType).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.Doctor).WithMany(p => p.ControlledDrugTransactions)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK_CDT_Doctors");

            entity.HasOne(d => d.Prescription).WithMany(p => p.ControlledDrugTransactions)
                .HasForeignKey(d => d.PrescriptionId)
                .HasConstraintName("FK_CDT_Prescriptions");

            entity.HasOne(d => d.Register).WithMany(p => p.ControlledDrugTransactions)
                .HasForeignKey(d => d.RegisterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CDT_Registers");
        });

        modelBuilder.Entity<Counter>(entity =>
        {
            entity.HasIndex(e => new { e.BranchId, e.Code }, "UQ_Counters_Branch_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CounterType).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Branch).WithMany(p => p.Counters)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Counters_Branches");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => e.TenantId, "IX_Customers_TenantId").HasFilter("([IsActive]=(1))");

            entity.HasIndex(e => new { e.TenantId, e.CustomerCode }, "UQ_Customers_Tenant_Code").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Cnic)
                .HasMaxLength(50)
                .HasColumnName("CNIC");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreditLimit).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.CustomerCode).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Gender).HasMaxLength(30);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Customers)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Customers_Tenants");
        });

        modelBuilder.Entity<CustomerLedger>(entity =>
        {
            entity.ToTable("CustomerLedger");

            entity.HasIndex(e => new { e.CustomerId, e.SequenceNo }, "IX_CustomerLedger_Customer");

            entity.Property(e => e.Credit).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Debit).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.ReferenceType).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.Branch).WithMany(p => p.CustomerLedgers)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerLedger_Branches");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerLedgers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerLedger_Customers");
        });

        modelBuilder.Entity<CustomerPayment>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);

            entity.HasOne(d => d.Branch).WithMany(p => p.CustomerPayments)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerPayments_Branches");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerPayments)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerPayments_Customers");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.CustomerPayments)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerPayments_Methods");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.Property(e => e.Comport)
                .HasMaxLength(50)
                .HasColumnName("COMPort");
            entity.Property(e => e.ConnectionType).HasMaxLength(50);
            entity.Property(e => e.DriverName).HasMaxLength(150);
            entity.Property(e => e.DriverVersion).HasMaxLength(50);
            entity.Property(e => e.Ip)
                .HasMaxLength(50)
                .HasColumnName("IP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MacAddress).HasMaxLength(100);
            entity.Property(e => e.Manufacturer).HasMaxLength(150);
            entity.Property(e => e.Model).HasMaxLength(150);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.SerialNumber).HasMaxLength(150);

            entity.HasOne(d => d.Branch).WithMany(p => p.Devices)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Devices_Branches");

            entity.HasOne(d => d.Counter).WithMany(p => p.Devices)
                .HasForeignKey(d => d.CounterId)
                .HasConstraintName("FK_Devices_Counters");

            entity.HasOne(d => d.DeviceType).WithMany(p => p.Devices)
                .HasForeignKey(d => d.DeviceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Devices_DeviceTypes");
        });

        modelBuilder.Entity<DeviceAssignment>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceAssignments)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeviceAssignments_Devices");

            entity.HasOne(d => d.Terminal).WithMany(p => p.DeviceAssignments)
                .HasForeignKey(d => d.TerminalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeviceAssignments_Terminals");
        });

        modelBuilder.Entity<DeviceEvent>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceEvents)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeviceEvents_Devices");
        });

        modelBuilder.Entity<DeviceSetting>(entity =>
        {
            entity.HasIndex(e => new { e.DeviceId, e.SettingKey }, "UQ_DeviceSettings_Device_Key").IsUnique();

            entity.Property(e => e.SettingKey).HasMaxLength(100);

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceSettings)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeviceSettings_Devices");
        });

        modelBuilder.Entity<DeviceType>(entity =>
        {
            entity.HasIndex(e => e.Code, "UQ_DeviceTypes_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<DispensingItem>(entity =>
        {
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 6)");

            entity.HasOne(d => d.Batch).WithMany(p => p.DispensingItems)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DispensingItems_Batches");

            entity.HasOne(d => d.DispensingRecord).WithMany(p => p.DispensingItems)
                .HasForeignKey(d => d.DispensingRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DispensingItems_Records");

            entity.HasOne(d => d.PrescriptionItem).WithMany(p => p.DispensingItems)
                .HasForeignKey(d => d.PrescriptionItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DispensingItems_Items");

            entity.HasOne(d => d.Product).WithMany(p => p.DispensingItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DispensingItems_Products");
        });

        modelBuilder.Entity<DispensingRecord>(entity =>
        {
            entity.HasOne(d => d.Customer).WithMany(p => p.DispensingRecords)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DispensingRecords_Customers");

            entity.HasOne(d => d.Prescription).WithMany(p => p.DispensingRecords)
                .HasForeignKey(d => d.PrescriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DispensingRecords_Prescriptions");

            entity.HasOne(d => d.Sale).WithMany(p => p.DispensingRecords)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DispensingRecords_Sales");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.ClinicName).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Pmdcnumber)
                .HasMaxLength(100)
                .HasColumnName("PMDCNumber");
            entity.Property(e => e.Specialization).HasMaxLength(150);
        });

        modelBuilder.Entity<EntityAttachment>(entity =>
        {
            entity.Property(e => e.EntityName).HasMaxLength(150);

            entity.HasOne(d => d.Attachment).WithMany(p => p.EntityAttachments)
                .HasForeignKey(d => d.AttachmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EntityAttachments_Attachments");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasIndex(e => new { e.BranchId, e.ExpenseNumber }, "UQ_Expenses_Branch_Number").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ExpenseNumber).HasMaxLength(50);

            entity.HasOne(d => d.Branch).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Expenses_Branches");

            entity.HasOne(d => d.Category).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Expenses_Categories");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Expenses_PaymentMethods");
        });

        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.HasIndex(e => e.Code, "UQ_ExpenseCategories_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<FiscalConfiguration>(entity =>
        {
            entity.Property(e => e.ApiBaseUrl).HasMaxLength(500);
            entity.Property(e => e.CredentialsReference).HasMaxLength(500);
            entity.Property(e => e.Environment).HasMaxLength(30);
            entity.Property(e => e.IntegrationType).HasMaxLength(50);
            entity.Property(e => e.Ntn)
                .HasMaxLength(50)
                .HasColumnName("NTN");
            entity.Property(e => e.Posid)
                .HasMaxLength(100)
                .HasColumnName("POSId");
            entity.Property(e => e.ProviderName).HasMaxLength(100);
            entity.Property(e => e.Strn)
                .HasMaxLength(50)
                .HasColumnName("STRN");

            entity.HasOne(d => d.Branch).WithMany(p => p.FiscalConfigurations)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK_FiscalConfig_Branches");

            entity.HasOne(d => d.Tenant).WithMany(p => p.FiscalConfigurations)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FiscalConfig_Tenants");
        });

        modelBuilder.Entity<FiscalDocument>(entity =>
        {
            entity.HasIndex(e => e.SaleId, "IX_FiscalDocuments_SaleId");

            entity.Property(e => e.DocumentType).HasMaxLength(50);
            entity.Property(e => e.ExternalInvoiceNumber).HasMaxLength(100);
            entity.Property(e => e.FbrinvoiceNumber)
                .HasMaxLength(100)
                .HasColumnName("FBRInvoiceNumber");
            entity.Property(e => e.InternalInvoiceNumber).HasMaxLength(100);
            entity.Property(e => e.Provider).HasMaxLength(100);
            entity.Property(e => e.Qrdata).HasColumnName("QRData");
            entity.Property(e => e.SubmissionStatus).HasMaxLength(50);
            entity.Property(e => e.VerificationUrl).HasMaxLength(1000);

            entity.HasOne(d => d.Sale).WithMany(p => p.FiscalDocuments)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FiscalDocuments_Sales");
        });

        modelBuilder.Entity<FiscalDocumentLine>(entity =>
        {
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.TaxRate).HasColumnType("decimal(9, 4)");
            entity.Property(e => e.TaxableAmount).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.FiscalDocument).WithMany(p => p.FiscalDocumentLines)
                .HasForeignKey(d => d.FiscalDocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FDL_Documents");

            entity.HasOne(d => d.SaleLine).WithMany(p => p.FiscalDocumentLines)
                .HasForeignKey(d => d.SaleLineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FDL_SaleLines");
        });

        modelBuilder.Entity<FiscalSubmission>(entity =>
        {
            entity.HasIndex(e => new { e.FiscalDocumentId, e.AttemptNo }, "UQ_FS_Document_Attempt").IsUnique();

            entity.Property(e => e.ErrorCode).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.RequestId).HasMaxLength(150);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.FiscalDocument).WithMany(p => p.FiscalSubmissions)
                .HasForeignKey(d => d.FiscalDocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FS_Documents");
        });

        modelBuilder.Entity<GoodsReceipt>(entity =>
        {
            entity.HasIndex(e => e.SupplierId, "IX_GoodsReceipts_SupplierId");

            entity.HasIndex(e => new { e.BranchId, e.Grnnumber }, "UQ_GoodsReceipts_Branch_GRN").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Grnnumber)
                .HasMaxLength(50)
                .HasColumnName("GRNNumber");
            entity.Property(e => e.InvoiceNumber).HasMaxLength(100);
            entity.Property(e => e.NetAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Status).HasMaxLength(30);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Branch).WithMany(p => p.GoodsReceipts)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GoodsReceipts_Branches");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.GoodsReceipts)
                .HasForeignKey(d => d.PurchaseOrderId)
                .HasConstraintName("FK_GoodsReceipts_PO");

            entity.HasOne(d => d.Supplier).WithMany(p => p.GoodsReceipts)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GoodsReceipts_Suppliers");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.GoodsReceipts)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GoodsReceipts_Warehouses");
        });

        modelBuilder.Entity<GoodsReceiptLine>(entity =>
        {
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.FreeQuantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.NetCost).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.OrderedQuantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.ReceivedQuantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.GoodsReceipt).WithMany(p => p.GoodsReceiptLines)
                .HasForeignKey(d => d.GoodsReceiptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GoodsReceiptLines_GR");

            entity.HasOne(d => d.Product).WithMany(p => p.GoodsReceiptLines)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GoodsReceiptLines_Products");

            entity.HasOne(d => d.ProductUnit).WithMany(p => p.GoodsReceiptLines)
                .HasForeignKey(d => d.ProductUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GoodsReceiptLines_ProductUnits");
        });

        modelBuilder.Entity<GoodsReceiptLineBatch>(entity =>
        {
            entity.Property(e => e.BatchNumber).HasMaxLength(100);
            entity.Property(e => e.FreeQuantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.Mrp)
                .HasColumnType("decimal(19, 4)")
                .HasColumnName("MRP");
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.SalePrice).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.GoodsReceiptLine).WithMany(p => p.GoodsReceiptLineBatches)
                .HasForeignKey(d => d.GoodsReceiptLineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GRLB_GoodsReceiptLines");

            entity.HasOne(d => d.WarehouseLocation).WithMany(p => p.GoodsReceiptLineBatches)
                .HasForeignKey(d => d.WarehouseLocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GRLB_WarehouseLocations");
        });

        modelBuilder.Entity<HeldSale>(entity =>
        {
            entity.Property(e => e.Status).HasMaxLength(30);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Branch).WithMany(p => p.HeldSales)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HeldSales_Branches");

            entity.HasOne(d => d.Customer).WithMany(p => p.HeldSales)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_HeldSales_Customers");

            entity.HasOne(d => d.Terminal).WithMany(p => p.HeldSales)
                .HasForeignKey(d => d.TerminalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HeldSales_Terminals");

            entity.HasOne(d => d.User).WithMany(p => p.HeldSales)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HeldSales_Users");
        });

        modelBuilder.Entity<IdempotencyKey>(entity =>
        {
            entity.HasIndex(e => new { e.TerminalId, e.Key }, "UQ_IdempotencyKeys_Terminal_Key").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.Key).HasMaxLength(150);

            entity.HasOne(d => d.Terminal).WithMany(p => p.IdempotencyKeys)
                .HasForeignKey(d => d.TerminalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IdempotencyKeys_Terminals");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.GenericName).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<IntegrationLog>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.IntegrationType).HasMaxLength(50);
            entity.Property(e => e.ReferenceType).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<InventoryBatch>(entity =>
        {
            entity.HasIndex(e => new { e.ProductId, e.ExpiryDate, e.BatchStatus }, "IX_InventoryBatches_FEFO");

            entity.Property(e => e.BatchNumber).HasMaxLength(100);
            entity.Property(e => e.BatchStatus).HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FreeQuantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.Mrp)
                .HasColumnType("decimal(19, 4)")
                .HasColumnName("MRP");
            entity.Property(e => e.PurchaseCost).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.QuantityReceived).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SalePrice).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.GoodsReceiptLine).WithMany(p => p.InventoryBatches)
                .HasForeignKey(d => d.GoodsReceiptLineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryBatches_GRL");

            entity.HasOne(d => d.Product).WithMany(p => p.InventoryBatches)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryBatches_Products");

            entity.HasOne(d => d.Supplier).WithMany(p => p.InventoryBatches)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryBatches_Suppliers");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.InventoryBatches)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryBatches_Warehouses");
        });

        modelBuilder.Entity<InventoryBatchLocation>(entity =>
        {
            entity.HasIndex(e => new { e.WarehouseLocationId, e.BatchId }, "IX_InventoryBatchLocations_Location_Batch");

            entity.HasIndex(e => new { e.BatchId, e.WarehouseLocationId }, "UQ_IBL_Batch_Location").IsUnique();

            entity.Property(e => e.QuantityOnHand).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.ReservedQuantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Batch).WithMany(p => p.InventoryBatchLocations)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IBL_Batches");

            entity.HasOne(d => d.WarehouseLocation).WithMany(p => p.InventoryBatchLocations)
                .HasForeignKey(d => d.WarehouseLocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IBL_Locations");
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasIndex(e => new { e.BatchId, e.MovementDate }, "IX_InventoryMovements_BatchId");

            entity.HasIndex(e => e.IdempotencyKey, "IX_InventoryMovements_Idempotency")
                .IsUnique()
                .HasFilter("([IdempotencyKey] IS NOT NULL)");

            entity.Property(e => e.BalanceAfter).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.BalanceBefore).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IdempotencyKey).HasMaxLength(150);
            entity.Property(e => e.MovementType).HasMaxLength(50);
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.ReferenceType).HasMaxLength(100);
            entity.Property(e => e.TotalCost).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Batch).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IM_Batches");

            entity.HasOne(d => d.Branch).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IM_Branches");

            entity.HasOne(d => d.Product).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IM_Products");

            entity.HasOne(d => d.ProductUnit).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.ProductUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IM_ProductUnits");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IM_Warehouses");

            entity.HasOne(d => d.WarehouseLocation).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.WarehouseLocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IM_Locations");
        });

        modelBuilder.Entity<InvoiceTaxis>(entity =>
        {
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.TaxRate).HasColumnType("decimal(9, 4)");
            entity.Property(e => e.TaxableAmount).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Sale).WithMany(p => p.InvoiceTaxes)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceTaxes_Sales");

            entity.HasOne(d => d.SaleLine).WithMany(p => p.InvoiceTaxes)
                .HasForeignKey(d => d.SaleLineId)
                .HasConstraintName("FK_InvoiceTaxes_SaleLines");

            entity.HasOne(d => d.TaxProfile).WithMany(p => p.InvoiceTaxes)
                .HasForeignKey(d => d.TaxProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceTaxes_TaxProfiles");
        });

        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.EntryNumber }, "UQ_JE_Tenant_EntryNumber").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.EntryNumber).HasMaxLength(50);
            entity.Property(e => e.ReferenceType).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.Branch).WithMany(p => p.JournalEntries)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JE_Branches");

            entity.HasOne(d => d.ReversalOfEntry).WithMany(p => p.InverseReversalOfEntry)
                .HasForeignKey(d => d.ReversalOfEntryId)
                .HasConstraintName("FK_JE_Reversal");

            entity.HasOne(d => d.Tenant).WithMany(p => p.JournalEntries)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JE_Tenants");
        });

        modelBuilder.Entity<JournalLine>(entity =>
        {
            entity.HasIndex(e => new { e.JournalEntryId, e.LineNo }, "UQ_JL_Entry_LineNo").IsUnique();

            entity.Property(e => e.Credit).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Debit).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(d => d.Account).WithMany(p => p.JournalLines)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JL_Accounts");

            entity.HasOne(d => d.JournalEntry).WithMany(p => p.JournalLines)
                .HasForeignKey(d => d.JournalEntryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JL_Entries");
        });

        modelBuilder.Entity<LoginSession>(entity =>
        {
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.Terminal).WithMany(p => p.LoginSessions)
                .HasForeignKey(d => d.TerminalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoginSessions_POSTerminals");

            entity.HasOne(d => d.User).WithMany(p => p.LoginSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoginSessions_Users");
        });

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LicenseNo).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
        });

        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.Property(e => e.Channel).HasMaxLength(30);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.Recipient).HasMaxLength(250);
            entity.Property(e => e.ReferenceType).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.Template).WithMany(p => p.NotificationLogs)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NotificationLogs_Templates");
        });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Code }, "UQ_NotificationTemplates_Tenant_Code").IsUnique();

            entity.Property(e => e.Channel).HasMaxLength(30);
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Subject).HasMaxLength(250);

            entity.HasOne(d => d.Tenant).WithMany(p => p.NotificationTemplates)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NotificationTemplates_Tenants");
        });

        modelBuilder.Entity<NumberSequence>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.DocumentType, e.BranchId, e.TerminalId }, "IX_NumberSequences_Lookup").IsUnique();

            entity.Property(e => e.DocumentType).HasMaxLength(50);
            entity.Property(e => e.NumberLength).HasDefaultValue(6);
            entity.Property(e => e.Prefix).HasMaxLength(30);
            entity.Property(e => e.ResetPeriod).HasMaxLength(30);

            entity.HasOne(d => d.Branch).WithMany(p => p.NumberSequences)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK_NumberSequences_Branches");

            entity.HasOne(d => d.Tenant).WithMany(p => p.NumberSequences)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NumberSequences_Tenants");

            entity.HasOne(d => d.Terminal).WithMany(p => p.NumberSequences)
                .HasForeignKey(d => d.TerminalId)
                .HasConstraintName("FK_NumberSequences_Terminals");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasIndex(e => e.Code, "UQ_PaymentMethods_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(30);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasIndex(e => e.Code, "UQ_Permissions_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Module).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<Posterminal>(entity =>
        {
            entity.ToTable("POSTerminals");

            entity.HasIndex(e => e.BranchId, "IX_POSTerminals_BranchId");

            entity.HasIndex(e => new { e.BranchId, e.TerminalCode }, "UQ_POSTerminals_Branch_Code").IsUnique();

            entity.Property(e => e.ComputerName).HasMaxLength(150);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MacAddress).HasMaxLength(100);
            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.TerminalCode).HasMaxLength(50);

            entity.HasOne(d => d.Branch).WithMany(p => p.Posterminals)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_POSTerminals_Branches");

            entity.HasOne(d => d.Counter).WithMany(p => p.Posterminals)
                .HasForeignKey(d => d.CounterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_POSTerminals_Counters");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasIndex(e => e.PrescriptionNumber, "UQ_Prescriptions_Number").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DiagnosisNotes).HasMaxLength(2000);
            entity.Property(e => e.PrescriptionNumber).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.Customer).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prescriptions_Customers");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prescriptions_Doctors");

            entity.HasOne(d => d.PrescriptionAttachment).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.PrescriptionAttachmentId)
                .HasConstraintName("FK_Prescriptions_Attachments");
        });

        modelBuilder.Entity<PrescriptionItem>(entity =>
        {
            entity.Property(e => e.DosageAmount).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.DosageUnit).HasMaxLength(50);
            entity.Property(e => e.DurationUnit).HasMaxLength(30);
            entity.Property(e => e.DurationValue).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.FrequencyCode).HasMaxLength(50);
            entity.Property(e => e.Instructions).HasMaxLength(1000);
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.Route).HasMaxLength(50);

            entity.HasOne(d => d.Prescription).WithMany(p => p.PrescriptionItems)
                .HasForeignKey(d => d.PrescriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrescriptionItems_Prescriptions");

            entity.HasOne(d => d.Product).WithMany(p => p.PrescriptionItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrescriptionItems_Products");
        });

        modelBuilder.Entity<PriceList>(entity =>
        {
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("PKR")
                .IsFixedLength();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PriceType).HasMaxLength(50);

            entity.HasOne(d => d.Tenant).WithMany(p => p.PriceLists)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PriceLists_Tenants");
        });

        modelBuilder.Entity<PrintTemplate>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PaperWidth).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TemplateType).HasMaxLength(50);

            entity.HasOne(d => d.Tenant).WithMany(p => p.PrintTemplates)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrintTemplates_Tenants");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_Products_CategoryId");

            entity.HasIndex(e => e.TenantId, "IX_Products_TenantId").HasFilter("([IsActive]=(1))");

            entity.HasIndex(e => new { e.TenantId, e.Sku }, "UQ_Products_Tenant_SKU").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Form).HasMaxLength(100);
            entity.Property(e => e.GenericName).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsReturnable).HasDefaultValue(true);
            entity.Property(e => e.IsSaleable).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.PackDescription).HasMaxLength(250);
            entity.Property(e => e.ProductCode).HasMaxLength(100);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .HasColumnName("SKU");
            entity.Property(e => e.Strength).HasMaxLength(100);
            entity.Property(e => e.StrengthUnit).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Brands");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.Products)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Manufacturers");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Products)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Tenants");

            entity.HasOne(d => d.TherapeuticClass).WithMany(p => p.Products)
                .HasForeignKey(d => d.TherapeuticClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_TherapeuticClasses");

            entity.HasMany(d => d.TaxProfiles).WithMany(p => p.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductTaxProfile",
                    r => r.HasOne<TaxProfile>().WithMany()
                        .HasForeignKey("TaxProfileId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ProductTaxProfiles_TaxProfiles"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ProductTaxProfiles_Products"),
                    j =>
                    {
                        j.HasKey("ProductId", "TaxProfileId");
                        j.ToTable("ProductTaxProfiles");
                    });
        });

        modelBuilder.Entity<ProductAlias>(entity =>
        {
            entity.Property(e => e.Alias).HasMaxLength(250);
            entity.Property(e => e.AliasType).HasMaxLength(50);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAliases)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductAliases_Products");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_ProductCategories_Parent");
        });

        modelBuilder.Entity<ProductIngredient>(entity =>
        {
            entity.Property(e => e.Percentage).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.Strength).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.StrengthUnit).HasMaxLength(50);

            entity.HasOne(d => d.Ingredient).WithMany(p => p.ProductIngredients)
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductIngredients_Ingredients");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductIngredients)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductIngredients_Products");
        });

        modelBuilder.Entity<ProductPrice>(entity =>
        {
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(9, 4)");
            entity.Property(e => e.Mrp)
                .HasColumnType("decimal(19, 4)")
                .HasColumnName("MRP");
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.SalePrice).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.PriceList).WithMany(p => p.ProductPrices)
                .HasForeignKey(d => d.PriceListId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductPrices_PriceLists");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductPrices)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductPrices_Products");

            entity.HasOne(d => d.ProductUnit).WithMany(p => p.ProductPrices)
                .HasForeignKey(d => d.ProductUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductPrices_ProductUnits");
        });

        modelBuilder.Entity<ProductRegulatoryProfile>(entity =>
        {
            entity.Property(e => e.ControlledDrugClass).HasMaxLength(100);
            entity.Property(e => e.DrugLicenseCategory).HasMaxLength(100);
            entity.Property(e => e.DrugRegistrationNo).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ScheduleCode).HasMaxLength(50);
            entity.Property(e => e.StorageCondition).HasMaxLength(250);
            entity.Property(e => e.TemperatureMax).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TemperatureMin).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductRegulatoryProfiles)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductRegulatoryProfiles_Products");
        });

        modelBuilder.Entity<ProductUnit>(entity =>
        {
            entity.HasIndex(e => new { e.ProductId, e.UnitId }, "UQ_ProductUnits_Product_Unit").IsUnique();

            entity.Property(e => e.ConversionToBase).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductUnits)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductUnits_Products");

            entity.HasOne(d => d.Unit).WithMany(p => p.ProductUnits)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductUnits_Units");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasIndex(e => e.SupplierId, "IX_PurchaseOrders_SupplierId");

            entity.HasIndex(e => new { e.BranchId, e.Ponumber }, "UQ_PurchaseOrders_Branch_PONumber").IsUnique();

            entity.Property(e => e.Podate).HasColumnName("PODate");
            entity.Property(e => e.Ponumber)
                .HasMaxLength(50)
                .HasColumnName("PONumber");
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.Branch).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrders_Branches");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrders_Suppliers");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrders_Warehouses");
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.FreeQuantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.NetAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Product).WithMany(p => p.PurchaseOrderLines)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrderLines_Products");

            entity.HasOne(d => d.ProductUnit).WithMany(p => p.PurchaseOrderLines)
                .HasForeignKey(d => d.ProductUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrderLines_ProductUnits");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.PurchaseOrderLines)
                .HasForeignKey(d => d.PurchaseOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseOrderLines_PO");
        });

        modelBuilder.Entity<ReasonCode>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.ReasonType, e.Code }, "UQ_ReasonCodes_Tenant_Type_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.ReasonType).HasMaxLength(50);

            entity.HasOne(d => d.Tenant).WithMany(p => p.ReasonCodes)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReasonCodes_Tenants");
        });

        modelBuilder.Entity<Refill>(entity =>
        {
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 6)");

            entity.HasOne(d => d.PrescriptionItem).WithMany(p => p.Refills)
                .HasForeignKey(d => d.PrescriptionItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Refills_PrescriptionItems");

            entity.HasOne(d => d.Sale).WithMany(p => p.Refills)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Refills_Sales");
        });

        modelBuilder.Entity<ReorderRule>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaximumStock).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.MinimumStock).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.ReorderPoint).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.ReorderQuantity).HasColumnType("decimal(19, 6)");

            entity.HasOne(d => d.Branch).WithMany(p => p.ReorderRules)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReorderRules_Branches");

            entity.HasOne(d => d.PreferredSupplier).WithMany(p => p.ReorderRules)
                .HasForeignKey(d => d.PreferredSupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReorderRules_Suppliers");

            entity.HasOne(d => d.Product).WithMany(p => p.ReorderRules)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReorderRules_Products");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.ReorderRules)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReorderRules_Warehouses");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Name }, "UQ_Roles_Tenant_Name").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Roles)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Roles_Tenants");

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RolePermissions_Permissions"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RolePermissions_Roles"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId");
                        j.ToTable("RolePermissions");
                    });
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasIndex(e => new { e.BranchId, e.SaleDate }, "IX_Sales_Branch_SaleDate");

            entity.HasIndex(e => new { e.BranchId, e.InvoiceNumber }, "UQ_Sales_Branch_Invoice").IsUnique();

            entity.Property(e => e.ChangeAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("PKR")
                .IsFixedLength();
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.DueAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Fbrstatus)
                .HasMaxLength(30)
                .HasColumnName("FBRStatus");
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
            entity.Property(e => e.NetAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.PaymentStatus).HasMaxLength(30);
            entity.Property(e => e.RoundOff).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SaleType).HasMaxLength(30);
            entity.Property(e => e.Status).HasMaxLength(30);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Branch).WithMany(p => p.Sales)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sales_Branches");

            entity.HasOne(d => d.Counter).WithMany(p => p.Sales)
                .HasForeignKey(d => d.CounterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sales_Counters");

            entity.HasOne(d => d.Customer).WithMany(p => p.Sales)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Sales_Customers");

            entity.HasOne(d => d.Terminal).WithMany(p => p.Sales)
                .HasForeignKey(d => d.TerminalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sales_Terminals");

            entity.HasOne(d => d.User).WithMany(p => p.Sales)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sales_Users");
        });

        modelBuilder.Entity<SaleLine>(entity =>
        {
            entity.HasIndex(e => e.SaleId, "IX_SaleLines_SaleId");

            entity.Property(e => e.BaseQuantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.ConversionFactor).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Mrp)
                .HasColumnType("decimal(19, 4)")
                .HasColumnName("MRP");
            entity.Property(e => e.NetAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.PrescriptionItem).WithMany(p => p.SaleLines)
                .HasForeignKey(d => d.PrescriptionItemId)
                .HasConstraintName("FK_SaleLines_PrescriptionItems");

            entity.HasOne(d => d.Product).WithMany(p => p.SaleLines)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleLines_Products");

            entity.HasOne(d => d.ProductUnit).WithMany(p => p.SaleLines)
                .HasForeignKey(d => d.ProductUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleLines_ProductUnits");

            entity.HasOne(d => d.Sale).WithMany(p => p.SaleLines)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleLines_Sales");
        });

        modelBuilder.Entity<SaleLineBatch>(entity =>
        {
            entity.HasIndex(e => e.SaleLineId, "IX_SaleLineBatches_SaleLineId");

            entity.Property(e => e.BaseQuantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Batch).WithMany(p => p.SaleLineBatches)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SLB_Batches");

            entity.HasOne(d => d.SaleLine).WithMany(p => p.SaleLineBatches)
                .HasForeignKey(d => d.SaleLineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SLB_SaleLines");
        });

        modelBuilder.Entity<SalePayment>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.ReferenceNumber).HasMaxLength(150);
            entity.Property(e => e.Status).HasMaxLength(30);
            entity.Property(e => e.TransactionType).HasMaxLength(30);

            entity.HasOne(d => d.ParentPayment).WithMany(p => p.InverseParentPayment)
                .HasForeignKey(d => d.ParentPaymentId)
                .HasConstraintName("FK_SalePayments_Parent");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.SalePayments)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalePayments_Methods");

            entity.HasOne(d => d.Sale).WithMany(p => p.SalePayments)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalePayments_Sales");
        });

        modelBuilder.Entity<SaleReturn>(entity =>
        {
            entity.HasIndex(e => new { e.BranchId, e.ReturnNumber }, "UQ_SaleReturns_Branch_Number").IsUnique();

            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RefundAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.ReturnNumber).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.Branch).WithMany(p => p.SaleReturns)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleReturns_Branches");

            entity.HasOne(d => d.RefundPaymentMethod).WithMany(p => p.SaleReturns)
                .HasForeignKey(d => d.RefundPaymentMethodId)
                .HasConstraintName("FK_SaleReturns_PaymentMethods");

            entity.HasOne(d => d.Sale).WithMany(p => p.SaleReturns)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleReturns_Sales");
        });

        modelBuilder.Entity<SaleReturnLine>(entity =>
        {
            entity.Property(e => e.Condition).HasMaxLength(50);
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.RefundPrice).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.ReturnToStock).HasDefaultValue(true);

            entity.HasOne(d => d.Batch).WithMany(p => p.SaleReturnLines)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleReturnLines_Batches");

            entity.HasOne(d => d.DestinationLocation).WithMany(p => p.SaleReturnLines)
                .HasForeignKey(d => d.DestinationLocationId)
                .HasConstraintName("FK_SaleReturnLines_Locations");

            entity.HasOne(d => d.Product).WithMany(p => p.SaleReturnLines)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleReturnLines_Products");

            entity.HasOne(d => d.ProductUnit).WithMany(p => p.SaleReturnLines)
                .HasForeignKey(d => d.ProductUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleReturnLines_ProductUnits");

            entity.HasOne(d => d.SaleLine).WithMany(p => p.SaleReturnLines)
                .HasForeignKey(d => d.SaleLineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleReturnLines_SaleLines");

            entity.HasOne(d => d.SaleReturn).WithMany(p => p.SaleReturnLines)
                .HasForeignKey(d => d.SaleReturnId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleReturnLines_Returns");
        });

        modelBuilder.Entity<StockAdjustment>(entity =>
        {
            entity.HasIndex(e => new { e.BranchId, e.AdjustmentNumber }, "UQ_StockAdjustments_Branch_Number").IsUnique();

            entity.Property(e => e.AdjustmentNumber).HasMaxLength(50);
            entity.Property(e => e.AdjustmentType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.Branch).WithMany(p => p.StockAdjustments)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockAdjustments_Branches");

            entity.HasOne(d => d.ReasonCode).WithMany(p => p.StockAdjustments)
                .HasForeignKey(d => d.ReasonCodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SA_ReasonCodes");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.StockAdjustments)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockAdjustments_Warehouses");
        });

        modelBuilder.Entity<StockAdjustmentLine>(entity =>
        {
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Batch).WithMany(p => p.StockAdjustmentLines)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SAL_Batches");

            entity.HasOne(d => d.Product).WithMany(p => p.StockAdjustmentLines)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SAL_Products");

            entity.HasOne(d => d.StockAdjustment).WithMany(p => p.StockAdjustmentLines)
                .HasForeignKey(d => d.StockAdjustmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SAL_Adjustments");

            entity.HasOne(d => d.WarehouseLocation).WithMany(p => p.StockAdjustmentLines)
                .HasForeignKey(d => d.WarehouseLocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SAL_Locations");
        });

        modelBuilder.Entity<StockCount>(entity =>
        {
            entity.HasIndex(e => new { e.BranchId, e.CountNumber }, "UQ_StockCounts_Branch_Number").IsUnique();

            entity.Property(e => e.CountNumber).HasMaxLength(50);
            entity.Property(e => e.CountType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.Branch).WithMany(p => p.StockCounts)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockCounts_Branches");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.StockCounts)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockCounts_Warehouses");
        });

        modelBuilder.Entity<StockCountLine>(entity =>
        {
            entity.Property(e => e.CountedQuantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.SystemQuantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.VarianceQuantity).HasColumnType("decimal(19, 6)");

            entity.HasOne(d => d.Batch).WithMany(p => p.StockCountLines)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SCL_Batches");

            entity.HasOne(d => d.Product).WithMany(p => p.StockCountLines)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SCL_Products");

            entity.HasOne(d => d.ReasonCode).WithMany(p => p.StockCountLines)
                .HasForeignKey(d => d.ReasonCodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SCL_ReasonCodes");

            entity.HasOne(d => d.StockCount).WithMany(p => p.StockCountLines)
                .HasForeignKey(d => d.StockCountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SCL_StockCounts");

            entity.HasOne(d => d.WarehouseLocation).WithMany(p => p.StockCountLines)
                .HasForeignKey(d => d.WarehouseLocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SCL_Locations");
        });

        modelBuilder.Entity<StockTransfer>(entity =>
        {
            entity.HasIndex(e => e.TransferNumber, "UQ_StockTransfers_Number").IsUnique();

            entity.Property(e => e.Status).HasMaxLength(30);
            entity.Property(e => e.TransferNumber).HasMaxLength(50);

            entity.HasOne(d => d.FromWarehouse).WithMany(p => p.StockTransferFromWarehouses)
                .HasForeignKey(d => d.FromWarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ST_FromWarehouse");

            entity.HasOne(d => d.ToWarehouse).WithMany(p => p.StockTransferToWarehouses)
                .HasForeignKey(d => d.ToWarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ST_ToWarehouse");
        });

        modelBuilder.Entity<StockTransferLine>(entity =>
        {
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.ReceivedQuantity).HasColumnType("decimal(19, 6)");

            entity.HasOne(d => d.Batch).WithMany(p => p.StockTransferLines)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_STL_Batches");

            entity.HasOne(d => d.FromLocation).WithMany(p => p.StockTransferLineFromLocations)
                .HasForeignKey(d => d.FromLocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_STL_FromLocation");

            entity.HasOne(d => d.Product).WithMany(p => p.StockTransferLines)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_STL_Products");

            entity.HasOne(d => d.ProductUnit).WithMany(p => p.StockTransferLines)
                .HasForeignKey(d => d.ProductUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_STL_ProductUnits");

            entity.HasOne(d => d.StockTransfer).WithMany(p => p.StockTransferLines)
                .HasForeignKey(d => d.StockTransferId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_STL_Transfers");

            entity.HasOne(d => d.ToLocation).WithMany(p => p.StockTransferLineToLocations)
                .HasForeignKey(d => d.ToLocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_STL_ToLocation");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.Code }, "UQ_Suppliers_Tenant_Code").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CompanyName).HasMaxLength(250);
            entity.Property(e => e.CreditLimit).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.DrugLicenseNo).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Ntn)
                .HasMaxLength(50)
                .HasColumnName("NTN");
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Strn)
                .HasMaxLength(50)
                .HasColumnName("STRN");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Suppliers)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Suppliers_Tenants");
        });

        modelBuilder.Entity<SupplierContact>(entity =>
        {
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);

            entity.HasOne(d => d.Supplier).WithMany(p => p.SupplierContacts)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupplierContacts_Suppliers");
        });

        modelBuilder.Entity<SupplierLedger>(entity =>
        {
            entity.ToTable("SupplierLedger");

            entity.HasIndex(e => new { e.SupplierId, e.SequenceNo }, "IX_SupplierLedger_Supplier");

            entity.Property(e => e.Credit).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Debit).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.ReferenceType).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.Branch).WithMany(p => p.SupplierLedgers)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupplierLedger_Branches");

            entity.HasOne(d => d.Supplier).WithMany(p => p.SupplierLedgers)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupplierLedger_Suppliers");
        });

        modelBuilder.Entity<SupplierPayment>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);

            entity.HasOne(d => d.Branch).WithMany(p => p.SupplierPayments)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupplierPayments_Branches");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.SupplierPayments)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupplierPayments_Methods");

            entity.HasOne(d => d.Supplier).WithMany(p => p.SupplierPayments)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupplierPayments_Suppliers");
        });

        modelBuilder.Entity<SupplierReturn>(entity =>
        {
            entity.HasIndex(e => new { e.BranchId, e.ReturnNumber }, "UQ_SupplierReturns_Branch_Number").IsUnique();

            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.ReturnNumber).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(30);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Branch).WithMany(p => p.SupplierReturns)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupplierReturns_Branches");

            entity.HasOne(d => d.Supplier).WithMany(p => p.SupplierReturns)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupplierReturns_Suppliers");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.SupplierReturns)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupplierReturns_Warehouses");
        });

        modelBuilder.Entity<SupplierReturnLine>(entity =>
        {
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Batch).WithMany(p => p.SupplierReturnLines)
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SRL_Batches");

            entity.HasOne(d => d.GoodsReceiptLine).WithMany(p => p.SupplierReturnLines)
                .HasForeignKey(d => d.GoodsReceiptLineId)
                .HasConstraintName("FK_SRL_GoodsReceiptLines");

            entity.HasOne(d => d.Product).WithMany(p => p.SupplierReturnLines)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SRL_Products");

            entity.HasOne(d => d.ProductUnit).WithMany(p => p.SupplierReturnLines)
                .HasForeignKey(d => d.ProductUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SRL_ProductUnits");

            entity.HasOne(d => d.SupplierReturn).WithMany(p => p.SupplierReturnLines)
                .HasForeignKey(d => d.SupplierReturnId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SRL_SupplierReturns");
        });

        modelBuilder.Entity<SyncBatch>(entity =>
        {
            entity.HasIndex(e => new { e.SyncNodeId, e.BatchNumber }, "UQ_SyncBatches_Node_BatchNumber").IsUnique();

            entity.Property(e => e.BatchNumber).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.SyncNode).WithMany(p => p.SyncBatches)
                .HasForeignKey(d => d.SyncNodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SyncBatches_Nodes");
        });

        modelBuilder.Entity<SyncItem>(entity =>
        {
            entity.Property(e => e.EntityName).HasMaxLength(150);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.Operation).HasMaxLength(30);
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.SyncBatch).WithMany(p => p.SyncItems)
                .HasForeignKey(d => d.SyncBatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SyncItems_Batches");
        });

        modelBuilder.Entity<SyncNode>(entity =>
        {
            entity.HasIndex(e => e.NodeCode, "UQ_SyncNodes_NodeCode").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NodeCode).HasMaxLength(100);

            entity.HasOne(d => d.Branch).WithMany(p => p.SyncNodes)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SyncNodes_Branches");

            entity.HasOne(d => d.Tenant).WithMany(p => p.SyncNodes)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SyncNodes_Tenants");

            entity.HasOne(d => d.Terminal).WithMany(p => p.SyncNodes)
                .HasForeignKey(d => d.TerminalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SyncNodes_Terminals");
        });

        modelBuilder.Entity<TaxProfile>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.TaxType).HasMaxLength(50);
        });

        modelBuilder.Entity<TaxRate>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Rate).HasColumnType("decimal(9, 4)");

            entity.HasOne(d => d.TaxProfile).WithMany(p => p.TaxRates)
                .HasForeignKey(d => d.TaxProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaxRates_TaxProfiles");
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LegalName).HasMaxLength(250);
            entity.Property(e => e.LicenseNo).HasMaxLength(100);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Ntn)
                .HasMaxLength(50)
                .HasColumnName("NTN");
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Strn)
                .HasMaxLength(50)
                .HasColumnName("STRN");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<TenantSetting>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.SettingKey }, "UQ_TenantSettings_Tenant_Key").IsUnique();

            entity.Property(e => e.SettingKey).HasMaxLength(150);

            entity.HasOne(d => d.Tenant).WithMany(p => p.TenantSettings)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TenantSettings_Tenants");
        });

        modelBuilder.Entity<TherapeuticClass>(entity =>
        {
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasIndex(e => e.ShortCode, "UQ_Units_ShortCode").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ShortCode).HasMaxLength(30);
            entity.Property(e => e.UnitType).HasMaxLength(50);
        });

        modelBuilder.Entity<UnitConversion>(entity =>
        {
            entity.Property(e => e.ConversionFactor).HasColumnType("decimal(19, 6)");
            entity.Property(e => e.IsExact).HasDefaultValue(true);

            entity.HasOne(d => d.FromUnit).WithMany(p => p.UnitConversionFromUnits)
                .HasForeignKey(d => d.FromUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UnitConversions_FromUnit");

            entity.HasOne(d => d.Product).WithMany(p => p.UnitConversions)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UnitConversions_Products");

            entity.HasOne(d => d.ToUnit).WithMany(p => p.UnitConversionToUnits)
                .HasForeignKey(d => d.ToUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UnitConversions_ToUnit");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.TenantId, "IX_Users_TenantId").HasFilter("([IsActive]=(1))");

            entity.HasIndex(e => new { e.TenantId, e.Username }, "UQ_Users_Tenant_Username").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasOne(d => d.Tenant).WithMany(p => p.Users)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Tenants");

            entity.HasMany(d => d.Branches).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserBranch",
                    r => r.HasOne<Branch>().WithMany()
                        .HasForeignKey("BranchId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserBranches_Branches"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserBranches_Users"),
                    j =>
                    {
                        j.HasKey("UserId", "BranchId");
                        j.ToTable("UserBranches");
                    });

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRoles_Roles"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRoles_Users"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("UserRoles");
                    });
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_Warehouses_BranchId");

            entity.HasIndex(e => new { e.BranchId, e.Code }, "UQ_Warehouses_Branch_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.WarehouseType).HasMaxLength(50);

            entity.HasOne(d => d.Branch).WithMany(p => p.Warehouses)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Warehouses_Branches");
        });

        modelBuilder.Entity<WarehouseLocation>(entity =>
        {
            entity.HasIndex(e => e.WarehouseId, "IX_WarehouseLocations_WarehouseId");

            entity.HasIndex(e => new { e.WarehouseId, e.Code }, "UQ_WarehouseLocations_Warehouse_Code").IsUnique();

            entity.Property(e => e.BinNo).HasMaxLength(50);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LocationType).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.RackNo).HasMaxLength(50);
            entity.Property(e => e.ShelfNo).HasMaxLength(50);

            entity.HasOne(d => d.Warehouse).WithMany(p => p.WarehouseLocations)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WarehouseLocations_Warehouses");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
