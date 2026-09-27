using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PharmacyManagement.Application.DTOs.Products;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class ProductServiceTests
{
    private static async Task<(PharmacyManagementDbContext db, ProductService sut, long tenantId)> SeedAsync()
    {
        var options = new DbContextOptionsBuilder<PharmacyManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new PharmacyManagementDbContext(options);
        var tenant = new Tenant { Name = "Demo", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RowVersion = new byte[] { 1 } };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var manufacturer = new Manufacturer { Name = "Acme", IsActive = true };
        db.Manufacturers.Add(manufacturer);
        await db.SaveChangesAsync();
        var brand = new Brand { ManufacturerId = manufacturer.Id, Name = "Acme Brand", IsActive = true };
        var category = new ProductCategory { Name = "Analgesics", IsActive = true };
        var tclass = new TherapeuticClass { Name = "NSAID" };
        db.Brands.Add(brand);
        db.ProductCategories.Add(category);
        db.TherapeuticClasses.Add(tclass);
        await db.SaveChangesAsync();

        var current = new Mock<ICurrentUserService>();
        current.SetupGet(c => c.TenantId).Returns(tenant.Id);
        var sut = new ProductService(db, current.Object);
        return (db, sut, tenant.Id);
    }

    [Fact]
    public async Task Create_and_get_by_sku_works()
    {
        var (db, sut, _) = await SeedAsync();
        await using (db)
        {
            var mfr = await db.Manufacturers.FirstAsync();
            var brand = await db.Brands.FirstAsync();
            var cat = await db.ProductCategories.FirstAsync();
            var tc = await db.TherapeuticClasses.FirstAsync();

            var created = await sut.CreateAsync(new CreateProductRequest
            {
                CategoryId = cat.Id,
                ManufacturerId = mfr.Id,
                BrandId = brand.Id,
                TherapeuticClassId = tc.Id,
                Sku = "PARA-500",
                Name = "Paracetamol 500mg"
            });

            created.Id.Should().BeGreaterThan(0);
            created.Sku.Should().Be("PARA-500");

            var bySku = await sut.GetBySkuAsync("PARA-500");
            bySku.Should().NotBeNull();
            bySku!.Name.Should().Be("Paracetamol 500mg");
        }
    }

    [Fact]
    public async Task Deactivate_sets_IsActive_false()
    {
        var (db, sut, tenantId) = await SeedAsync();
        await using (db)
        {
            var mfr = await db.Manufacturers.FirstAsync();
            var brand = await db.Brands.FirstAsync();
            var cat = await db.ProductCategories.FirstAsync();
            var tc = await db.TherapeuticClasses.FirstAsync();
            var created = await sut.CreateAsync(new CreateProductRequest
            {
                CategoryId = cat.Id,
                ManufacturerId = mfr.Id,
                BrandId = brand.Id,
                TherapeuticClassId = tc.Id,
                Sku = "IBU-200",
                Name = "Ibuprofen 200mg"
            });

            await sut.DeactivateAsync(created.Id);
            var product = await db.Products.AsNoTracking().FirstAsync(p => p.Id == created.Id && p.TenantId == tenantId);
            product.IsActive.Should().BeFalse();
        }
    }
}
