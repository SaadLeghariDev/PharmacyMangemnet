using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PharmacyManagement.Application.DTOs.Alerts;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class Phase7ReorderAlertsTests
{
    private sealed class Fixture
    {
        public PharmacyManagementDbContext Db { get; init; } = null!;
        public long TenantId { get; init; }
        public long BranchId { get; init; }
        public long WarehouseId { get; init; }
        public long LocationId { get; init; }
        public long UserId { get; init; }
        public long SupplierId { get; init; }
        public long ProductId { get; init; }
        public long BaseUnitId { get; init; }
        public GoodsReceiptService Grn { get; init; } = null!;
        public ReorderRuleService ReorderRules { get; init; } = null!;
        public AlertRuleService AlertRules { get; init; } = null!;
        public AlertService Alerts { get; init; } = null!;
        public NotificationTemplateService Templates { get; init; } = null!;
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
            UserId = user.Id,
            SupplierId = supplier.Id,
            ProductId = product.Id,
            BaseUnitId = basePu.Id,
            Grn = new GoodsReceiptService(db, current.Object, sequences),
            ReorderRules = new ReorderRuleService(db, current.Object),
            AlertRules = new AlertRuleService(db, current.Object),
            Alerts = new AlertService(db, current.Object),
            Templates = new NotificationTemplateService(db, current.Object)
        };
    }

    private static async Task SeedStockAsync(Fixture fx, decimal qty, DateOnly? expiry = null)
    {
        var draft = await fx.Grn.CreateDraftAsync(new CreateGoodsReceiptRequest
        {
            BranchId = fx.BranchId,
            WarehouseId = fx.WarehouseId,
            SupplierId = fx.SupplierId,
            Lines =
            [
                new GoodsReceiptLineRequest
                {
                    ProductId = fx.ProductId,
                    ProductUnitId = fx.BaseUnitId,
                    ReceivedQuantity = qty,
                    UnitCost = 2,
                    Batches =
                    [
                        new GoodsReceiptLineBatchRequest
                        {
                            BatchNumber = "B1",
                            ExpiryDate = expiry ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
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
    public async Task ReorderRule_create_and_low_stock_candidate_when_below_reorder_point()
    {
        var fx = await SeedAsync();
        await SeedStockAsync(fx, qty: 8);

        var rule = await fx.ReorderRules.CreateAsync(new CreateReorderRuleRequest
        {
            BranchId = fx.BranchId,
            WarehouseId = fx.WarehouseId,
            ProductId = fx.ProductId,
            MinimumStock = 5,
            MaximumStock = 100,
            ReorderPoint = 20,
            ReorderQuantity = 50,
            PreferredSupplierId = fx.SupplierId,
            IsActive = true
        });

        rule.Id.Should().BeGreaterThan(0);
        rule.ProductSku.Should().Be("PARA-500");

        var candidates = await fx.ReorderRules.GetLowStockCandidatesAsync(new LowStockCandidateQuery());
        candidates.TotalCount.Should().Be(1);
        candidates.Items[0].AvailableQuantity.Should().Be(8);
        candidates.Items[0].ShortageQuantity.Should().Be(12);
        candidates.Items[0].ReorderRuleId.Should().Be(rule.Id);
    }

    [Fact]
    public async Task Evaluate_creates_low_stock_and_expiry_alerts_then_ack_resolve()
    {
        var fx = await SeedAsync();
        await SeedStockAsync(fx, qty: 3, expiry: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)));

        await fx.ReorderRules.CreateAsync(new CreateReorderRuleRequest
        {
            BranchId = fx.BranchId,
            WarehouseId = fx.WarehouseId,
            ProductId = fx.ProductId,
            MinimumStock = 5,
            MaximumStock = 100,
            ReorderPoint = 10,
            ReorderQuantity = 40,
            PreferredSupplierId = fx.SupplierId
        });

        var lowStockRule = await fx.AlertRules.CreateAsync(new CreateAlertRuleRequest
        {
            BranchId = fx.BranchId,
            AlertType = AlertTypes.LowStock,
            Threshold = 10,
            IsActive = true
        });
        var expiryRule = await fx.AlertRules.CreateAsync(new CreateAlertRuleRequest
        {
            BranchId = fx.BranchId,
            AlertType = AlertTypes.Expiry,
            DaysBeforeExpiry = 30,
            IsActive = true
        });

        await fx.Templates.CreateAsync(new CreateNotificationTemplateRequest
        {
            Code = "ALERT_LowStock",
            Channel = "InApp",
            Subject = "Low stock",
            Body = "Stock below reorder point",
            IsActive = true
        });

        var result = await fx.Alerts.EvaluateAsync();
        result.RulesScanned.Should().Be(2);
        result.AlertsCreated.Should().Be(2);
        result.NotificationLogsCreated.Should().Be(1);

        var alerts = await fx.Alerts.SearchAsync(new AlertQuery());
        alerts.TotalCount.Should().Be(2);
        alerts.Items.Should().Contain(a => a.AlertType == AlertTypes.LowStock && a.Status == AlertStatuses.Open);
        alerts.Items.Should().Contain(a => a.AlertType == AlertTypes.Expiry && a.BatchId != null);

        var lowStock = alerts.Items.First(a => a.AlertType == AlertTypes.LowStock);
        lowStock.Severity.Should().Be(AlertSeverities.Critical); // 3 <= MinimumStock 5
        lowStock.AlertRuleId.Should().Be(lowStockRule.Id);

        var acknowledged = await fx.Alerts.AcknowledgeAsync(lowStock.Id);
        acknowledged.Status.Should().Be(AlertStatuses.Acknowledged);

        var resolved = await fx.Alerts.ResolveAsync(lowStock.Id);
        resolved.Status.Should().Be(AlertStatuses.Resolved);
        resolved.ResolvedBy.Should().Be(fx.UserId);
        resolved.ResolvedAt.Should().NotBeNull();

        // Second evaluate should not duplicate open/ack for same product (resolved allows new)
        var again = await fx.Alerts.EvaluateAsync();
        again.AlertsCreated.Should().BeGreaterThanOrEqualTo(1); // low stock again + maybe not expiry if still open
        expiryRule.Id.Should().BeGreaterThan(0);

        var logs = await fx.Templates.SearchLogsAsync(new NotificationLogQuery { ReferenceType = "Alert" });
        logs.TotalCount.Should().BeGreaterThanOrEqualTo(1);
        logs.Items[0].Status.Should().Be(NotificationLogStatuses.Pending);
    }

    [Fact]
    public async Task Evaluate_does_not_duplicate_open_alerts()
    {
        var fx = await SeedAsync();
        await SeedStockAsync(fx, qty: 1);

        await fx.ReorderRules.CreateAsync(new CreateReorderRuleRequest
        {
            BranchId = fx.BranchId,
            WarehouseId = fx.WarehouseId,
            ProductId = fx.ProductId,
            MinimumStock = 0,
            MaximumStock = 50,
            ReorderPoint = 5,
            ReorderQuantity = 20,
            PreferredSupplierId = fx.SupplierId
        });
        await fx.AlertRules.CreateAsync(new CreateAlertRuleRequest
        {
            AlertType = AlertTypes.LowStock,
            IsActive = true
        });

        var first = await fx.Alerts.EvaluateAsync();
        first.AlertsCreated.Should().Be(1);

        var second = await fx.Alerts.EvaluateAsync();
        second.AlertsCreated.Should().Be(0);
    }

    [Fact]
    public void Permission_codes_include_reorder_and_alerts()
    {
        PermissionCodes.InvReorder.Should().Be("INV.REORDER");
        PermissionCodes.AlertView.Should().Be("ALERT.VIEW");
        PermissionCodes.AlertManage.Should().Be("ALERT.MANAGE");
        AlertTypes.IsKnown("LowStock").Should().BeTrue();
        AlertTypes.IsKnown("Expiry").Should().BeTrue();
        AlertStatuses.Open.Should().Be("Open");
        AlertSeverities.Critical.Should().Be("Critical");
    }
}
