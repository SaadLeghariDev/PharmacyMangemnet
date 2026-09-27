using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Options;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class Phase13FiscalGatewayTests
{
    [Fact]
    public async Task Selecting_uses_mock_when_BaseUrl_unset()
    {
        var opts = Microsoft.Extensions.Options.Options.Create(new FiscalOptions { BaseUrl = "", ApiKey = "" });
        var mock = new MockFiscalGateway();
        var live = new LiveHttpFiscalGateway(
            new StubHttpClientFactory(_ => throw new InvalidOperationException("live should not be called")),
            opts,
            NullLogger<LiveHttpFiscalGateway>.Instance);
        var sut = new SelectingFiscalGateway(mock, live, opts, NullLogger<SelectingFiscalGateway>.Instance);

        var result = await sut.SubmitAsync(new FiscalGatewayRequest
        {
            Provider = "FBR",
            InternalInvoiceNumber = "INV-MOCK-1",
            NetAmount = 100,
            TaxAmount = 17
        });

        result.Success.Should().BeTrue();
        result.FbrInvoiceNumber.Should().StartWith("FBR-");
        result.VerificationUrl.Should().Contain("mock-fbr.local");
    }

    [Fact]
    public async Task Live_posts_and_maps_success_response()
    {
        var handler = new StubHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"fbrInvoiceNumber":"FBR-LIVE-99","qrData":"QR|99","verificationUrl":"https://fbr.example/v/99"}""",
                    Encoding.UTF8,
                    "application/json")
            });
        var opts = Microsoft.Extensions.Options.Options.Create(new FiscalOptions
        {
            BaseUrl = "https://fbr.example",
            ApiKey = "test-key",
            SubmitPath = "/api/invoice/submit",
            AuthScheme = "ApiKey"
        });
        var sut = new LiveHttpFiscalGateway(
            new StubHttpClientFactory(_ => new HttpClient(handler) { BaseAddress = null }),
            opts,
            NullLogger<LiveHttpFiscalGateway>.Instance);

        var result = await sut.SubmitAsync(new FiscalGatewayRequest
        {
            Provider = "FBR",
            InternalInvoiceNumber = "INV-1",
            NetAmount = 250,
            TaxAmount = 40
        });

        result.Success.Should().BeTrue();
        result.Status.Should().Be("Success");
        result.FbrInvoiceNumber.Should().Be("FBR-LIVE-99");
        result.QrData.Should().Be("QR|99");
        result.HttpStatusCode.Should().Be(200);
        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.RequestUri!.ToString().Should().Be("https://fbr.example/api/invoice/submit");
        handler.LastRequest.Headers.Contains("X-API-Key").Should().BeTrue();
    }

    [Fact]
    public async Task Live_maps_http_error_to_failed()
    {
        var handler = new StubHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("""{"error":"bad invoice"}""", Encoding.UTF8, "application/json")
            });
        var opts = Microsoft.Extensions.Options.Options.Create(new FiscalOptions
        {
            BaseUrl = "https://fbr.example",
            ApiKey = "k",
            AuthScheme = "Bearer"
        });
        var sut = new LiveHttpFiscalGateway(
            new StubHttpClientFactory(_ => new HttpClient(handler)),
            opts,
            NullLogger<LiveHttpFiscalGateway>.Instance);

        var result = await sut.SubmitAsync(new FiscalGatewayRequest
        {
            InternalInvoiceNumber = "INV-BAD",
            NetAmount = 10,
            TaxAmount = 1
        });

        result.Success.Should().BeFalse();
        result.Status.Should().Be("Rejected");
        result.HttpStatusCode.Should().Be(400);
        handler.LastRequest!.Headers.Authorization!.Scheme.Should().Be("Bearer");
    }

    [Fact]
    public async Task Selecting_uses_live_when_BaseUrl_set()
    {
        var handler = new StubHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"invoice":"LIVE-1"}""", Encoding.UTF8, "application/json")
            });
        var opts = Microsoft.Extensions.Options.Options.Create(new FiscalOptions { BaseUrl = "https://fbr.example", ApiKey = "k" });
        var mock = new MockFiscalGateway();
        var live = new LiveHttpFiscalGateway(
            new StubHttpClientFactory(_ => new HttpClient(handler)),
            opts,
            NullLogger<LiveHttpFiscalGateway>.Instance);
        var sut = new SelectingFiscalGateway(mock, live, opts, NullLogger<SelectingFiscalGateway>.Instance);

        var result = await sut.SubmitAsync(new FiscalGatewayRequest
        {
            InternalInvoiceNumber = "INV-SEL",
            NetAmount = 1,
            TaxAmount = 0
        });

        result.Success.Should().BeTrue();
        result.FbrInvoiceNumber.Should().Be("LIVE-1");
        handler.LastRequest.Should().NotBeNull();
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(responder(request));
        }
    }

    private sealed class StubHttpClientFactory(Func<string, HttpClient> factory) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => factory(name);
    }
}
