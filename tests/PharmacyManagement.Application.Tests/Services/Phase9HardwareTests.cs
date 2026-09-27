using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PharmacyManagement.Application.DTOs.Hardware;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class Phase9HardwareTests
{
    private sealed class Fixture
    {
        public PharmacyManagementDbContext Db { get; init; } = null!;
        public long TenantId { get; init; }
        public long BranchId { get; init; }
        public long TerminalId { get; init; }
        public long DeviceTypeId { get; init; }
        public long ProductId { get; init; }
        public DeviceService Devices { get; init; } = null!;
        public PrintService Print { get; init; } = null!;
        public AttachmentService Attachments { get; init; } = null!;
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

        var counter = new Counter
        {
            BranchId = branch.Id,
            Code = "C1",
            Name = "Counter 1",
            IsActive = true
        };
        db.Counters.Add(counter);
        await db.SaveChangesAsync();

        var terminal = new Posterminal
        {
            BranchId = branch.Id,
            CounterId = counter.Id,
            TerminalCode = "T1",
            IsActive = true
        };
        db.Posterminals.Add(terminal);

        var deviceType = new DeviceType { Code = "PRINTER", Name = "Receipt Printer" };
        db.DeviceTypes.Add(deviceType);

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
            Sku = "SKU-PARA-500",
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

        var current = new Mock<ICurrentUserService>();
        current.SetupGet(c => c.TenantId).Returns(tenant.Id);
        current.SetupGet(c => c.UserId).Returns(1L);

        return new Fixture
        {
            Db = db,
            TenantId = tenant.Id,
            BranchId = branch.Id,
            TerminalId = terminal.Id,
            DeviceTypeId = deviceType.Id,
            ProductId = product.Id,
            Devices = new DeviceService(db, current.Object),
            Print = new PrintService(db, current.Object),
            Attachments = new AttachmentService(db, current.Object)
        };
    }

    [Fact]
    public async Task Device_crud_assignment_settings_and_events_work()
    {
        var fx = await SeedAsync();

        var types = await fx.Devices.SearchDeviceTypesAsync(new DeviceTypeQuery());
        types.TotalCount.Should().Be(1);
        types.Items[0].Code.Should().Be("PRINTER");

        var created = await fx.Devices.CreateDeviceAsync(new CreateDeviceRequest
        {
            BranchId = fx.BranchId,
            DeviceTypeId = fx.DeviceTypeId,
            Name = "Zebra GC420t",
            Manufacturer = "Zebra",
            Model = "GC420t",
            ConnectionType = "USB",
            IsDefault = true,
            IsActive = true
        });
        created.Id.Should().BeGreaterThan(0);
        created.DeviceTypeCode.Should().Be("PRINTER");

        var updated = await fx.Devices.UpdateDeviceAsync(created.Id, new UpdateDeviceRequest
        {
            DeviceTypeId = fx.DeviceTypeId,
            Name = "Zebra Label",
            ConnectionType = "USB",
            IsDefault = true,
            IsActive = true,
            LastSeenAt = DateTime.UtcNow
        });
        updated.Name.Should().Be("Zebra Label");
        updated.LastSeenAt.Should().NotBeNull();

        var assignment = await fx.Devices.CreateAssignmentAsync(new CreateDeviceAssignmentRequest
        {
            DeviceId = created.Id,
            TerminalId = fx.TerminalId
        });
        assignment.IsActive.Should().BeTrue();

        var ended = await fx.Devices.EndAssignmentAsync(assignment.Id);
        ended.IsActive.Should().BeFalse();
        ended.AssignedTo.Should().NotBeNull();

        var setting = await fx.Devices.UpsertSettingAsync(created.Id, new UpsertDeviceSettingRequest
        {
            SettingKey = "dpi",
            SettingValue = "203",
            IsEncrypted = false
        });
        setting.SettingValue.Should().Be("203");
        var again = await fx.Devices.UpsertSettingAsync(created.Id, new UpsertDeviceSettingRequest
        {
            SettingKey = "dpi",
            SettingValue = "300",
            IsEncrypted = false
        });
        again.Id.Should().Be(setting.Id);
        again.SettingValue.Should().Be("300");

        var evt = await fx.Devices.AppendEventAsync(created.Id, new CreateDeviceEventRequest
        {
            EventType = "Heartbeat",
            Status = DeviceEventStatuses.Ok,
            Payload = "{\"online\":true}"
        });
        evt.Status.Should().Be(DeviceEventStatuses.Ok);

        var events = await fx.Devices.SearchEventsAsync(created.Id, new DeviceEventQuery());
        events.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Print_template_and_job_status_transitions_work()
    {
        var fx = await SeedAsync();

        var device = await fx.Devices.CreateDeviceAsync(new CreateDeviceRequest
        {
            BranchId = fx.BranchId,
            DeviceTypeId = fx.DeviceTypeId,
            Name = "Printer",
            IsActive = true
        });

        var template = await fx.Print.CreateTemplateAsync(new CreatePrintTemplateRequest
        {
            TemplateType = PrintTemplateTypes.Barcode,
            Name = "Standard Label",
            TemplateContent = "^XA^FD{{BARCODE}}^XZ",
            PaperWidth = 50,
            IsDefault = true,
            IsActive = true
        });

        var job = await fx.Print.CreateJobAsync(new CreateBarcodePrintJobRequest
        {
            BranchId = fx.BranchId,
            PrinterDeviceId = device.Id,
            ProductId = fx.ProductId,
            Quantity = 10,
            TemplateId = template.Id
        });
        job.Status.Should().Be(BarcodePrintJobStatuses.Queued);

        var printing = await fx.Print.UpdateJobStatusAsync(job.Id, new UpdateBarcodePrintJobStatusRequest
        {
            Status = BarcodePrintJobStatuses.Printing
        });
        printing.Status.Should().Be(BarcodePrintJobStatuses.Printing);

        var printed = await fx.Print.SimulateCompleteAsync(job.Id, new SimulateBarcodePrintJobRequest());
        printed.Status.Should().Be(BarcodePrintJobStatuses.Printed);
        printed.PrintedAt.Should().NotBeNull();

        var job2 = await fx.Print.CreateJobAsync(new CreateBarcodePrintJobRequest
        {
            BranchId = fx.BranchId,
            PrinterDeviceId = device.Id,
            ProductId = fx.ProductId,
            Quantity = 2,
            TemplateId = template.Id
        });
        var failed = await fx.Print.SimulateCompleteAsync(job2.Id, new SimulateBarcodePrintJobRequest { Fail = true });
        failed.Status.Should().Be(BarcodePrintJobStatuses.Failed);
    }

    [Fact]
    public async Task Attachment_metadata_and_entity_link_work()
    {
        var fx = await SeedAsync();

        var attachment = await fx.Attachments.CreateAsync(new CreateAttachmentRequest
        {
            FileName = "rx-scan.pdf",
            StoragePath = "stub://base64:JVBERi0x",
            ContentType = "application/pdf",
            FileSize = 12,
            Hash = "abc123"
        });
        attachment.StoragePath.Should().StartWith("stub://");

        var link = await fx.Attachments.LinkAsync(new CreateEntityAttachmentRequest
        {
            AttachmentId = attachment.Id,
            EntityName = "Product",
            EntityId = fx.ProductId
        });
        link.EntityName.Should().Be("Product");
        link.FileName.Should().Be("rx-scan.pdf");

        var links = await fx.Attachments.SearchLinksAsync(new EntityAttachmentQuery
        {
            EntityName = "Product",
            EntityId = fx.ProductId
        });
        links.TotalCount.Should().Be(1);
    }
}
