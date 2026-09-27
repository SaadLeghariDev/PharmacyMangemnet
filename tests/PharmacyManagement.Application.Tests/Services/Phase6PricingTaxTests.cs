using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PharmacyManagement.Application.DTOs.Pricing;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.DTOs.Tax;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class Phase6PricingTaxTests
{
    private sealed class Fixture
    {
        public PharmacyManagementDbContext Db { get; init; } = null!;
        public long TenantId { get; init; }
        public long BranchId { get; init; }
        public long WarehouseId { get; init; }
        public long LocationId { get; init; }
        public long CounterId { get; init; }
        public long TerminalId { get; init; }
        public long UserId { get; init; }
        public long SupplierId { get; init; }
        public long ProductId { get; init; }
        public long BaseUnitId { get; init; }
        public long CashMethodId { get; init; }
        public SaleService Sales { get; init; } = null!;
        public GoodsReceiptService Grn { get; init; } = null!;
        public PriceListService PriceLists { get; init; } = null!;
        public ProductPriceService ProductPrices { get; init; } = null!;
        public TaxProfileService TaxProfiles { get; init; } = null!;
    }

    private static async Task<Fixture> SeedAsync()
    {
        var options = new DbContextOptionsBuilder<PharmacyManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new PharmacyManagementDbContext(options);

        var tenant = new Tenant
        {
            Name = "Demo",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var branch = new Branch
        {
            TenantId = tenant.Id,
            Code = "MAIN",
            Name = "Main",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }
        };
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        var user = new User
        {
            TenantId = tenant.Id,
            Username = "admin",
            Email = "a@test.local",
            PasswordHash = "x",
            FullName = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }
        };
        db.Users.Add(user);

        var wh = new Warehouse
        {
            BranchId = branch.Id,
            Code = "WH1",
            Name = "Main WH",
            IsMain = true,
            IsActive = true
        };
        db.Warehouses.Add(wh);
        await db.SaveChangesAsync();

        var loc = new WarehouseLocation
        {
            WarehouseId = wh.Id,
            Code = "A-01",
            Name = "Aisle",
            LocationType = "Selling",
            IsActive = true
        };
        db.WarehouseLocations.Add(loc);

        var counter = new Counter { BranchId = branch.Id, Code = "C1", Name = "Counter 1", IsActive = true };
        db.Counters.Add(counter);
        await db.SaveChangesAsync();

        var terminal = new Posterminal
        {
            BranchId = branch.Id,
            CounterId = counter.Id,
            TerminalCode = "T1",
            IsPrimary = true,
            IsOnline = true,
            IsActive = true
        };
        db.Posterminals.Add(terminal);

        var cash = new PaymentMethod { Name = "Cash", Code = "CASH", Type = "Cash", IsActive = true };
        db.PaymentMethods.Add(cash);

        var supplier = new Supplier
        {
            TenantId = tenant.Id,
            Code = "SUP01",
            Name = "Supplier",
            CreditLimit = 100000,
            PaymentTermsDays = 30,
            IsActive = true
        };
        db.Suppliers.Add(supplier);

        var mfr = new Manufacturer { Name = "Acme", IsActive = true };
        db.Manufacturers.Add(mfr);
        await db.SaveChangesAsync();
        var brand = new Brand { ManufacturerId = mfr.Id, Name = "Brand", IsActive = true };
        var cat = new ProductCategory { Name = "OTC", IsActive = true };
        var tc = new TherapeuticClass { Name = "Analgesic" };
        db.Brands.Add(brand);
        db.ProductCategories.Add(cat);
        db.TherapeuticClasses.Add(tc);
        await db.SaveChangesAsync();

        var product = new Product
        {
            TenantId = tenant.Id,
            CategoryId = cat.Id,
            ManufacturerId = mfr.Id,
            BrandId = brand.Id,
            TherapeuticClassId = tc.Id,
            Sku = "PARA-500",
            Name = "Paracetamol 500mg",
            IsActive = true,
            IsSaleable = true,
            IsReturnable = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var unitTab = new Unit { ShortCode = "TAB", Name = "Tablet" };
        db.Units.Add(unitTab);
        await db.SaveChangesAsync();

        var basePu = new ProductUnit
        {
            ProductId = product.Id,
            UnitId = unitTab.Id,
            IsBaseUnit = true,
            IsSaleUnit = true,
            IsPurchaseUnit = true,
            ConversionToBase = 1,
            IsActive = true
        };
        db.ProductUnits.Add(basePu);

        db.NumberSequences.Add(new NumberSequence
        {
            TenantId = tenant.Id,
            BranchId = branch.Id,
            DocumentType = DocumentTypes.GoodsReceipt,
            Prefix = "GRN-",
            CurrentNumber = 0,
            NumberLength = 6
        });
        db.NumberSequences.Add(new NumberSequence
        {
            TenantId = tenant.Id,
            BranchId = branch.Id,
            TerminalId = terminal.Id,
            DocumentType = DocumentTypes.Sale,
            Prefix = "INV-",
            CurrentNumber = 0,
            NumberLength = 6
        });
        await db.SaveChangesAsync();

        var current = new Mock<ICurrentUserService>();
        current.SetupGet(c => c.TenantId).Returns(tenant.Id);
        current.SetupGet(c => c.UserId).Returns(user.Id);

        var sequences = new NumberSequenceService(db);
        return new Fixture
        {
            Db = db,
            TenantId = tenant.Id,
            BranchId = branch.Id,
            WarehouseId = wh.Id,
            LocationId = loc.Id,
            CounterId = counter.Id,
            TerminalId = terminal.Id,
            UserId = user.Id,
            SupplierId = supplier.Id,
            ProductId = product.Id,
            BaseUnitId = basePu.Id,
            CashMethodId = cash.Id,
            Sales = new SaleService(db, current.Object, sequences),
            Grn = new GoodsReceiptService(db, current.Object, sequences),
            PriceLists = new PriceListService(db, current.Object),
            ProductPrices = new ProductPriceService(db, current.Object),
            TaxProfiles = new TaxProfileService(db, current.Object)
        };
    }

    private static async Task SeedStockAsync(Fixture fx, decimal qty = 100)
    {
        var draft = await fx.Grn.CreateDraftAsync(new Application.DTOs.Purchasing.CreateGoodsReceiptRequest
        {
            BranchId = fx.BranchId,
            WarehouseId = fx.WarehouseId,
            SupplierId = fx.SupplierId,
            Lines =
            [
                new Application.DTOs.Purchasing.GoodsReceiptLineRequest
                {
                    ProductId = fx.ProductId,
                    ProductUnitId = fx.BaseUnitId,
                    ReceivedQuantity = qty,
                    UnitCost = 2,
                    Batches =
                    [
                        new Application.DTOs.Purchasing.GoodsReceiptLineBatchRequest
                        {
                            BatchNumber = "B1",
                            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                            Mrp = 6,
                            SalePrice = 5,
                            Quantity = qty,
                            WarehouseLocationId = fx.LocationId
                        }
                    ]
                }
            ]
        });
        await fx.Grn.PostAsync(draft.Id);
    }

    [Fact]
    public async Task PriceList_setting_default_clears_other_defaults()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            var first = await fx.PriceLists.CreateAsync(new CreatePriceListRequest
            {
                Name = "Retail",
                CurrencyCode = "PKR",
                IsDefault = true,
                IsActive = true
            });
            var second = await fx.PriceLists.CreateAsync(new CreatePriceListRequest
            {
                Name = "Wholesale",
                CurrencyCode = "PKR",
                IsDefault = true,
                IsActive = true
            });

            second.IsDefault.Should().BeTrue();
            var reloadedFirst = await fx.PriceLists.GetByIdAsync(first.Id);
            reloadedFirst!.IsDefault.Should().BeFalse();

            var defaults = await fx.Db.PriceLists.AsNoTracking()
                .CountAsync(p => p.TenantId == fx.TenantId && p.IsDefault);
            defaults.Should().Be(1);
        }
    }

    [Fact]
    public async Task Sale_resolves_unit_price_from_default_price_list()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx, 50);

            var defaultList = await fx.PriceLists.CreateAsync(new CreatePriceListRequest
            {
                Name = "Default",
                CurrencyCode = "PKR",
                IsDefault = true,
                IsActive = true
            });
            var otherList = await fx.PriceLists.CreateAsync(new CreatePriceListRequest
            {
                Name = "Promo",
                CurrencyCode = "PKR",
                IsDefault = false,
                IsActive = true
            });

            await fx.ProductPrices.CreateAsync(new CreateProductPriceRequest
            {
                PriceListId = otherList.Id,
                ProductId = fx.ProductId,
                ProductUnitId = fx.BaseUnitId,
                PurchasePrice = 2,
                SalePrice = 99,
                Mrp = 100,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1)
            });
            await fx.ProductPrices.CreateAsync(new CreateProductPriceRequest
            {
                PriceListId = defaultList.Id,
                ProductId = fx.ProductId,
                ProductUnitId = fx.BaseUnitId,
                PurchasePrice = 2,
                SalePrice = 12,
                Mrp = 15,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1)
            });

            var sale = await fx.Sales.CreateAsync(new CreateSaleRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                WarehouseId = fx.WarehouseId,
                Lines =
                [
                    new CreateSaleLineRequest
                    {
                        ProductId = fx.ProductId,
                        ProductUnitId = fx.BaseUnitId,
                        Quantity = 2
                        // UnitPrice omitted — server resolves from default price list
                    }
                ],
                Payments =
                [
                    new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 24 }
                ]
            });

            sale.Lines[0].UnitPrice.Should().Be(12);
            sale.Subtotal.Should().Be(24);
            sale.TaxAmount.Should().Be(0);
            sale.NetAmount.Should().Be(24);
        }
    }

    [Fact]
    public async Task ProductPrice_effective_window_excludes_expired_rows()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx, 20);

            var list = await fx.PriceLists.CreateAsync(new CreatePriceListRequest
            {
                Name = "Default",
                CurrencyCode = "PKR",
                IsDefault = true,
                IsActive = true
            });

            var expired = await fx.ProductPrices.CreateAsync(new CreateProductPriceRequest
            {
                PriceListId = list.Id,
                ProductId = fx.ProductId,
                ProductUnitId = fx.BaseUnitId,
                PurchasePrice = 1,
                SalePrice = 50,
                Mrp = 55,
                EffectiveFrom = DateTime.UtcNow.AddDays(-30),
                EffectiveTo = DateTime.UtcNow.AddDays(-1)
            });

            await fx.ProductPrices.CreateAsync(new CreateProductPriceRequest
            {
                PriceListId = list.Id,
                ProductId = fx.ProductId,
                ProductUnitId = fx.BaseUnitId,
                PurchasePrice = 1,
                SalePrice = 8,
                Mrp = 10,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1)
            });

            var active = await fx.ProductPrices.SearchAsync(new ProductPriceQuery
            {
                PriceListId = list.Id,
                ProductId = fx.ProductId,
                ActiveOnly = true
            });
            active.Items.Should().HaveCount(1);
            active.Items[0].SalePrice.Should().Be(8);
            active.Items.Should().NotContain(p => p.Id == expired.Id);

            var sale = await fx.Sales.CreateAsync(new CreateSaleRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                WarehouseId = fx.WarehouseId,
                Lines =
                [
                    new CreateSaleLineRequest
                    {
                        ProductId = fx.ProductId,
                        ProductUnitId = fx.BaseUnitId,
                        Quantity = 1
                    }
                ],
                Payments =
                [
                    new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 8 }
                ]
            });

            sale.Lines[0].UnitPrice.Should().Be(8);
        }
    }

    [Fact]
    public async Task Sale_computes_tax_writes_InvoiceTaxes_ignores_client_tax()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx, 50);

            var profile = await fx.TaxProfiles.CreateAsync(new CreateTaxProfileRequest
            {
                Name = "GST 17%",
                TaxType = "GST",
                IsActive = true
            });
            await fx.TaxProfiles.AddRateAsync(profile.Id, new CreateTaxRateRequest
            {
                Rate = 17,
                EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                IsActive = true
            });
            await fx.TaxProfiles.ReplaceProductTaxProfilesAsync(fx.ProductId, new ReplaceProductTaxProfilesRequest
            {
                TaxProfileIds = [profile.Id]
            });

            var sale = await fx.Sales.CreateAsync(new CreateSaleRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                WarehouseId = fx.WarehouseId,
                Lines =
                [
                    new CreateSaleLineRequest
                    {
                        ProductId = fx.ProductId,
                        ProductUnitId = fx.BaseUnitId,
                        Quantity = 10,
                        UnitPrice = 10,
                        TaxAmount = 999 // must be ignored
                    }
                ],
                Payments =
                [
                    new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 117 }
                ]
            });

            sale.Subtotal.Should().Be(100);
            sale.TaxAmount.Should().Be(17);
            sale.NetAmount.Should().Be(117);
            sale.Lines[0].TaxAmount.Should().Be(17);

            var taxes = await fx.Db.InvoiceTaxes.AsNoTracking()
                .Where(t => t.SaleId == sale.Id)
                .ToListAsync();
            taxes.Should().HaveCount(1);
            taxes[0].TaxProfileId.Should().Be(profile.Id);
            taxes[0].TaxRate.Should().Be(17);
            taxes[0].TaxableAmount.Should().Be(100);
            taxes[0].TaxAmount.Should().Be(17);
            taxes[0].SaleLineId.Should().NotBeNull();
        }
    }

    [Fact]
    public void Permission_codes_include_price_and_tax()
    {
        PermissionCodes.PriceView.Should().Be("PRICE.VIEW");
        PermissionCodes.PriceEdit.Should().Be("PRICE.EDIT");
        PermissionCodes.TaxView.Should().Be("TAX.VIEW");
        PermissionCodes.TaxEdit.Should().Be("TAX.EDIT");
    }
}
