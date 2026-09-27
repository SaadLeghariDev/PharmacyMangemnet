using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Application.Options;
using PharmacyManagement.Infrastructure.Identity;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));
        services.Configure<AuthBootstrapOptions>(configuration.GetSection(AuthBootstrapOptions.SectionName));

        var connectionString = configuration.GetConnectionString("PharmacyManagement")
            ?? throw new InvalidOperationException("Connection string 'PharmacyManagement' is not configured.");

        services.AddDbContext<PharmacyManagementDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHasher<PasswordIdentityUser>, PasswordHasher<PasswordIdentityUser>>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
