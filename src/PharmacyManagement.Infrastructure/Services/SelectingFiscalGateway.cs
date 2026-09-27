using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Options;

namespace PharmacyManagement.Infrastructure.Services;

/// <summary>
/// Routes fiscal submits to the live HTTP adapter when <see cref="FiscalOptions.BaseUrl"/> is set;
/// otherwise uses the mock gateway (dev / local default).
/// </summary>
public sealed class SelectingFiscalGateway(
    MockFiscalGateway mock,
    LiveHttpFiscalGateway live,
    IOptions<FiscalOptions> options,
    ILogger<SelectingFiscalGateway> logger) : IFiscalGateway
{
    public Task<FiscalGatewayResult> SubmitAsync(FiscalGatewayRequest request, CancellationToken ct = default)
    {
        if (options.Value.IsLiveConfigured)
        {
            logger.LogInformation(
                "Using live FBR HTTP gateway for invoice {Invoice}",
                request.InternalInvoiceNumber);
            return live.SubmitAsync(request, ct);
        }

        return mock.SubmitAsync(request, ct);
    }
}
