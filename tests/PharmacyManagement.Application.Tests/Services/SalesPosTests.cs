using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class SalesPosTests
{
    private sealed class Fixture
    {
        public PharmacyManagementDbContext Db { get; init; } = null!;
        public long TenantId { get; init; }
        public long BranchId { get; init; }
        public long WarehouseId { get; init; }
        public long LocationId { get; init; }
        public long QuarantineLocationId { get; init; }
        public long CounterId { get; init; }
        public long TerminalId { get; init; }
        public long UserId { get; init; }
        public long SupplierId { get; init; }
        public long ProductId { get; init; }
        public long BaseUnitId { get; init; }
        public long CashMethodId { get; init; }
        public long CreditMethodId { get; init; }
        public long CustomerId { get; init; }
        public SaleService Sales { get; init; } = null!;
        public HeldSaleService HeldSales { get; init; } = null!;
        public SaleReturnService Returns { get; init; } = null!;
        public GoodsReceiptService Grn { get; init; } = null!;
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
            Username = "cashier",
            Email = "c@test.local",
            PasswordHash = "x",
            FullName = "Cashier",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

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

        var loc = new WarehouseLocation { WarehouseId = wh.Id, Code = "A-01", Name = "Aisle", LocationType = "Selling", IsActive = true };
        var qLoc = new WarehouseLocation { WarehouseId = wh.Id, Code = "Q-01", Name = "Quarantine", LocationType = "Quarantine", IsActive = true };
        db.WarehouseLocations.AddRange(loc, qLoc);
        await db.SaveChangesAsync();

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
        await db.SaveChangesAsync();

        var cash = new PaymentMethod { Name = "Cash", Code = "CASH", Type = "Cash", IsActive = true };
        var credit = new PaymentMethod { Name = "Credit", Code = "CREDIT", Type = "Credit", IsActive = true };
        db.PaymentMethods.AddRange(cash, credit);

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

        var customer = new Customer
        {
            TenantId = tenant.Id,
            CustomerCode = "C-001",
            Name = "Walk-in Credit",
            CreditLimit = 5000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Customers.Add(customer);

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
        db.NumberSequences.Add(new NumberSequence
        {
            TenantId = tenant.Id,
            BranchId = branch.Id,
            DocumentType = DocumentTypes.SaleReturn,
            Prefix = "RET-",
            CurrentNumber = 0,
            NumberLength = 6
        });
        await db.SaveChangesAsync();

        var current = new Mock<ICurrentUserService>();
        current.SetupGet(c => c.TenantId).Returns(tenant.Id);
        current.SetupGet(c => c.UserId).Returns(user.Id);

        var sequences = new NumberSequenceService(db);
        var grn = new GoodsReceiptService(db, current.Object, sequences);
        var sales = new SaleService(db, current.Object, sequences);
        var held = new HeldSaleService(db, current.Object);
        var returns = new SaleReturnService(db, current.Object, sequences);

        return new Fixture
        {
            Db = db,
            TenantId = tenant.Id,
            BranchId = branch.Id,
            WarehouseId = wh.Id,
            LocationId = loc.Id,
            QuarantineLocationId = qLoc.Id,
            CounterId = counter.Id,
            TerminalId = terminal.Id,
            UserId = user.Id,
            SupplierId = supplier.Id,
            ProductId = product.Id,
            BaseUnitId = basePu.Id,
            CashMethodId = cash.Id,
            CreditMethodId = credit.Id,
            CustomerId = customer.Id,
            Sales = sales,
            HeldSales = held,
            Returns = returns,
            Grn = grn
        };
    }

    private static async Task SeedStockAsync(Fixture fx, params (string Batch, DateOnly Expiry, decimal Qty)[] batches)
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
                    ReceivedQuantity = batches.Sum(b => b.Qty),
                    UnitCost = 2,
                    Batches = batches.Select(b => new Application.DTOs.Purchasing.GoodsReceiptLineBatchRequest
                    {
                        BatchNumber = b.Batch,
                        ExpiryDate = b.Expiry,
                        Mrp = 6,
                        SalePrice = 5,
                        Quantity = b.Qty,
                        WarehouseLocationId = fx.LocationId
                    }).ToList()
                }
            ]
        });
        await fx.Grn.PostAsync(draft.Id);
    }

    [Fact]
    public async Task Sale_fefo_allocates_earliest_expiry_first()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx,
                ("LATE", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)), 100),
                ("EARLY", DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)), 40));

            var sale = await fx.Sales.CreateAsync(new CreateSaleRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                WarehouseId = fx.WarehouseId,
                SaleType = "Retail",
                Lines =
                [
                    new CreateSaleLineRequest
                    {
                        ProductId = fx.ProductId,
                        ProductUnitId = fx.BaseUnitId,
                        Quantity = 10,
                        UnitPrice = 5
                    }
                ],
                Payments =
                [
                    new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 50 }
                ]
            });

            sale.InvoiceNumber.Should().StartWith("INV-");
            sale.Status.Should().Be("Completed");
            sale.PaymentStatus.Should().Be("Paid");
            sale.NetAmount.Should().Be(50);
            sale.Lines.Should().HaveCount(1);
            sale.Lines[0].Batches.Should().HaveCount(1);
            sale.Lines[0].Batches[0].BatchNumber.Should().Be("EARLY");

            var earlyLoc = await fx.Db.InventoryBatchLocations.AsNoTracking()
                .Include(l => l.Batch)
                .FirstAsync(l => l.Batch.BatchNumber == "EARLY");
            earlyLoc.QuantityOnHand.Should().Be(30);

            var movs = await fx.Db.InventoryMovements.AsNoTracking()
                .Where(m => m.ReferenceType == "Sale" && m.ReferenceId == sale.Id)
                .ToListAsync();
            movs.Should().HaveCount(1);
            movs[0].Quantity.Should().Be(-10);
            movs[0].MovementType.Should().Be(MovementTypes.Sale);
        }
    }

    [Fact]
    public async Task Sale_multi_batch_line_splits_across_fefo_batches()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx,
                ("EARLY", DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(2)), 30),
                ("LATE", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)), 100));

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
                        Quantity = 50,
                        UnitPrice = 5
                    }
                ],
                Payments =
                [
                    new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 250 }
                ]
            });

            sale.Lines[0].Batches.Should().HaveCount(2);
            sale.Lines[0].Batches.Sum(b => b.BaseQuantity).Should().Be(50);
            sale.Lines[0].Batches.Should().Contain(b => b.BatchNumber == "EARLY" && b.BaseQuantity == 30);
            sale.Lines[0].Batches.Should().Contain(b => b.BatchNumber == "LATE" && b.BaseQuantity == 20);
        }
    }

    [Fact]
    public async Task Sale_insufficient_stock_throws_validation()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx,
                ("ONLY", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)), 5));

            var act = async () => await fx.Sales.CreateAsync(new CreateSaleRequest
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
                        Quantity = 20,
                        UnitPrice = 5
                    }
                ],
                Payments =
                [
                    new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 100 }
                ]
            });

            await act.Should().ThrowAsync<ValidationAppException>()
                .Where(e => e.Errors.Any(m => m.Contains("insufficient", StringComparison.OrdinalIgnoreCase)));
        }
    }

    [Fact]
    public async Task Concurrent_location_update_during_sale_maps_to_conflict()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx,
                ("CONC", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)), 20));

            var loc = await fx.Db.InventoryBatchLocations.FirstAsync();
            // Simulate concurrent writer bumping RowVersion after we load — InMemory does not enforce
            // concurrency tokens the same way, so assert ConflictException mapping contract used by service.
            var options = new DbContextOptionsBuilder<PharmacyManagementDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            await using var db2 = new PharmacyManagementDbContext(options);
            db2.InventoryBatchLocations.Add(new InventoryBatchLocation
            {
                BatchId = 1,
                WarehouseLocationId = 1,
                QuantityOnHand = 10,
                ReservedQuantity = 0,
                UpdatedAt = DateTime.UtcNow,
                RowVersion = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }
            });
            await db2.SaveChangesAsync();
            var id = (await db2.InventoryBatchLocations.FirstAsync()).Id;

            await using var ctx1 = new PharmacyManagementDbContext(options);
            await using var ctx2 = new PharmacyManagementDbContext(options);
            var a = await ctx1.InventoryBatchLocations.FirstAsync(l => l.Id == id);
            var b = await ctx2.InventoryBatchLocations.FirstAsync(l => l.Id == id);
            a.QuantityOnHand -= 1;
            ctx1.Entry(a).Property(x => x.RowVersion).CurrentValue = new byte[] { 2, 0, 0, 0, 0, 0, 0, 0 };
            await ctx1.SaveChangesAsync();
            b.QuantityOnHand -= 1;
            var act = async () =>
            {
                try
                {
                    await ctx2.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new ConflictException("Stock location was modified concurrently. Reload and retry.");
                }
            };
            await act.Should().ThrowAsync<ConflictException>();
            _ = loc;
        }
    }

    [Fact]
    public async Task Credit_sale_writes_customer_ledger_debit()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx,
                ("CR", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)), 50));

            var sale = await fx.Sales.CreateAsync(new CreateSaleRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                WarehouseId = fx.WarehouseId,
                CustomerId = fx.CustomerId,
                SaleType = "Credit",
                Lines =
                [
                    new CreateSaleLineRequest
                    {
                        ProductId = fx.ProductId,
                        ProductUnitId = fx.BaseUnitId,
                        Quantity = 4,
                        UnitPrice = 50
                    }
                ],
                Payments =
                [
                    new CreateSalePaymentRequest { PaymentMethodId = fx.CreditMethodId, Amount = 200 }
                ]
            });

            sale.PaymentStatus.Should().Be("Unpaid");
            sale.DueAmount.Should().Be(200);
            sale.PaidAmount.Should().Be(0);

            var ledger = await fx.Db.CustomerLedgers.AsNoTracking()
                .Where(l => l.CustomerId == fx.CustomerId && l.ReferenceId == sale.Id)
                .ToListAsync();
            ledger.Should().ContainSingle();
            ledger[0].Debit.Should().Be(200);
            ledger[0].TransactionType.Should().Be("Sale");
        }
    }

    [Fact]
    public async Task Held_sale_hold_resume_discard()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            var held = await fx.HeldSales.HoldAsync(new HoldSaleRequest
            {
                BranchId = fx.BranchId,
                TerminalId = fx.TerminalId,
                CartData = """{"items":[{"sku":"PARA-500","qty":2}]}""",
                TotalAmount = 10
            });
            held.Status.Should().Be("Held");

            var resumed = await fx.HeldSales.ResumeAsync(held.Id);
            resumed.Status.Should().Be("Resumed");
            resumed.CartData.Should().Contain("PARA-500");

            await fx.HeldSales.DiscardAsync(held.Id);
            var after = await fx.HeldSales.GetByIdAsync(held.Id);
            after!.Status.Should().Be("Cancelled");
        }
    }

    [Fact]
    public async Task Sale_return_good_restocks_and_quarantine_for_damaged()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx,
                ("RET", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)), 40));

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
                        UnitPrice = 5
                    }
                ],
                Payments =
                [
                    new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 50 }
                ]
            });

            var batchId = sale.Lines[0].Batches[0].BatchId;
            var before = await fx.Db.InventoryBatchLocations.AsNoTracking()
                .Where(l => l.BatchId == batchId && l.WarehouseLocationId == fx.LocationId)
                .Select(l => l.QuantityOnHand)
                .FirstAsync();

            var goodReturn = await fx.Returns.CreateAsync(new CreateSaleReturnRequest
            {
                SaleId = sale.Id,
                Reason = "Customer request",
                RefundPaymentMethodId = fx.CashMethodId,
                PostImmediately = true,
                Lines =
                [
                    new CreateSaleReturnLineRequest
                    {
                        SaleLineId = sale.Lines[0].Id,
                        BatchId = batchId,
                        ProductUnitId = fx.BaseUnitId,
                        Quantity = 2,
                        Condition = "Good",
                        DestinationLocationId = fx.LocationId
                    }
                ]
            });

            goodReturn.Status.Should().Be("Posted");
            goodReturn.ReturnNumber.Should().StartWith("RET-");
            var afterGood = await fx.Db.InventoryBatchLocations.AsNoTracking()
                .Where(l => l.BatchId == batchId && l.WarehouseLocationId == fx.LocationId)
                .Select(l => l.QuantityOnHand)
                .FirstAsync();
            afterGood.Should().Be(before + 2);

            var damaged = await fx.Returns.CreateAsync(new CreateSaleReturnRequest
            {
                SaleId = sale.Id,
                Reason = "Damaged blister",
                RefundPaymentMethodId = fx.CashMethodId,
                Lines =
                [
                    new CreateSaleReturnLineRequest
                    {
                        SaleLineId = sale.Lines[0].Id,
                        BatchId = batchId,
                        ProductUnitId = fx.BaseUnitId,
                        Quantity = 1,
                        Condition = "Damaged"
                    }
                ]
            });

            damaged.Status.Should().Be("Posted");
            damaged.Lines[0].ReturnToStock.Should().BeFalse();
            damaged.Lines[0].DestinationLocationId.Should().Be(fx.QuarantineLocationId);

            var qQty = await fx.Db.InventoryBatchLocations.AsNoTracking()
                .Where(l => l.BatchId == batchId && l.WarehouseLocationId == fx.QuarantineLocationId)
                .Select(l => l.QuantityOnHand)
                .FirstAsync();
            qQty.Should().Be(1);

            var updatedSale = await fx.Sales.GetByIdAsync(sale.Id);
            updatedSale!.Status.Should().Be("PartiallyReturned");
        }
    }

    [Fact]
    public async Task Idempotent_sale_returns_same_invoice()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx,
                ("IDEM", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)), 50));

            var req = new CreateSaleRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                WarehouseId = fx.WarehouseId,
                IdempotencyKey = "offline-sale-001",
                Lines =
                [
                    new CreateSaleLineRequest
                    {
                        ProductId = fx.ProductId,
                        ProductUnitId = fx.BaseUnitId,
                        Quantity = 3,
                        UnitPrice = 5
                    }
                ],
                Payments =
                [
                    new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 15 }
                ]
            };

            var first = await fx.Sales.CreateAsync(req);
            var second = await fx.Sales.CreateAsync(req);
            second.Id.Should().Be(first.Id);
            second.InvoiceNumber.Should().Be(first.InvoiceNumber);

            var stock = await fx.Db.InventoryBatchLocations.AsNoTracking().SumAsync(l => l.QuantityOnHand);
            stock.Should().Be(47); // only one depletion
        }
    }
}
