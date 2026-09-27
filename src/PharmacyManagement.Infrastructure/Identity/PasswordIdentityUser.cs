namespace PharmacyManagement.Infrastructure.Identity;

/// <summary>Minimal identity user shape for ASP.NET PasswordHasher only (no Identity stores).</summary>
public sealed class PasswordIdentityUser
{
    public string UserName { get; set; } = string.Empty;
}
