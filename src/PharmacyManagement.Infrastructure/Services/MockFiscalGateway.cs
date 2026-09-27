using System.Text.Json;
using PharmacyManagement.Application.Interfaces;

namespace PharmacyManagement.Infrastructure.Services;

/// <summary>Development/mock FBR adapter — no external credentials required.</summary>
public sealed class MockFiscalGateway : IFiscalGateway
{
    public Task<FiscalGatewayResult> SubmitAsync(FiscalGatewayRequest request, CancellationToken ct = default)
    {
        var requestId = Guid.NewGuid().ToString("N");
        var requestPayload = JsonSerializer.Serialize(new
        {
            request.Provider,
            request.InternalInvoiceNumber,
            request.NetAmount,
            request.TaxAmount,
            requestId
        });

        if (request.ForceFailure)
        {
            return Task.FromResult(new FiscalGatewayResult
            {
                Success = false,
                Status = "Failed",
                HttpStatusCode = 503,
                ErrorCode = "MOCK_GATEWAY_DOWN",
                ErrorMessage = "Mock FBR gateway forced failure.",
                RequestPayload = requestPayload,
                ResponsePayload = JsonSerializer.Serialize(new { error = "gateway unavailable" })
            });
        }

        var fbrNumber = $"FBR-{DateTime.UtcNow:yyyyMMddHHmmss}-{request.InternalInvoiceNumber}";
        var qr = $"FBR|{fbrNumber}|{request.NetAmount:0.00}";
        return Task.FromResult(new FiscalGatewayResult
        {
            Success = true,
            Status = "Success",
            HttpStatusCode = 200,
            FbrInvoiceNumber = fbrNumber,
            QrData = qr,
            VerificationUrl = $"https://mock-fbr.local/verify/{Uri.EscapeDataString(fbrNumber)}",
            RequestPayload = requestPayload,
            ResponsePayload = JsonSerializer.Serialize(new
            {
                invoice = fbrNumber,
                status = "Accepted",
                qr
            })
        });
    }
}
