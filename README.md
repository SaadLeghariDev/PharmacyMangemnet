# Pharmacy Management System

Database-first pharmacy management platform.

- **Phase 1** — SQL Server DDL  
- **Phase 2A–2E** — ASP.NET API (Auth, Products, Purchasing, Inventory, Sales/POS, Cash, Customers, Prescriptions, Controlled, Fiscal stub)  
- **Phase 3** — Angular counter UI (`web/`) — login, shell, usable POS

## Structure

```text
PharmacyManagement.sln
src/                      # .NET API + Application + Infrastructure
tests/
database/                 # Phase 1 DDL (source of truth — no EF migrations)
web/                      # Angular 19 UI (Phase 3)
```

## Prerequisites

- .NET 8 SDK
- SQL Server (Docker script below, or local Express / `F:\Pharmacymanagemnt` clone with LocalExpress)
- Node.js 20+ (Angular UI)

## Database (Phase 1)

```bash
./database/scripts/docker-up.sh
./database/scripts/apply_all.sh
```

Docker default: `localhost,14333` / sa / `Your_strong_Password123`.

### Local SQL Express / Windows clone

If you cloned under `F:\Pharmacymanagemnt` (or similar), point the API at Express via `appsettings.LocalExpress.json` or:

`Server=DESKTOP-H9TF8EF\SQLEXPRESS;Database=PharmacyManagement;Trusted_Connection=True;TrustServerCertificate=True;`

## API (Phase 2E+)

```bash
dotnet restore
dotnet build
dotnet run --project src/PharmacyManagement.Api --urls http://127.0.0.1:5329
```

- Swagger: http://127.0.0.1:5329/swagger  
- Health: http://127.0.0.1:5329/health  
- Connection string key: `ConnectionStrings:PharmacyManagement`  
- CORS origins include Angular `http://127.0.0.1:43123` (see `Cors:AllowedOrigins`)

### Development login

Seed user `admin` — in **Development**, password is `Admin@12345` (`AuthBootstrap:DevelopmentAdminPassword`).

### Modules

| Module | Base route |
|--------|------------|
| Auth / Org / Products | `/api/v1/...` |
| Purchasing / GRN | `/api/v1/purchase-orders`, `/api/v1/goods-receipts` |
| Inventory | `/api/v1/inventory`, stock transfers/adjustments/counts |
| Sales / Held / Returns | `/api/v1/sales`, `/api/v1/held-sales`, `/api/v1/sale-returns` |
| Cash / Customers | `/api/v1/cash-shifts`, `/api/v1/customers` |
| Rx / Controlled / Fiscal | `/api/v1/prescriptions`, `/api/v1/controlled-registers`, `/api/v1/fiscal/documents` |

## Angular UI (Phase 3)

```bash
cd web
npm install
npm start
```

- App: http://127.0.0.1:43123  
- Talks to API at `http://127.0.0.1:5329` (`web/src/environments/environment.ts`)  
- Working slice: **Login**, **app shell** (module nav stubs), **POS / Sales** (search/barcode, cart, hold/resume, FEFO complete sale, invoice/receipt)

```bash
cd web && npm run build
```

## Tests

```bash
dotnet test
```

## Explicit non-goals (current)

Full GRN/FBR/controlled UI polish, live FBR credentials, EF migrations / schema changes.
