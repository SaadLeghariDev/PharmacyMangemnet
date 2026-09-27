# Pharmacy Management System

Database-first pharmacy management platform. Phase 1 delivers SQL Server DDL; Phase 2A delivers an ASP.NET Core Web API (Auth, Organization, Products).

## Structure

```text
PharmacyManagement.sln
src/
  PharmacyManagement.Api/
  PharmacyManagement.Application/
  PharmacyManagement.Domain/
  PharmacyManagement.Infrastructure/
tests/
  PharmacyManagement.Application.Tests/
database/                 # Phase 1 DDL (source of truth — no EF migrations)
```

## Prerequisites

- .NET 8 SDK
- SQL Server (Docker script below, or local Express)

## Database (Phase 1)

```bash
./database/scripts/docker-up.sh
./database/scripts/apply_all.sh
```

Docker default: `localhost,14333` / sa / `Your_strong_Password123`.

## API (Phase 2A)

```bash
dotnet restore
dotnet build
dotnet run --project src/PharmacyManagement.Api --urls http://127.0.0.1:5288
```

- Swagger: http://127.0.0.1:5288/swagger
- Health: http://127.0.0.1:5288/health
- Connection string key: `ConnectionStrings:PharmacyManagement`

### Local SQL Express (PC)

Use `appsettings.LocalExpress.json` or override:

`Server=DESKTOP-H9TF8EF\SQLEXPRESS;Database=PharmacyManagement;Trusted_Connection=True;TrustServerCertificate=True;`

### Development login

Seed user `admin` has a placeholder password hash. In **Development**, password is `Admin@12345` (see `AuthBootstrap:DevelopmentAdminPassword`). Replace `PasswordHash` with an ASP.NET Identity hash before production.

## Tests

```bash
dotnet test
```

## Explicit non-goals (Phase 2A)

Purchases, inventory transactions, sales/POS, returns, Angular UI, EF migrations, schema changes.
