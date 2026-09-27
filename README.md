# Pharmacy Management System

Database-first pharmacy management platform. Phase 1 SQL Server DDL; Phase 2A Auth/Org/Products; Phase 2B Purchasing + Inventory; Phase 2C Sales/POS; Phase 2D Cash/Void/Customer AR; Phase 2E Prescriptions / Controlled register / Fiscal stub.

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

## API (Phase 2E)

```bash
dotnet restore
dotnet build
dotnet run --project src/PharmacyManagement.Api --urls http://127.0.0.1:5329
```

- Swagger: http://127.0.0.1:5329/swagger
- Health: http://127.0.0.1:5329/health
- Connection string key: `ConnectionStrings:PharmacyManagement`

### Modules

| Module | Base route | Permissions |
|--------|------------|-------------|
| Auth / Org / Products | `/api/v1/...` | ORG.*, PROD.* (Phase 2A) |
| Suppliers | `/api/v1/suppliers` | `PROC.PO` |
| Purchase Orders | `/api/v1/purchase-orders` | `PROC.PO` |
| Goods Receipts | `/api/v1/goods-receipts` | `PROC.GRN` |
| Inventory queries | `/api/v1/inventory` | `INV.VIEW` |
| Stock Transfers | `/api/v1/stock-transfers` | `INV.TRANSFER` |
| Stock Adjustments | `/api/v1/stock-adjustments` | `INV.ADJUST` |
| Stock Counts | `/api/v1/stock-counts` | `INV.ADJUST` |
| Sales / POS | `/api/v1/sales` | `POS.SALE` |
| Held sales | `/api/v1/held-sales` | `POS.HOLD` |
| Sale returns | `/api/v1/sale-returns` | `POS.RETURN` |
| Sale void | `/api/v1/sales/{id}/void` | `POS.VOID` |
| Cash shifts | `/api/v1/cash-shifts` | `FIN.CASH` |
| Customers / AR | `/api/v1/customers` | `CUST.VIEW` / `CUST.EDIT` |
| Doctors | `/api/v1/doctors` | `RX.DISPENSE` |
| Prescriptions / dispense | `/api/v1/prescriptions` | `RX.DISPENSE` |
| Controlled registers | `/api/v1/controlled-registers` | `CTRL.MANAGE` |
| Fiscal / FBR | `/api/v1/fiscal/documents` | `FISCAL.SUBMIT` |

Document numbers use atomic `NumberSequences` increments (UPDLOCK) — never `MAX(Id)+1`.
Sale invoices are terminal-scoped (`SALE` + `TerminalId`); returns are branch-scoped (`RETURN`); customer codes are tenant-scoped (`CUSTOMER`); prescriptions are tenant-scoped (`PRESCRIPTION` / `RX-`); controlled registers are branch-scoped (`CTRL_REGISTER` / `CDR-`).

Fiscal submit uses a mock FBR gateway when no live credentials are configured (`ForceFailure` supported for retry testing).

### Local SQL Express (PC)

Use `appsettings.LocalExpress.json` or override:

`Server=DESKTOP-H9TF8EF\SQLEXPRESS;Database=PharmacyManagement;Trusted_Connection=True;TrustServerCertificate=True;`

### Development login

Seed user `admin` has a placeholder password hash. In **Development**, password is `Admin@12345` (see `AuthBootstrap:DevelopmentAdminPassword`). Replace `PasswordHash` with an ASP.NET Identity hash before production.

## Tests

```bash
dotnet test
```

## Explicit non-goals (Phase 2E)

Angular UI, live FBR credentials, prescription attachment upload, standalone refill-only API, supplier AR, EF migrations, schema changes.
