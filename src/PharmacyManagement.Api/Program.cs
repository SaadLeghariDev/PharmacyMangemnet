using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PharmacyManagement.Api.Authorization;
using PharmacyManagement.Api.Middleware;
using PharmacyManagement.Application;
using PharmacyManagement.Application.Options;
using PharmacyManagement.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Local SQL Express overrides — Development only (never on MonsterASP / Production).
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddJsonFile("appsettings.LocalExpress.json", optional: true, reloadOnChange: true);
        // Re-apply env vars so launch profiles still win over the JSON file.
        builder.Configuration.AddEnvironmentVariables();
    }

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddPermissionPolicies();

    var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                ClockSkew = TimeSpan.FromMinutes(1),
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
        });

    var corsOrigins = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()?.AllowedOrigins
        ?? ["http://localhost:4200"];
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DefaultCors", policy =>
            policy.WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod());
    });

    var connectionString = builder.Configuration.GetConnectionString("PharmacyManagement");
    var healthChecks = builder.Services.AddHealthChecks();
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        healthChecks.AddSqlServer(connectionString, name: "sqlserver");
    }
    else
    {
        Log.Warning("ConnectionStrings:PharmacyManagement is missing; SQL health check not registered");
    }

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Pharmacy Management API",
            Version = "v1",
            Description = "Phase 2E - Auth, Org, Products, Purchasing, Inventory, Sales, Cash, Customers, Prescriptions, Controlled, Fiscal (Database-First)"
        });
        // FullName avoids ApiResponse vs ApiResponse<T> collisions. Dots/backticks break
        // Swagger UI's $ref resolver (UI then reports a bogus "missing version field" error).
        c.CustomSchemaIds(static type => SanitizeSchemaId(type));
        c.CustomOperationIds(api =>
            $"{api.ActionDescriptor.RouteValues["controller"]}_{api.ActionDescriptor.RouteValues["action"]}_{api.HttpMethod}");
        c.IgnoreObsoleteActions();
        // DateOnly / TimeOnly map cleanly for Swagger UI under older Swashbuckle hosts.
        c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
        c.MapType<DateOnly?>(() => new OpenApiSchema { Type = "string", Format = "date", Nullable = true });
        c.MapType<TimeOnly>(() => new OpenApiSchema { Type = "string", Format = "time" });
        c.MapType<TimeOnly?>(() => new OpenApiSchema { Type = "string", Format = "time", Nullable = true });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // OpenAPI JSON via Swashbuckle; UI via Scalar (Swagger UI fails on this large dotted-schema doc).
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "/openapi/{documentName}.json";
    });

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();

    app.UseCors("DefaultCors");
    app.UseAuthentication();
    app.UseAuthorization();

    // Serve Angular SPA from wwwroot (production publish to medistock.tryasp.net).
    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Pharmacy Management API")
            .WithOpenApiRoutePattern("/openapi/{documentName}.json");
    });

    // Angular client-side routes (exclude API / docs / health).
    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program
{
    private static string SanitizeSchemaId(Type type)
    {
        var raw = type.FullName ?? type.Name;
        return raw
            .Replace('+', '.')
            .Replace('.', '_')
            .Replace('`', '_')
            .Replace(',', '_')
            .Replace('[', '_')
            .Replace(']', '_')
            .Replace(' ', '_');
    }
}
