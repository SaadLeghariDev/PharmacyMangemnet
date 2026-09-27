using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Options;

namespace PharmacyManagement.Infrastructure.Services;

/// <summary>
/// Live FBR HTTP adapter. Posts invoice payload to <see cref="FiscalOptions.BaseUrl"/> + SubmitPath.
/// Expects JSON response with optional fields: invoice / fbrInvoiceNumber / qr / qrData / verificationUrl / status.
/// </summary>
public sealed class LiveHttpFiscalGateway(
    IHttpClientFactory httpClientFactory,
    IOptions<FiscalOptions> options,
    ILogger<LiveHttpFiscalGateway> logger) : IFiscalGateway
{
    public const string HttpClientName = "FiscalFbr";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<FiscalGatewayResult> SubmitAsync(FiscalGatewayRequest request, CancellationToken ct = default)
    {
        var opts = options.Value;
        var baseUrl = (opts.BaseUrl ?? string.Empty).Trim().TrimEnd('/');
        var path = string.IsNullOrWhiteSpace(opts.SubmitPath) ? "/api/invoice/submit" : opts.SubmitPath.Trim();
        if (!path.StartsWith('/')) path = "/" + path;
        var url = baseUrl + path;

        var body = new
        {
            provider = string.IsNullOrWhiteSpace(request.Provider) ? opts.Provider : request.Provider,
            internalInvoiceNumber = request.InternalInvoiceNumber,
            netAmount = request.NetAmount,
            taxAmount = request.TaxAmount,
            forceFailure = request.ForceFailure
        };
        var requestPayload = JsonSerializer.Serialize(body, JsonOptions);

        if (request.ForceFailure)
        {
            return new FiscalGatewayResult
            {
                Success = false,
                Status = "Failed",
                HttpStatusCode = 503,
                ErrorCode = "FORCE_FAILURE",
                ErrorMessage = "ForceFailure requested before live FBR call.",
                RequestPayload = requestPayload,
                ResponsePayload = JsonSerializer.Serialize(new { error = "forceFailure" })
            };
        }

        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(requestPayload, Encoding.UTF8, "application/json")
            };

            var apiKey = opts.ApiKey?.Trim();
            if (!string.IsNullOrEmpty(apiKey))
            {
                if (string.Equals(opts.AuthScheme, "Bearer", StringComparison.OrdinalIgnoreCase))
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                else
                    httpRequest.Headers.TryAddWithoutValidation("X-API-Key", apiKey);
            }

            using var response = await client.SendAsync(httpRequest, ct);
            var responsePayload = await response.Content.ReadAsStringAsync(ct);
            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Live FBR submit failed HTTP {Status} for {Invoice}", statusCode, request.InternalInvoiceNumber);
                return new FiscalGatewayResult
                {
                    Success = false,
                    Status = statusCode is >= 400 and < 500 ? "Rejected" : "Failed",
                    HttpStatusCode = statusCode,
                    ErrorCode = $"HTTP_{statusCode}",
                    ErrorMessage = Truncate(responsePayload, 500) is { Length: > 0 } msg
                        ? msg
                        : $"FBR HTTP {(int)response.StatusCode}",
                    RequestPayload = requestPayload,
                    ResponsePayload = responsePayload
                };
            }

            string? fbrNumber = null;
            string? qr = null;
            string? verifyUrl = null;
            try
            {
                using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(responsePayload) ? "{}" : responsePayload);
                var root = doc.RootElement;
                fbrNumber = ReadString(root, "fbrInvoiceNumber")
                    ?? ReadString(root, "invoice")
                    ?? ReadString(root, "externalInvoiceNumber");
                qr = ReadString(root, "qrData") ?? ReadString(root, "qr");
                verifyUrl = ReadString(root, "verificationUrl");
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Live FBR response was not JSON for {Invoice}", request.InternalInvoiceNumber);
            }

            if (string.IsNullOrWhiteSpace(fbrNumber))
                fbrNumber = $"FBR-LIVE-{request.InternalInvoiceNumber}";

            return new FiscalGatewayResult
            {
                Success = true,
                Status = "Success",
                HttpStatusCode = statusCode,
                FbrInvoiceNumber = fbrNumber,
                QrData = qr,
                VerificationUrl = verifyUrl,
                RequestPayload = requestPayload,
                ResponsePayload = responsePayload
            };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Live FBR submit error for {Invoice}", request.InternalInvoiceNumber);
            return new FiscalGatewayResult
            {
                Success = false,
                Status = "Failed",
                HttpStatusCode = 0,
                ErrorCode = "HTTP_EXCEPTION",
                ErrorMessage = ex.Message,
                RequestPayload = requestPayload,
                ResponsePayload = JsonSerializer.Serialize(new { error = ex.Message })
            };
        }
    }

    private static string? ReadString(JsonElement root, string name)
    {
        if (root.ValueKind != JsonValueKind.Object) return null;
        if (!root.TryGetProperty(name, out var prop)) return null;
        return prop.ValueKind == JsonValueKind.String ? prop.GetString() : prop.ToString();
    }

    private static string Truncate(string? s, int max) =>
        string.IsNullOrEmpty(s) ? string.Empty : s.Length <= max ? s : s[..max];
}
