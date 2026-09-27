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
