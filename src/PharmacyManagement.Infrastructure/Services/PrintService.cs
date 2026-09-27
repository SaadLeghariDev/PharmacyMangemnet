using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Hardware;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class PrintService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser) : IPrintService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<PrintTemplateDto>> SearchTemplatesAsync(
        PrintTemplateQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.PrintTemplates.AsNoTracking().Where(t => t.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(query.TemplateType))
        {
            var type = query.TemplateType.Trim();
            q = q.Where(t => t.TemplateType == type);
        }

        if (query.IsActive is bool active) q = q.Where(t => t.IsActive == active);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(t => t.Name.Contains(s) || t.TemplateType.Contains(s));
        }

        q = q.OrderBy(t => t.TemplateType).ThenBy(t => t.Name);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<PrintTemplateDto>
        {
            Items = rows.Select(MapTemplate).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<PrintTemplateDto?> GetTemplateByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.PrintTemplates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId, ct);
        return entity is null ? null : MapTemplate(entity);
    }

    public async Task<PrintTemplateDto> CreateTemplateAsync(
        CreatePrintTemplateRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = new PrintTemplate
        {
            TenantId = tenantId,
            TemplateType = request.TemplateType.Trim(),
            Name = request.Name.Trim(),
            TemplateContent = request.TemplateContent,
            PaperWidth = request.PaperWidth,
            IsDefault = request.IsDefault,
            IsActive = request.IsActive
        };
        db.PrintTemplates.Add(entity);
        await db.SaveChangesAsync(ct);
        return MapTemplate(entity);
    }

    public async Task<PrintTemplateDto> UpdateTemplateAsync(
        long id, UpdatePrintTemplateRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.PrintTemplates.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Print template {id} not found.");

        entity.TemplateType = request.TemplateType.Trim();
        entity.Name = request.Name.Trim();
        entity.TemplateContent = request.TemplateContent;
        entity.PaperWidth = request.PaperWidth;
        entity.IsDefault = request.IsDefault;
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return MapTemplate(entity);
    }

    public async Task<PagedResult<BarcodePrintJobDto>> SearchJobsAsync(
        BarcodePrintJobQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.BarcodePrintJobs.AsNoTracking()
            .Include(j => j.Branch)
            .Include(j => j.PrinterDevice)
            .Include(j => j.Product)
            .Include(j => j.Template)
            .Where(j => j.Branch.TenantId == tenantId);

        if (query.BranchId is long branchId) q = q.Where(j => j.BranchId == branchId);
        if (query.ProductId is long productId) q = q.Where(j => j.ProductId == productId);
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim();
            q = q.Where(j => j.Status == status);
        }

        q = q.OrderByDescending(j => j.CreatedAt);
        var total = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<BarcodePrintJobDto>
        {
            Items = rows.Select(MapJob).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<BarcodePrintJobDto?> GetJobByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.BarcodePrintJobs.AsNoTracking()
            .Include(j => j.Branch)
            .Include(j => j.PrinterDevice)
            .Include(j => j.Product)
            .Include(j => j.Template)
            .FirstOrDefaultAsync(j => j.Id == id && j.Branch.TenantId == tenantId, ct);
        return entity is null ? null : MapJob(entity);
    }

    public async Task<BarcodePrintJobDto> CreateJobAsync(
        CreateBarcodePrintJobRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var branch = await db.Branches.FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Branch {request.BranchId} not found.");
        var printer = await db.Devices
            .FirstOrDefaultAsync(d => d.Id == request.PrinterDeviceId && d.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Printer device {request.PrinterDeviceId} not found.");
        if (printer.BranchId != branch.Id)
            throw new ValidationAppException(["Printer device must belong to the same branch as the job."]);

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId && p.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Product {request.ProductId} not found.");
        var template = await db.PrintTemplates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Print template {request.TemplateId} not found.");

        if (request.BatchId is long batchId)
        {
            _ = await db.InventoryBatches.FirstOrDefaultAsync(b => b.Id == batchId, ct)
                ?? throw new NotFoundException($"Batch {batchId} not found.");
        }

        var entity = new BarcodePrintJob
        {
            BranchId = branch.Id,
            PrinterDeviceId = printer.Id,
            ProductId = product.Id,
            BatchId = request.BatchId,
            Quantity = request.Quantity,
            TemplateId = template.Id,
            Status = BarcodePrintJobStatuses.Queued,
            CreatedBy = currentUser.UserId,
            CreatedAt = DateTime.UtcNow
        };
        db.BarcodePrintJobs.Add(entity);
        await db.SaveChangesAsync(ct);
        return (await GetJobByIdAsync(entity.Id, ct))!;
    }

    public async Task<BarcodePrintJobDto> UpdateJobStatusAsync(
        long id, UpdateBarcodePrintJobStatusRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        if (!BarcodePrintJobStatuses.IsKnown(request.Status))
            throw new ValidationAppException([
                $"Status must be one of: {BarcodePrintJobStatuses.Queued}, {BarcodePrintJobStatuses.Printing}, {BarcodePrintJobStatuses.Printed}, {BarcodePrintJobStatuses.Failed}, {BarcodePrintJobStatuses.Cancelled}."
            ]);

        var status = BarcodePrintJobStatuses.Normalize(request.Status);
        var entity = await db.BarcodePrintJobs
            .FirstOrDefaultAsync(j => j.Id == id && j.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Barcode print job {id} not found.");

        entity.Status = status;
        if (status == BarcodePrintJobStatuses.Printed)
            entity.PrintedAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (await GetJobByIdAsync(entity.Id, ct))!;
    }

    public async Task<BarcodePrintJobDto> SimulateCompleteAsync(
        long id, SimulateBarcodePrintJobRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await db.BarcodePrintJobs
            .FirstOrDefaultAsync(j => j.Id == id && j.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Barcode print job {id} not found.");

        if (request.Fail)
        {
            entity.Status = BarcodePrintJobStatuses.Failed;
        }
        else
        {
            entity.Status = BarcodePrintJobStatuses.Printed;
            entity.PrintedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return (await GetJobByIdAsync(entity.Id, ct))!;
    }

    private static PrintTemplateDto MapTemplate(PrintTemplate t) => new()
    {
        Id = t.Id,
        TenantId = t.TenantId,
        TemplateType = t.TemplateType,
        Name = t.Name,
        TemplateContent = t.TemplateContent,
        PaperWidth = t.PaperWidth,
        IsDefault = t.IsDefault,
        IsActive = t.IsActive
    };

    private static BarcodePrintJobDto MapJob(BarcodePrintJob j) => new()
    {
        Id = j.Id,
        BranchId = j.BranchId,
        BranchName = j.Branch?.Name,
        PrinterDeviceId = j.PrinterDeviceId,
        PrinterDeviceName = j.PrinterDevice?.Name,
        ProductId = j.ProductId,
        ProductSku = j.Product?.Sku,
        ProductName = j.Product?.Name,
        BatchId = j.BatchId,
        Quantity = j.Quantity,
        TemplateId = j.TemplateId,
        TemplateName = j.Template?.Name,
        Status = j.Status,
        CreatedBy = j.CreatedBy,
        CreatedAt = j.CreatedAt,
        PrintedAt = j.PrintedAt
    };
}
