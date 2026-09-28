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
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Optional Local SQL Express overrides (file may be absent). Re-add env vars so
    // launch profiles (Docker vs LocalExpress) still win over the JSON file.
    builder.Configuration.AddJsonFile("appsettings.LocalExpress.json", optional: true, reloadOnChange: true);
    builder.Configuration.AddEnvironmentVariables();

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
            Description = "Phase 2E — Auth, Org, Products, Purchasing, Inventory, Sales, Cash, Customers, Prescriptions, Controlled, Fiscal (Database-First)"
        });
        // Avoid schemaId collisions (e.g. ApiResponse vs ApiResponse<T>, nested types).
        c.CustomSchemaIds(type =>
            (type.FullName ?? type.Name).Replace("+", ".", StringComparison.Ordinal));
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

    // Serve OpenAPI before exception JSON wrapping so schema-gen failures
    // are not turned into non-OpenAPI ApiResponse payloads for Swagger UI.
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Relative URL so VS https://localhost:7xxx / path-base still resolve correctly.
        c.SwaggerEndpoint("v1/swagger.json", "Pharmacy Management API v1");
        c.RoutePrefix = "swagger";
        c.EnableDeepLinking();
    });

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();

    app.UseCors("DefaultCors");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

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

public partial class Program;
