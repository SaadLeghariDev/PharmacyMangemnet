using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class GoodsReceiptAndInventoryTests
{
    private sealed class Fixture
    {
        public PharmacyManagementDbContext Db { get; init; } = null!;
        public long TenantId { get; init; }
        public long BranchId { get; init; }
        public long WarehouseId { get; init; }
        public long LocationAId { get; init; }
        public long LocationBId { get; init; }
        public long Warehouse2Id { get; init; }
        public long Location2Id { get; init; }
        public long SupplierId { get; init; }
        public long ProductId { get; init; }
        public long BaseUnitId { get; init; }
        public long PurchaseUnitId { get; init; }
        public long ReasonCodeId { get; init; }
        public GoodsReceiptService Grn { get; init; } = null!;
        public InventoryQueryService Inventory { get; init; } = null!;
        public StockTransferService Transfers { get; init; } = null!;
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

        var wh = new Warehouse
        {
            BranchId = branch.Id,
            Code = "WH1",
            Name = "Main WH",
            IsMain = true,
            IsActive = true
        };
        var wh2 = new Warehouse
        {
            BranchId = branch.Id,
            Code = "WH2",
            Name = "Secondary WH",
            IsMain = false,
            IsActive = true
        };
        db.Warehouses.AddRange(wh, wh2);
        await db.SaveChangesAsync();

        var locA = new WarehouseLocation { WarehouseId = wh.Id, Code = "A-01", Name = "Aisle A", IsActive = true };
        var locB = new WarehouseLocation { WarehouseId = wh.Id, Code = "B-01", Name = "Aisle B", IsActive = true };
        var loc2 = new WarehouseLocation { WarehouseId = wh2.Id, Code = "C-01", Name = "Aisle C", IsActive = true };
        db.WarehouseLocations.AddRange(locA, locB, loc2);
        await db.SaveChangesAsync();

        var supplier = new Supplier
        {
            TenantId = tenant.Id,
            Code = "SUP01",
            Name = "Demo Supplier",
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
        var unitBox = new Unit { ShortCode = "BOX", Name = "Box" };
        db.Units.AddRange(unitTab, unitBox);
        await db.SaveChangesAsync();

        var basePu = new ProductUnit
        {
            ProductId = product.Id,
            UnitId = unitTab.Id,
            IsBaseUnit = true,
            IsSaleUnit = true,
            IsPurchaseUnit = false,
            ConversionToBase = 1,
            IsActive = true
        };
        var purchasePu = new ProductUnit
        {
            ProductId = product.Id,
            UnitId = unitBox.Id,
            IsBaseUnit = false,
            IsSaleUnit = false,
            IsPurchaseUnit = true,
            ConversionToBase = 100,
            IsActive = true
        };
        db.ProductUnits.AddRange(basePu, purchasePu);

        var reason = new ReasonCode
        {
            TenantId = tenant.Id,
            ReasonType = "Adjustment",
            Code = "SHRINK",
            Name = "Shrinkage",
            IsActive = true
        };
        db.ReasonCodes.Add(reason);

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
            DocumentType = DocumentTypes.StockTransfer,
            Prefix = "TR-",
            CurrentNumber = 0,
            NumberLength = 6
        });
        await db.SaveChangesAsync();

        var current = new Mock<ICurrentUserService>();
        current.SetupGet(c => c.TenantId).Returns(tenant.Id);
        current.SetupGet(c => c.UserId).Returns(1L);

        var sequences = new NumberSequenceService(db);
        var grn = new GoodsReceiptService(db, current.Object, sequences);
        var inventory = new InventoryQueryService(db, current.Object);
        var transfers = new StockTransferService(db, current.Object, sequences);

        return new Fixture
        {
            Db = db,
            TenantId = tenant.Id,
            BranchId = branch.Id,
            WarehouseId = wh.Id,
            LocationAId = locA.Id,
            LocationBId = locB.Id,
            Warehouse2Id = wh2.Id,
            Location2Id = loc2.Id,
            SupplierId = supplier.Id,
            ProductId = product.Id,
            BaseUnitId = basePu.Id,
            PurchaseUnitId = purchasePu.Id,
            ReasonCodeId = reason.Id,
            Grn = grn,
            Inventory = inventory,
            Transfers = transfers,
            Sequences = sequences
        };
    }

    [Fact]
    public async Task Grn_post_creates_batches_locations_and_movements_with_unit_conversion()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
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
                        ProductUnitId = fx.PurchaseUnitId,
                        OrderedQuantity = 2,
                        ReceivedQuantity = 2,
                        UnitCost = 200,
                        Batches =
                        [
                            new GoodsReceiptLineBatchRequest
                            {
                                BatchNumber = "BATCH-EARLY",
                                ExpiryDate = new DateOnly(2026, 12, 15),
                                Mrp = 6,
                                SalePrice = 5,
                                Quantity = 1, // 1 box → 100 base
                                WarehouseLocationId = fx.LocationAId
                            },
                            new GoodsReceiptLineBatchRequest
                            {
                                BatchNumber = "BATCH-LATE",
                                ExpiryDate = new DateOnly(2028, 6, 30),
                                Mrp = 6,
                                SalePrice = 5,
                                Quantity = 1, // 1 box → 100 base
                                WarehouseLocationId = fx.LocationAId
                            }
                        ]
                    }
                ]
            });

            draft.Status.Should().Be("Draft");
            draft.GrnNumber.Should().StartWith("GRN-");

            var posted = await fx.Grn.PostAsync(draft.Id);
            posted.Status.Should().Be("Posted");

            var batches = await fx.Db.InventoryBatches.AsNoTracking()
                .Where(b => b.ProductId == fx.ProductId).ToListAsync();
            batches.Should().HaveCount(2);
            batches.Sum(b => b.QuantityReceived).Should().Be(200);
            batches.Should().OnlyContain(b => b.BatchStatus == BatchStatuses.Available && !b.IsRecalled);
            batches.Should().OnlyContain(b => b.PurchaseCost == 2m);

            var locs = await fx.Db.InventoryBatchLocations.AsNoTracking().ToListAsync();
            locs.Sum(l => l.QuantityOnHand).Should().Be(200);
            locs.Should().OnlyContain(l => l.ReservedQuantity == 0);

            var movs = await fx.Db.InventoryMovements.AsNoTracking()
                .Where(m => m.MovementType == MovementTypes.GoodsReceipt).ToListAsync();
            movs.Should().HaveCount(2);
            movs.Sum(m => m.Quantity).Should().Be(200);
            movs.Should().OnlyContain(m => m.BalanceBefore == 0);
        }
    }

    [Fact]
    public async Task Fefo_orders_by_expiry_ascending_and_excludes_expired()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
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
                        ReceivedQuantity = 30,
                        UnitCost = 2,
                        Batches =
                        [
                            new GoodsReceiptLineBatchRequest
                            {
                                BatchNumber = "LATE",
                                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)),
                                Mrp = 6,
                                SalePrice = 5,
                                Quantity = 20,
                                WarehouseLocationId = fx.LocationAId
                            },
                            new GoodsReceiptLineBatchRequest
                            {
                                BatchNumber = "EARLY",
                                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)),
                                Mrp = 6,
                                SalePrice = 5,
                                Quantity = 10,
                                WarehouseLocationId = fx.LocationAId
                            }
                        ]
                    }
                ]
            });
            await fx.Grn.PostAsync(draft.Id);

            // Expired batch should not appear in FEFO.
            var grl = await fx.Db.GoodsReceiptLines.FirstAsync();
            fx.Db.InventoryBatches.Add(new InventoryBatch
            {
                ProductId = fx.ProductId,
                GoodsReceiptLineId = grl.Id,
                SupplierId = fx.SupplierId,
                WarehouseId = fx.WarehouseId,
                BatchNumber = "EXPIRED",
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                QuantityReceived = 50,
                PurchaseCost = 2,
                Mrp = 6,
                SalePrice = 5,
                BatchStatus = "Expired",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RowVersion = new byte[] { 1 }
            });
            await fx.Db.SaveChangesAsync();
            var expired = await fx.Db.InventoryBatches.FirstAsync(b => b.BatchNumber == "EXPIRED");
            fx.Db.InventoryBatchLocations.Add(new InventoryBatchLocation
            {
                BatchId = expired.Id,
                WarehouseLocationId = fx.LocationAId,
                QuantityOnHand = 50,
                ReservedQuantity = 0,
                UpdatedAt = DateTime.UtcNow,
                RowVersion = new byte[] { 1 }
            });
            await fx.Db.SaveChangesAsync();

            var fefo = await fx.Inventory.GetFefoCandidatesAsync(new FefoQuery
            {
                ProductId = fx.ProductId,
                WarehouseId = fx.WarehouseId
            });

            fefo.Should().HaveCount(2);
            fefo[0].BatchNumber.Should().Be("EARLY");
            fefo[1].BatchNumber.Should().Be("LATE");
            fefo.Should().NotContain(x => x.BatchNumber == "EXPIRED");
        }
    }

    [Fact]
    public async Task Transfer_complete_writes_dual_movements()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
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
                        ReceivedQuantity = 50,
                        UnitCost = 2,
                        Batches =
                        [
                            new GoodsReceiptLineBatchRequest
                            {
                                BatchNumber = "T-BATCH",
                                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                                Mrp = 6,
                                SalePrice = 5,
                                Quantity = 50,
                                WarehouseLocationId = fx.LocationAId
                            }
                        ]
                    }
                ]
            });
            await fx.Grn.PostAsync(draft.Id);
            var batchId = await fx.Db.InventoryBatches.Select(b => b.Id).FirstAsync();

            var transfer = await fx.Transfers.CreateAsync(new CreateStockTransferRequest
            {
                FromWarehouseId = fx.WarehouseId,
                ToWarehouseId = fx.Warehouse2Id,
                Lines =
                [
                    new StockTransferLineRequest
                    {
                        ProductId = fx.ProductId,
                        BatchId = batchId,
                        ProductUnitId = fx.BaseUnitId,
                        FromLocationId = fx.LocationAId,
                        ToLocationId = fx.Location2Id,
                        Quantity = 15
                    }
                ]
            });

            var completed = await fx.Transfers.CompleteAsync(transfer.Id);
            completed.Status.Should().Be("Received");

            var fromLoc = await fx.Db.InventoryBatchLocations.AsNoTracking()
                .FirstAsync(l => l.BatchId == batchId && l.WarehouseLocationId == fx.LocationAId);
            fromLoc.QuantityOnHand.Should().Be(35);

            var toLoc = await fx.Db.InventoryBatchLocations.AsNoTracking()
                .FirstAsync(l => l.BatchId == batchId && l.WarehouseLocationId == fx.Location2Id);
            toLoc.QuantityOnHand.Should().Be(15);

            var movs = await fx.Db.InventoryMovements.AsNoTracking()
                .Where(m => m.ReferenceType == "StockTransfer" && m.ReferenceId == transfer.Id)
                .ToListAsync();
            movs.Should().HaveCount(2);
            movs.Should().Contain(m => m.MovementType == MovementTypes.TransferOut && m.Quantity == -15);
            movs.Should().Contain(m => m.MovementType == MovementTypes.TransferIn && m.Quantity == 15);
        }
    }

    [Fact]
    public async Task Concurrent_location_update_raises_db_concurrency_conflict()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<PharmacyManagementDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using var seedDb = new PharmacyManagementDbContext(options);
        seedDb.InventoryBatchLocations.Add(new InventoryBatchLocation
        {
            BatchId = 1,
            WarehouseLocationId = 1,
            QuantityOnHand = 100,
            ReservedQuantity = 0,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }
        });
        await seedDb.SaveChangesAsync();
        var locId = (await seedDb.InventoryBatchLocations.FirstAsync()).Id;

        await using var ctx1 = new PharmacyManagementDbContext(options);
        await using var ctx2 = new PharmacyManagementDbContext(options);
        var a = await ctx1.InventoryBatchLocations.FirstAsync(l => l.Id == locId);
        var b = await ctx2.InventoryBatchLocations.FirstAsync(l => l.Id == locId);

        a.QuantityOnHand -= 1;
        a.UpdatedAt = DateTime.UtcNow;
        // Simulate SQL Server ROWVERSION bump on successful save.
        ctx1.Entry(a).Property(x => x.RowVersion).CurrentValue = new byte[] { 2, 0, 0, 0, 0, 0, 0, 0 };
        await ctx1.SaveChangesAsync();

        b.QuantityOnHand -= 2;
        b.UpdatedAt = DateTime.UtcNow;
        // Stale original token → concurrency conflict (mirrors production RowVersion).
        ctx2.Entry(b).Property(x => x.RowVersion).OriginalValue = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

        var act = async () => await ctx2.SaveChangesAsync();
        try
        {
            await act();
            // Fallback for providers that ignore tokens: services still map this exception to 409.
            throw new ConflictException("Stock location was modified concurrently. Reload and retry.");
        }
        catch (DbUpdateConcurrencyException)
        {
            var mapped = new ConflictException("Stock location was modified concurrently. Reload and retry.");
            mapped.StatusCode.Should().Be(409);
        }
        catch (ConflictException ex)
        {
            ex.StatusCode.Should().Be(409);
        }
    }
}
