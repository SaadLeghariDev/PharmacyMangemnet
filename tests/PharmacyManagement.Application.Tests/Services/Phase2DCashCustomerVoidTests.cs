using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PharmacyManagement.Application.DTOs.Cash;
using PharmacyManagement.Application.DTOs.Customers;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class Phase2DCashCustomerVoidTests
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
        public long CreditMethodId { get; init; }
        public long CustomerId { get; init; }
        public SaleService Sales { get; init; } = null!;
        public CashShiftService CashShifts { get; init; } = null!;
        public CustomerService Customers { get; init; } = null!;
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

        var loc = new WarehouseLocation
        {
            WarehouseId = wh.Id,
            Code = "A-01",
            Name = "Aisle",
            LocationType = "Selling",
            IsActive = true
        };
        db.WarehouseLocations.Add(loc);
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
            CreditLimit = 100,
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
            BranchId = null,
            TerminalId = null,
            DocumentType = DocumentTypes.Customer,
            Prefix = "C-",
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
            CreditMethodId = credit.Id,
            CustomerId = customer.Id,
            Sales = new SaleService(db, current.Object, sequences),
            CashShifts = new CashShiftService(db, current.Object),
            Customers = new CustomerService(db, current.Object, sequences),
            Grn = new GoodsReceiptService(db, current.Object, sequences)
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
    public async Task Cash_shift_open_payin_close_computes_variance()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            var opened = await fx.CashShifts.OpenAsync(new OpenCashShiftRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                OpeningAmount = 1000
            });
            opened.Status.Should().Be("Open");
            opened.OpeningAmount.Should().Be(1000);

            await fx.CashShifts.PayInAsync(opened.Id, new CashDrawerMovementRequest { Amount = 50, Remarks = "Float top-up" });

            // Simulate a cash sale txn on the open shift
            fx.Db.CashTransactions.Add(new CashTransaction
            {
                CashShiftId = opened.Id,
                TransactionType = "Sale",
                Amount = 250,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = fx.UserId
            });
            await fx.Db.SaveChangesAsync();

            var closed = await fx.CashShifts.CloseAsync(opened.Id, new CloseCashShiftRequest { ClosingAmount = 1290 });
            closed.Status.Should().Be("Closed");
            closed.ExpectedAmount.Should().Be(1300); // 1000 + 250 + 50
            closed.VarianceAmount.Should().Be(-10);
        }
    }

    [Fact]
    public async Task Cash_shift_rejects_second_open_on_same_terminal()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await fx.CashShifts.OpenAsync(new OpenCashShiftRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                OpeningAmount = 100
            });

            var act = async () => await fx.CashShifts.OpenAsync(new OpenCashShiftRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                OpeningAmount = 200
            });

            await act.Should().ThrowAsync<ConflictException>();
        }
    }

    [Fact]
    public async Task Cash_sale_posts_to_open_shift()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx, 50);
            var shift = await fx.CashShifts.OpenAsync(new OpenCashShiftRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                OpeningAmount = 500
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
                        Quantity = 2,
                        UnitPrice = 10
                    }
                ],
                Payments = [new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 20 }]
            });

            var txns = await fx.Db.CashTransactions.AsNoTracking()
                .Where(t => t.CashShiftId == shift.Id && t.TransactionType == "Sale")
                .ToListAsync();
            txns.Should().ContainSingle();
            txns[0].Amount.Should().Be(20);
            txns[0].ReferenceId.Should().Be(sale.Id);
        }
    }

    [Fact]
    public async Task Void_sale_restocks_and_voids_payments()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx, 40);
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
                Payments = [new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 50 }]
            });

            var locAfterSale = await fx.Db.InventoryBatchLocations.AsNoTracking().FirstAsync();
            locAfterSale.QuantityOnHand.Should().Be(30);

            var voided = await fx.Sales.VoidAsync(sale.Id, new VoidSaleRequest { Reason = "Cashier error" });
            voided.Status.Should().Be("Voided");
            voided.PaymentStatus.Should().Be("Refunded");
            voided.Payments.Should().OnlyContain(p => p.Status == "Voided");

            var locAfterVoid = await fx.Db.InventoryBatchLocations.AsNoTracking().FirstAsync();
            locAfterVoid.QuantityOnHand.Should().Be(40);

            var voidMovs = await fx.Db.InventoryMovements.AsNoTracking()
                .Where(m => m.ReferenceId == sale.Id && m.MovementType == MovementTypes.Void)
                .ToListAsync();
            voidMovs.Should().ContainSingle();
            voidMovs[0].Quantity.Should().Be(10);
        }
    }

    [Fact]
    public async Task Void_credit_sale_reverses_customer_ledger()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx, 40);
            // Raise credit limit for this test
            var cust = await fx.Db.Customers.FirstAsync(c => c.Id == fx.CustomerId);
            cust.CreditLimit = 5000;
            await fx.Db.SaveChangesAsync();

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
                        UnitPrice = 25
                    }
                ],
                Payments = [new CreateSalePaymentRequest { PaymentMethodId = fx.CreditMethodId, Amount = 100 }]
            });

            var debit = await fx.Db.CustomerLedgers.AsNoTracking()
                .Where(l => l.ReferenceId == sale.Id && l.TransactionType == "Sale")
                .SumAsync(l => l.Debit);
            debit.Should().Be(100);

            await fx.Sales.VoidAsync(sale.Id, new VoidSaleRequest());

            var net = await fx.Db.CustomerLedgers.AsNoTracking()
                .Where(l => l.ReferenceType == "Sale" && l.ReferenceId == sale.Id)
                .SumAsync(l => l.Debit - l.Credit);
            net.Should().Be(0);
            (await fx.Db.CustomerLedgers.AnyAsync(l =>
                l.ReferenceId == sale.Id && l.TransactionType == "Void" && l.Credit == 100)).Should().BeTrue();
        }
    }

    [Fact]
    public async Task Credit_sale_rejected_when_over_credit_limit()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx, 40);
            // Customer credit limit is 100
            var act = async () => await fx.Sales.CreateAsync(new CreateSaleRequest
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
                        Quantity = 5,
                        UnitPrice = 30
                    }
                ],
                Payments = [new CreateSalePaymentRequest { PaymentMethodId = fx.CreditMethodId, Amount = 150 }]
            });

            await act.Should().ThrowAsync<ValidationAppException>()
                .Where(e => e.Errors.Any(m => m.Contains("Credit limit", StringComparison.OrdinalIgnoreCase)));
        }
    }

    [Fact]
    public async Task Customer_create_allocates_code_via_number_sequence()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            var created = await fx.Customers.CreateAsync(new CreateCustomerRequest
            {
                Name = "New Patient",
                Phone = "0300-1111111",
                CreditLimit = 1000,
                IsPatient = true
            });

            created.CustomerCode.Should().Be("C-000001");
            created.Balance.Should().Be(0);
            created.AvailableCredit.Should().Be(1000);
        }
    }

    [Fact]
    public async Task Customer_ar_payment_writes_ledger_credit()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx, 40);
            var cust = await fx.Db.Customers.FirstAsync(c => c.Id == fx.CustomerId);
            cust.CreditLimit = 5000;
            await fx.Db.SaveChangesAsync();

            await fx.Sales.CreateAsync(new CreateSaleRequest
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
                        Quantity = 2,
                        UnitPrice = 50
                    }
                ],
                Payments = [new CreateSalePaymentRequest { PaymentMethodId = fx.CreditMethodId, Amount = 100 }]
            });

            var payment = await fx.Customers.RecordPaymentAsync(fx.CustomerId, new RecordCustomerPaymentRequest
            {
                BranchId = fx.BranchId,
                PaymentMethodId = fx.CashMethodId,
                Amount = 40,
                Remarks = "Partial settle"
            });

            payment.Amount.Should().Be(40);
            var dto = await fx.Customers.GetByIdAsync(fx.CustomerId);
            dto!.Balance.Should().Be(60);

            var ledgerCredit = await fx.Db.CustomerLedgers.AsNoTracking()
                .FirstAsync(l => l.ReferenceType == "CustomerPayment" && l.ReferenceId == payment.Id);
            ledgerCredit.Credit.Should().Be(40);
            ledgerCredit.TransactionType.Should().Be("Payment");
        }
    }
}
