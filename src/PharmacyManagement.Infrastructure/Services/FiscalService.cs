using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Fiscal;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class FiscalService(
    PharmacyManagementDbContext db,
    ICurrentUserService currentUser,
    IFiscalGateway gateway) : IFiscalService
{
    private long RequireTenantId() =>
        currentUser.TenantId ?? throw new UnauthorizedAppException("Tenant scope required.");

    public async Task<PagedResult<FiscalDocumentDto>> SearchAsync(FiscalDocumentQuery query, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var q = db.FiscalDocuments.AsNoTracking()
            .Include(d => d.Sale)
            .Where(d => d.Sale.Branch.TenantId == tenantId);

        if (query.SaleId is long saleId) q = q.Where(d => d.SaleId == saleId);
        if (!string.IsNullOrWhiteSpace(query.SubmissionStatus))
            q = q.Where(d => d.SubmissionStatus == query.SubmissionStatus);

        q = q.OrderByDescending(d => d.Id);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<FiscalDocumentDto>
        {
            Items = items.Select(d => Map(d)).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<FiscalDocumentDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();
        var entity = await Query()
            .FirstOrDefaultAsync(d => d.Id == id && d.Sale.Branch.TenantId == tenantId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<FiscalDocumentDto> CreateAsync(CreateFiscalDocumentRequest request, CancellationToken ct = default)
    {
        var tenantId = RequireTenantId();

        var sale = await db.Sales
            .Include(s => s.Branch)
            .Include(s => s.SaleLines)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId && s.Branch.TenantId == tenantId, ct)
            ?? throw new NotFoundException($"Sale {request.SaleId} not found.");

        if (sale.Status != "Completed")
            throw new ValidationAppException(["Fiscal documents can only be created for Completed sales."]);

        var existing = await db.FiscalDocuments.AsNoTracking()
            .FirstOrDefaultAsync(d => d.SaleId == sale.Id, ct);
        if (existing is not null)
            return (await GetByIdAsync(existing.Id, ct))!;

        var provider = string.IsNullOrWhiteSpace(request.Provider) ? "FBR" : request.Provider.Trim();
        var doc = new FiscalDocument
        {
            SaleId = sale.Id,
            Provider = provider,
            DocumentType = request.DocumentType.Trim(),
            InternalInvoiceNumber = sale.InvoiceNumber,
            SubmissionStatus = "Pending"
        };

        foreach (var line in sale.SaleLines)
        {
            var taxable = Math.Round(line.NetAmount - line.TaxAmount, 4);
            if (taxable < 0) taxable = 0;
            var rate = taxable > 0 && line.TaxAmount > 0
                ? Math.Round(line.TaxAmount / taxable * 100m, 4)
                : 0;
            doc.FiscalDocumentLines.Add(new FiscalDocumentLine
            {
                SaleLineId = line.Id,
                TaxableAmount = taxable,
                TaxRate = rate,
                TaxAmount = Math.Round(line.TaxAmount, 4)
            });
        }

        sale.Fbrstatus ??= "Pending";
        db.FiscalDocuments.Add(doc);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(doc.Id, ct))!;
    }

    public Task<FiscalDocumentDto> SubmitAsync(long id, FiscalSubmitRequest request, CancellationToken ct = default) =>
        SubmitInternalAsync(id, request, retry: false, ct);

    public Task<FiscalDocumentDto> RetryAsync(long id, FiscalSubmitRequest request, CancellationToken ct = default) =>
        SubmitInternalAsync(id, request, retry: true, ct);

    private async Task<FiscalDocumentDto> SubmitInternalAsync(
        long id, FiscalSubmitRequest request, bool retry, CancellationToken ct)
    {
        var tenantId = RequireTenantId();

        await using var tx = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var doc = await db.FiscalDocuments
                .Include(d => d.Sale).ThenInclude(s => s.Branch)
                .Include(d => d.FiscalDocumentLines)
                .Include(d => d.FiscalSubmissions)
                .FirstOrDefaultAsync(d => d.Id == id && d.Sale.Branch.TenantId == tenantId, ct)
                ?? throw new NotFoundException($"Fiscal document {id} not found.");

            if (doc.Sale.Status != "Completed")
                throw new ValidationAppException(["Cannot submit fiscal document for a non-completed sale."]);

            if (!retry)
            {
                if (doc.SubmissionStatus is "Accepted")
                    throw new ValidationAppException(["Fiscal document already accepted."]);
                if (doc.SubmissionStatus is not ("Pending" or "Failed" or "Rejected" or "Retrying"))
                    throw new ValidationAppException([$"Cannot submit from status {doc.SubmissionStatus}."]);
            }
            else
            {
                if (doc.SubmissionStatus is not ("Failed" or "Rejected" or "Retrying"))
                    throw new ValidationAppException([
                        $"Retry only allowed for Failed/Rejected/Retrying (current: {doc.SubmissionStatus})."]);
            }

            var attemptNo = (doc.FiscalSubmissions.Count == 0
                ? 0
                : doc.FiscalSubmissions.Max(s => s.AttemptNo)) + 1;

            doc.SubmissionStatus = retry ? "Retrying" : "Submitted";
            doc.SubmittedAt = DateTime.UtcNow;

            var result = await gateway.SubmitAsync(new FiscalGatewayRequest
            {
                Provider = doc.Provider,
                InternalInvoiceNumber = doc.InternalInvoiceNumber ?? doc.Sale.InvoiceNumber,
                NetAmount = doc.Sale.NetAmount,
                TaxAmount = doc.Sale.TaxAmount,
                ForceFailure = request.ForceFailure
            }, ct);

            var submission = new FiscalSubmission
            {
                FiscalDocumentId = doc.Id,
                AttemptNo = attemptNo,
                RequestId = Guid.NewGuid().ToString("N"),
                Status = result.Success ? "Success" : result.Status is "Rejected" ? "Rejected" : "Failed",
                HttpStatusCode = result.HttpStatusCode,
                RequestPayload = result.RequestPayload,
                ResponsePayload = result.ResponsePayload,
                ErrorCode = result.ErrorCode,
                ErrorMessage = result.ErrorMessage,
                SubmittedAt = DateTime.UtcNow
            };
            db.FiscalSubmissions.Add(submission);

            doc.RawRequest = result.RequestPayload;
            doc.RawResponse = result.ResponsePayload;
            doc.ResponseAt = DateTime.UtcNow;

            if (result.Success)
            {
                doc.SubmissionStatus = "Accepted";
                doc.FbrinvoiceNumber = result.FbrInvoiceNumber;
                doc.ExternalInvoiceNumber = result.FbrInvoiceNumber;
                doc.Qrdata = result.QrData;
                doc.VerificationUrl = result.VerificationUrl;
                doc.Sale.Fbrstatus = "Accepted";
            }
            else
            {
                doc.SubmissionStatus = submission.Status == "Rejected" ? "Rejected" : "Failed";
                doc.Sale.Fbrstatus = "Failed";
            }

            doc.Sale.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            if (tx is not null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            throw;
        }

        return (await GetByIdAsync(id, ct))!;
    }

    private IQueryable<FiscalDocument> Query() =>
        db.FiscalDocuments.AsNoTracking()
            .Include(d => d.Sale)
            .Include(d => d.FiscalDocumentLines)
            .Include(d => d.FiscalSubmissions);

    private static FiscalDocumentDto Map(FiscalDocument d) => new()
    {
        Id = d.Id,
        SaleId = d.SaleId,
        Provider = d.Provider,
        DocumentType = d.DocumentType,
        InternalInvoiceNumber = d.InternalInvoiceNumber,
        ExternalInvoiceNumber = d.ExternalInvoiceNumber,
        FbrInvoiceNumber = d.FbrinvoiceNumber,
        SubmissionStatus = d.SubmissionStatus,
        SubmittedAt = d.SubmittedAt,
        ResponseAt = d.ResponseAt,
        QrData = d.Qrdata,
        VerificationUrl = d.VerificationUrl,
        SaleFbrStatus = d.Sale?.Fbrstatus,
        Lines = d.FiscalDocumentLines?.Select(l => new FiscalDocumentLineDto
        {
            Id = l.Id,
            SaleLineId = l.SaleLineId,
            TaxableAmount = l.TaxableAmount,
            TaxRate = l.TaxRate,
            TaxAmount = l.TaxAmount
        }).ToList() ?? [],
        Submissions = d.FiscalSubmissions?.OrderBy(s => s.AttemptNo).Select(s => new FiscalSubmissionDto
        {
            Id = s.Id,
            AttemptNo = s.AttemptNo,
            RequestId = s.RequestId,
            Status = s.Status,
            HttpStatusCode = s.HttpStatusCode,
            ErrorCode = s.ErrorCode,
            ErrorMessage = s.ErrorMessage,
            SubmittedAt = s.SubmittedAt
        }).ToList() ?? []
    };
}
