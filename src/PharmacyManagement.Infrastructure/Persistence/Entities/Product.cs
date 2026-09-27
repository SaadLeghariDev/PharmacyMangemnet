using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Product
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public long CategoryId { get; set; }

    public long ManufacturerId { get; set; }

    public long BrandId { get; set; }

    public long TherapeuticClassId { get; set; }

    public string Sku { get; set; } = null!;

    public string? ProductCode { get; set; }

    public string Name { get; set; } = null!;

    public string? GenericName { get; set; }

    public string? Form { get; set; }

    public string? Strength { get; set; }

    public string? StrengthUnit { get; set; }

    public string? PackDescription { get; set; }

    public bool PrescriptionRequired { get; set; }

    public bool IsControlled { get; set; }

    public bool IsTemperatureSensitive { get; set; }

    public bool IsRefrigerated { get; set; }

    public bool IsReturnable { get; set; }

    public bool IsSaleable { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public virtual ICollection<BarcodePrintJob> BarcodePrintJobs { get; set; } = new List<BarcodePrintJob>();

    public virtual ICollection<Barcode> Barcodes { get; set; } = new List<Barcode>();

    public virtual Brand Brand { get; set; } = null!;

    public virtual ProductCategory Category { get; set; } = null!;

    public virtual ICollection<ControlledDrugRegister> ControlledDrugRegisters { get; set; } = new List<ControlledDrugRegister>();

    public virtual ICollection<DispensingItem> DispensingItems { get; set; } = new List<DispensingItem>();

    public virtual ICollection<GoodsReceiptLine> GoodsReceiptLines { get; set; } = new List<GoodsReceiptLine>();

    public virtual ICollection<InventoryBatch> InventoryBatches { get; set; } = new List<InventoryBatch>();

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();

    public virtual ICollection<ProductAlias> ProductAliases { get; set; } = new List<ProductAlias>();

    public virtual ICollection<ProductIngredient> ProductIngredients { get; set; } = new List<ProductIngredient>();

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    public virtual ICollection<ProductRegulatoryProfile> ProductRegulatoryProfiles { get; set; } = new List<ProductRegulatoryProfile>();

    public virtual ICollection<ProductUnit> ProductUnits { get; set; } = new List<ProductUnit>();

    public virtual ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = new List<PurchaseOrderLine>();

    public virtual ICollection<ReorderRule> ReorderRules { get; set; } = new List<ReorderRule>();

    public virtual ICollection<SaleLine> SaleLines { get; set; } = new List<SaleLine>();

    public virtual ICollection<SaleReturnLine> SaleReturnLines { get; set; } = new List<SaleReturnLine>();

    public virtual ICollection<StockAdjustmentLine> StockAdjustmentLines { get; set; } = new List<StockAdjustmentLine>();

    public virtual ICollection<StockCountLine> StockCountLines { get; set; } = new List<StockCountLine>();

    public virtual ICollection<StockTransferLine> StockTransferLines { get; set; } = new List<StockTransferLine>();

    public virtual ICollection<SupplierReturnLine> SupplierReturnLines { get; set; } = new List<SupplierReturnLine>();

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual TherapeuticClass TherapeuticClass { get; set; } = null!;

    public virtual ICollection<UnitConversion> UnitConversions { get; set; } = new List<UnitConversion>();

    public virtual ICollection<TaxProfile> TaxProfiles { get; set; } = new List<TaxProfile>();
}
