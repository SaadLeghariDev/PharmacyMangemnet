namespace PharmacyManagement.Application.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "PharmacyManagement";
    public string Audience { get; set; } = "PharmacyManagement.Api";
    public string Key { get; set; } = "CHANGE_ME_TO_A_LONG_SECRET_KEY_AT_LEAST_32_CHARS";
    public int ExpiryMinutes { get; set; } = 480;
}

public sealed class CorsOptions
{
    public const string SectionName = "Cors";
    public string[] AllowedOrigins { get; set; } = ["http://localhost:4200"];
}

public sealed class AuthBootstrapOptions
{
    public const string SectionName = "AuthBootstrap";
    /// <summary>Development-only plaintext password accepted when seed PasswordHash is a known placeholder.</summary>
    public string DevelopmentAdminPassword { get; set; } = "Admin@12345";
}

/// <summary>
/// FBR / fiscal gateway configuration. When <see cref="BaseUrl"/> is unset (or empty),
/// the mock gateway is used. Set via appsettings or env: Fiscal__BaseUrl, Fiscal__ApiKey.
/// Never commit real API keys.
/// </summary>
public sealed class FiscalOptions
{
    public const string SectionName = "Fiscal";

    /// <summary>Optional provider label written into requests (default FBR).</summary>
    public string Provider { get; set; } = "FBR";

    /// <summary>Live FBR HTTP base URL. When null/empty, mock gateway remains active.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>API key sent as X-API-Key (and Bearer when AuthScheme is Bearer).</summary>
    public string? ApiKey { get; set; }

    /// <summary>Relative path appended to BaseUrl for invoice submit (default /api/invoice/submit).</summary>
    public string SubmitPath { get; set; } = "/api/invoice/submit";

    /// <summary>Auth header style: ApiKey (X-API-Key) or Bearer.</summary>
    public string AuthScheme { get; set; } = "ApiKey";

    /// <summary>HTTP timeout seconds for live calls.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    public bool IsLiveConfigured =>
        !string.IsNullOrWhiteSpace(BaseUrl);
}
