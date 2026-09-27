using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PharmacyManagement.Application.DTOs.Controlled;
using PharmacyManagement.Application.DTOs.Fiscal;
using PharmacyManagement.Application.DTOs.Prescriptions;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class Phase2EPrescriptionFiscalTests
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
        public long ControlledProductId { get; init; }
        public long BaseUnitId { get; init; }
        public long CtrlBaseUnitId { get; init; }
        public long CashMethodId { get; init; }
        public long CustomerId { get; init; }
        public SaleService Sales { get; init; } = null!;
        public GoodsReceiptService Grn { get; init; } = null!;
        public DoctorService Doctors { get; init; } = null!;
        public PrescriptionService Prescriptions { get; init; } = null!;
        public ControlledDrugService Controlled { get; init; } = null!;
        public FiscalService Fiscal { get; init; } = null!;
        public NumberSequenceService Sequences { get; init; } = null!;
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
            Username = "pharmacist",
            Email = "p@test.local",
            PasswordHash = "x",
            FullName = "Pharmacist",
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

        var customer = new Customer
        {
            TenantId = tenant.Id,
            CustomerCode = "C-001",
            Name = "Patient Ali",
            CreditLimit = 1000,
            IsPatient = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Customers.Add(customer);

        var mfr = new Manufacturer { Name = "Acme", IsActive = true };
        db.Manufacturers.Add(mfr);
        await db.SaveChangesAsync();
        var brand = new Brand { ManufacturerId = mfr.Id, Name = "Brand", IsActive = true };
        var cat = new ProductCategory { Name = "Rx", IsActive = true };
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
            PrescriptionRequired = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }
        };
        var ctrl = new Product
        {
            TenantId = tenant.Id,
            CategoryId = cat.Id,
            ManufacturerId = mfr.Id,
            BrandId = brand.Id,
            TherapeuticClassId = tc.Id,
            Sku = "MORPH-10",
            Name = "Morphine 10mg",
            IsActive = true,
            IsSaleable = true,
            IsReturnable = true,
            IsControlled = true,
            PrescriptionRequired = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }
        };
        db.Products.AddRange(product, ctrl);
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
        var ctrlPu = new ProductUnit
        {
            ProductId = ctrl.Id,
            UnitId = unitTab.Id,
            IsBaseUnit = true,
            IsSaleUnit = true,
            IsPurchaseUnit = true,
            ConversionToBase = 1,
            IsActive = true
        };
        db.ProductUnits.AddRange(basePu, ctrlPu);

        db.NumberSequences.AddRange(
            new NumberSequence
            {
                TenantId = tenant.Id,
                BranchId = branch.Id,
                DocumentType = DocumentTypes.GoodsReceipt,
                Prefix = "GRN-",
                CurrentNumber = 0,
                NumberLength = 6
            },
            new NumberSequence
            {
                TenantId = tenant.Id,
                BranchId = branch.Id,
                TerminalId = terminal.Id,
                DocumentType = DocumentTypes.Sale,
                Prefix = "INV-",
                CurrentNumber = 0,
                NumberLength = 6
            },
            new NumberSequence
            {
                TenantId = tenant.Id,
                BranchId = null,
                TerminalId = null,
                DocumentType = DocumentTypes.Prescription,
                Prefix = "RX-",
                CurrentNumber = 0,
                NumberLength = 6
            },
            new NumberSequence
            {
                TenantId = tenant.Id,
                BranchId = branch.Id,
                TerminalId = null,
                DocumentType = DocumentTypes.ControlledRegister,
                Prefix = "CDR-",
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
            ControlledProductId = ctrl.Id,
            BaseUnitId = basePu.Id,
            CtrlBaseUnitId = ctrlPu.Id,
            CashMethodId = cash.Id,
            CustomerId = customer.Id,
            Sequences = sequences,
            Sales = new SaleService(db, current.Object, sequences),
            Grn = new GoodsReceiptService(db, current.Object, sequences),
            Doctors = new DoctorService(db),
            Prescriptions = new PrescriptionService(db, current.Object, sequences),
            Controlled = new ControlledDrugService(db, current.Object, sequences),
            Fiscal = new FiscalService(db, current.Object, new MockFiscalGateway())
        };
    }

    private static async Task SeedStockAsync(Fixture fx, long productId, long unitId, decimal qty, string batch)
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
                    ProductId = productId,
                    ProductUnitId = unitId,
                    ReceivedQuantity = qty,
                    UnitCost = 2,
                    Batches =
                    [
                        new Application.DTOs.Purchasing.GoodsReceiptLineBatchRequest
                        {
                            BatchNumber = batch,
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
    public async Task Doctor_and_prescription_allocate_rx_number()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            var doctor = await fx.Doctors.CreateAsync(new CreateDoctorRequest
            {
                Name = "Dr. Sara Khan",
                PmdcNumber = "PMDC-123",
                Specialization = "General"
            });
            doctor.Id.Should().BeGreaterThan(0);

            var rx = await fx.Prescriptions.CreateAsync(new CreatePrescriptionRequest
            {
                CustomerId = fx.CustomerId,
                DoctorId = doctor.Id,
                Items =
                [
                    new CreatePrescriptionItemRequest
                    {
                        ProductId = fx.ProductId,
                        Quantity = 20,
                        DosageAmount = 1,
                        DosageUnit = "tab",
                        FrequencyCode = "BID"
                    }
                ]
            });

            rx.PrescriptionNumber.Should().Be("RX-000001");
            rx.Status.Should().Be("Active");
            rx.Items.Should().HaveCount(1);
            rx.Items[0].RemainingQuantity.Should().Be(20);
        }
    }

    [Fact]
    public async Task Dispense_links_sale_and_marks_dispensed()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx, fx.ProductId, fx.BaseUnitId, 100, "B-RX");
            var doctor = await fx.Doctors.CreateAsync(new CreateDoctorRequest { Name = "Dr. A" });
            var rx = await fx.Prescriptions.CreateAsync(new CreatePrescriptionRequest
            {
                CustomerId = fx.CustomerId,
                DoctorId = doctor.Id,
                Items = [new CreatePrescriptionItemRequest { ProductId = fx.ProductId, Quantity = 10 }]
            });
            var itemId = rx.Items[0].Id;

            var sale = await fx.Sales.CreateAsync(new CreateSaleRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                WarehouseId = fx.WarehouseId,
                CustomerId = fx.CustomerId,
                Lines =
                [
                    new CreateSaleLineRequest
                    {
                        ProductId = fx.ProductId,
                        ProductUnitId = fx.BaseUnitId,
                        Quantity = 10,
                        UnitPrice = 5,
                        PrescriptionItemId = itemId
                    }
                ],
                Payments = [new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 50 }]
            });

            var dispensed = await fx.Prescriptions.DispenseAsync(rx.Id, new DispensePrescriptionRequest
            {
                SaleId = sale.Id
            });

            dispensed.Status.Should().Be("Dispensed");
            dispensed.DispensingRecords.Should().HaveCount(1);
            dispensed.DispensingRecords[0].SaleId.Should().Be(sale.Id);
            dispensed.Items[0].DispensedQuantity.Should().Be(10);
            dispensed.Items[0].RemainingQuantity.Should().Be(0);
        }
    }

    [Fact]
    public async Task Dispense_partial_then_reject_over_qty()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx, fx.ProductId, fx.BaseUnitId, 100, "B-RX2");
            var doctor = await fx.Doctors.CreateAsync(new CreateDoctorRequest { Name = "Dr. B" });
            var rx = await fx.Prescriptions.CreateAsync(new CreatePrescriptionRequest
            {
                CustomerId = fx.CustomerId,
                DoctorId = doctor.Id,
                Items = [new CreatePrescriptionItemRequest { ProductId = fx.ProductId, Quantity = 20 }]
            });
            var itemId = rx.Items[0].Id;

            var sale1 = await fx.Sales.CreateAsync(new CreateSaleRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                WarehouseId = fx.WarehouseId,
                CustomerId = fx.CustomerId,
                Lines =
                [
                    new CreateSaleLineRequest
                    {
                        ProductId = fx.ProductId,
                        ProductUnitId = fx.BaseUnitId,
                        Quantity = 8,
                        UnitPrice = 5,
                        PrescriptionItemId = itemId
                    }
                ],
                Payments = [new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 40 }]
            });

            var partial = await fx.Prescriptions.DispenseAsync(rx.Id, new DispensePrescriptionRequest { SaleId = sale1.Id });
            partial.Status.Should().Be("PartiallyDispensed");

            var sale2 = await fx.Sales.CreateAsync(new CreateSaleRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                WarehouseId = fx.WarehouseId,
                CustomerId = fx.CustomerId,
                Lines =
                [
                    new CreateSaleLineRequest
                    {
                        ProductId = fx.ProductId,
                        ProductUnitId = fx.BaseUnitId,
                        Quantity = 15,
                        UnitPrice = 5,
                        PrescriptionItemId = itemId
                    }
                ],
                Payments = [new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 75 }]
            });

            var act = async () => await fx.Prescriptions.DispenseAsync(rx.Id, new DispensePrescriptionRequest
            {
                SaleId = sale2.Id
            });
            await act.Should().ThrowAsync<ValidationAppException>();
        }
    }

    [Fact]
    public async Task Controlled_dispense_decrements_and_void_restores()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx, fx.ControlledProductId, fx.CtrlBaseUnitId, 50, "CTRL-B1");
            var register = await fx.Controlled.OpenAsync(new OpenControlledRegisterRequest
            {
                BranchId = fx.BranchId,
                ProductId = fx.ControlledProductId,
                OpeningBalance = 40
            });
            register.RegisterNumber.Should().Be("CDR-000001");
            register.CurrentBalance.Should().Be(40);

            var doctor = await fx.Doctors.CreateAsync(new CreateDoctorRequest { Name = "Dr. Ctrl" });
            var rx = await fx.Prescriptions.CreateAsync(new CreatePrescriptionRequest
            {
                CustomerId = fx.CustomerId,
                DoctorId = doctor.Id,
                Items = [new CreatePrescriptionItemRequest { ProductId = fx.ControlledProductId, Quantity = 5 }]
            });

            var sale = await fx.Sales.CreateAsync(new CreateSaleRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                WarehouseId = fx.WarehouseId,
                CustomerId = fx.CustomerId,
                Lines =
                [
                    new CreateSaleLineRequest
                    {
                        ProductId = fx.ControlledProductId,
                        ProductUnitId = fx.CtrlBaseUnitId,
                        Quantity = 5,
                        UnitPrice = 10,
                        PrescriptionItemId = rx.Items[0].Id
                    }
                ],
                Payments = [new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 50 }]
            });

            await fx.Prescriptions.DispenseAsync(rx.Id, new DispensePrescriptionRequest
            {
                SaleId = sale.Id,
                WitnessedBy = fx.UserId
            });

            var afterDisp = await fx.Controlled.GetByIdAsync(register.Id);
            afterDisp!.CurrentBalance.Should().Be(35);
            afterDisp.RecentTransactions.Should().Contain(t => t.TransactionType == "Dispense" && t.Quantity == 5);

            await fx.Sales.VoidAsync(sale.Id, new VoidSaleRequest { Reason = "Wrong qty" });

            var afterVoid = await fx.Controlled.GetByIdAsync(register.Id);
            afterVoid!.CurrentBalance.Should().Be(40);
            afterVoid.RecentTransactions.Should().Contain(t => t.TransactionType == "Return");

            var rxAfter = await fx.Prescriptions.GetByIdAsync(rx.Id);
            rxAfter!.Status.Should().Be("Active");
        }
    }

    [Fact]
    public async Task Sale_rejects_prescription_customer_mismatch()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx, fx.ProductId, fx.BaseUnitId, 50, "B4");
            var other = new Customer
            {
                TenantId = fx.TenantId,
                CustomerCode = "C-002",
                Name = "Other",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            fx.Db.Customers.Add(other);
            await fx.Db.SaveChangesAsync();

            var doctor = await fx.Doctors.CreateAsync(new CreateDoctorRequest { Name = "Dr. D" });
            var rx = await fx.Prescriptions.CreateAsync(new CreatePrescriptionRequest
            {
                CustomerId = fx.CustomerId,
                DoctorId = doctor.Id,
                Items = [new CreatePrescriptionItemRequest { ProductId = fx.ProductId, Quantity = 5 }]
            });

            var act = async () => await fx.Sales.CreateAsync(new CreateSaleRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                WarehouseId = fx.WarehouseId,
                CustomerId = other.Id,
                Lines =
                [
                    new CreateSaleLineRequest
                    {
                        ProductId = fx.ProductId,
                        ProductUnitId = fx.BaseUnitId,
                        Quantity = 5,
                        UnitPrice = 5,
                        PrescriptionItemId = rx.Items[0].Id
                    }
                ],
                Payments = [new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 25 }]
            });

            await act.Should().ThrowAsync<ValidationAppException>();
        }
    }

    [Fact]
    public async Task Fiscal_create_submit_accept_and_retry_after_failure()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await SeedStockAsync(fx, fx.ProductId, fx.BaseUnitId, 20, "B-FIS");
            var sale = await fx.Sales.CreateAsync(new CreateSaleRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                WarehouseId = fx.WarehouseId,
                CustomerId = fx.CustomerId,
                Lines =
                [
                    new CreateSaleLineRequest
                    {
                        ProductId = fx.ProductId,
                        ProductUnitId = fx.BaseUnitId,
                        Quantity = 2,
                        UnitPrice = 10,
                        TaxAmount = 2
                    }
                ],
                Payments = [new CreateSalePaymentRequest { PaymentMethodId = fx.CashMethodId, Amount = 22 }]
            });

            var doc = await fx.Fiscal.CreateAsync(new CreateFiscalDocumentRequest { SaleId = sale.Id });
            doc.SubmissionStatus.Should().Be("Pending");
            doc.Lines.Should().HaveCount(1);

            var failed = await fx.Fiscal.SubmitAsync(doc.Id, new FiscalSubmitRequest { ForceFailure = true });
            failed.SubmissionStatus.Should().Be("Failed");
            failed.SaleFbrStatus.Should().Be("Failed");
            failed.Submissions.Should().HaveCount(1);

            var saleAfterFail = await fx.Sales.GetByIdAsync(sale.Id);
            saleAfterFail!.Status.Should().Be("Completed");
            saleAfterFail.FbrStatus.Should().Be("Failed");

            var accepted = await fx.Fiscal.RetryAsync(doc.Id, new FiscalSubmitRequest());
            accepted.SubmissionStatus.Should().Be("Accepted");
            accepted.FbrInvoiceNumber.Should().NotBeNullOrWhiteSpace();
            accepted.Submissions.Should().HaveCount(2);
            accepted.SaleFbrStatus.Should().Be("Accepted");
        }
    }

    [Fact]
    public async Task Controlled_manual_receipt_increases_balance()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            var register = await fx.Controlled.OpenAsync(new OpenControlledRegisterRequest
            {
                BranchId = fx.BranchId,
                ProductId = fx.ControlledProductId,
                OpeningBalance = 10
            });

            var txn = await fx.Controlled.PostTransactionAsync(register.Id, new PostControlledTransactionRequest
            {
                TransactionType = "Receipt",
                Quantity = 5,
                Remarks = "Delivery"
            });

            txn.BalanceAfter.Should().Be(15);
            var updated = await fx.Controlled.GetByIdAsync(register.Id);
            updated!.CurrentBalance.Should().Be(15);
        }
    }
}
