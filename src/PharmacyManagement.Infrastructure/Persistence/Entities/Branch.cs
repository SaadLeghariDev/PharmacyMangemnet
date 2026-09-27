using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Branch
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? BranchType { get; set; }

    public string? Ntn { get; set; }

    public string? Strn { get; set; }

    public string? DrugLicenseNo { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Province { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public virtual ICollection<BarcodePrintJob> BarcodePrintJobs { get; set; } = new List<BarcodePrintJob>();

    public virtual ICollection<BranchSetting> BranchSettings { get; set; } = new List<BranchSetting>();

    public virtual ICollection<CashShift> CashShifts { get; set; } = new List<CashShift>();

    public virtual ICollection<ControlledDrugRegister> ControlledDrugRegisters { get; set; } = new List<ControlledDrugRegister>();

    public virtual ICollection<Counter> Counters { get; set; } = new List<Counter>();

    public virtual ICollection<CustomerLedger> CustomerLedgers { get; set; } = new List<CustomerLedger>();

    public virtual ICollection<CustomerPayment> CustomerPayments { get; set; } = new List<CustomerPayment>();

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<FiscalConfiguration> FiscalConfigurations { get; set; } = new List<FiscalConfiguration>();

    public virtual ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();

    public virtual ICollection<HeldSale> HeldSales { get; set; } = new List<HeldSale>();

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();

    public virtual ICollection<NumberSequence> NumberSequences { get; set; } = new List<NumberSequence>();

    public virtual ICollection<Posterminal> Posterminals { get; set; } = new List<Posterminal>();

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<ReorderRule> ReorderRules { get; set; } = new List<ReorderRule>();

    public virtual ICollection<SaleReturn> SaleReturns { get; set; } = new List<SaleReturn>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();

    public virtual ICollection<StockCount> StockCounts { get; set; } = new List<StockCount>();

    public virtual ICollection<SupplierLedger> SupplierLedgers { get; set; } = new List<SupplierLedger>();

    public virtual ICollection<SupplierPayment> SupplierPayments { get; set; } = new List<SupplierPayment>();

    public virtual ICollection<SupplierReturn> SupplierReturns { get; set; } = new List<SupplierReturn>();

    public virtual ICollection<SyncNode> SyncNodes { get; set; } = new List<SyncNode>();

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
