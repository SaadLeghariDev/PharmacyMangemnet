using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using PharmacyManagement.Application.DTOs.Auth;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Options;
using PharmacyManagement.Infrastructure.Identity;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class AuthServicePasswordTests
{
    private static PharmacyManagementDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<PharmacyManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PharmacyManagementDbContext(options);
    }

    [Fact]
    public async Task Login_with_development_placeholder_hash_accepts_bootstrap_password()
    {
        await using var db = CreateDb();
        var tenant = new Tenant { Name = "Demo", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RowVersion = new byte[] { 1 } };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var role = new Role { TenantId = tenant.Id, Name = "Administrator", IsSystemRole = true };
        var perm = new Permission { Code = "ORG.VIEW", Name = "View Org", Module = "Organization" };
        role.Permissions.Add(perm);
        var user = new User
        {
            TenantId = tenant.Id,
            Username = "admin",
            FullName = "Admin",
            PasswordHash = AuthService.SeedPlaceholderHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }
        };
        user.Roles.Add(role);
        db.Permissions.Add(perm);
        db.Roles.Add(role);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var env = new Mock<IHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        var current = new Mock<ICurrentUserService>();
        var hasher = new PasswordHasher<PasswordIdentityUser>();
        var sut = new AuthService(
            db,
            Microsoft.Extensions.Options.Options.Create(new JwtOptions { Key = "Phase2A_Dev_SigningKey_ChangeInProduction_32+" }),
            Microsoft.Extensions.Options.Options.Create(new AuthBootstrapOptions { DevelopmentAdminPassword = "Admin@12345" }),
            env.Object,
            current.Object,
            hasher);

        var result = await sut.LoginAsync(new LoginRequest { Username = "admin", Password = "Admin@12345" });
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.User.Username.Should().Be("admin");
        result.User.Permissions.Should().Contain("ORG.VIEW");
    }

    [Fact]
    public async Task Login_rejects_wrong_password()
    {
        await using var db = CreateDb();
        var tenant = new Tenant { Name = "Demo", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, RowVersion = new byte[] { 1 } };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        db.Users.Add(new User
        {
            TenantId = tenant.Id,
            Username = "admin",
            FullName = "Admin",
            PasswordHash = AuthService.SeedPlaceholderHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }
        });
        await db.SaveChangesAsync();

        var env = new Mock<IHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var sut = new AuthService(
            db,
            Microsoft.Extensions.Options.Options.Create(new JwtOptions { Key = "Phase2A_Dev_SigningKey_ChangeInProduction_32+" }),
            Microsoft.Extensions.Options.Options.Create(new AuthBootstrapOptions { DevelopmentAdminPassword = "Admin@12345" }),
            env.Object,
            new Mock<ICurrentUserService>().Object,
            new PasswordHasher<PasswordIdentityUser>());

        var act = async () => await sut.LoginAsync(new LoginRequest { Username = "admin", Password = "wrong" });
        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }
}
