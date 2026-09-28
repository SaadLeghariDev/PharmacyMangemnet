# Pharmacy Management System

Database-first pharmacy management platform.

- **Phase 1** — SQL Server DDL  
- **Phase 2A–2E** — ASP.NET API (Auth, Products, Purchasing, Inventory, Sales/POS, Cash, Customers, Prescriptions, Controlled, Fiscal)  
- **Phase 3** — Angular counter UI (`web/`) — login, shell, usable POS  
- **Phase 4** — Expenses + ExpenseCategories (API + Angular)
- **Phase 5** — Supplier payments / returns / ledger (API + Angular)
- **Phase 6** — Price lists, product prices, tax profiles/rates, sale InvoiceTaxes (API + Angular)
- **Phase 7** — Reorder rules, alert rules/alerts evaluate, notification templates/logs (API + Angular)
- **Phase 8** — Users / Roles / Branches / Tenant & Branch settings / Reason codes / Audit logs
- **Phase 9** — Hardware devices, print templates & barcode print jobs, attachment metadata (API + Angular)
- **Phase 10** — GL / Accounting: AccountTypes, Chart of Accounts, Journal entries (API + Angular)
- **Phase 11** — Offline sync nodes/batches/push/pull + idempotency key admin (API + Angular)
- **Phase 12** — Angular deepen: Products, Inventory, Purchases/GRN, Customers, Dashboard, Reports
- **Phase 13** — FBR fiscal: mock default + configurable live HTTP adapter

## Structure

```text
PharmacyManagement.sln
src/                      # .NET API + Application + Infrastructure
tests/
database/                 # Phase 1 DDL (source of truth — no EF migrations)
web/                      # Angular 19 UI (Phase 3+)
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

## API

```bash
dotnet restore
dotnet build
dotnet run --project src/PharmacyManagement.Api --launch-profile PharmacyManagement.Api
```

- Swagger: http://127.0.0.1:5288/swagger  
- OpenAPI JSON: http://127.0.0.1:5288/swagger/v1/swagger.json (must include `"openapi"`)  
- Health: http://127.0.0.1:5288/health  
- Connection string key: `ConnectionStrings:PharmacyManagement`  
- CORS origins include Angular `http://127.0.0.1:43123` (see `Cors:AllowedOrigins`)

### Launch profiles

| Profile | Use when | URL | DB |
|---------|----------|-----|----|
| `PharmacyManagement.Api` (default) | Docker SQL | `http://127.0.0.1:5288` | sa / `Your_strong_Password123` |
| `LocalExpress` | Windows SQL Express | `http://127.0.0.1:5288` | Trusted_Connection |
| `https` | VS default HTTPS | `https://localhost:7288` + `http://127.0.0.1:5288` | Docker SQL |
| `https (LocalExpress)` | VS HTTPS + Express | same dual URL | Trusted_Connection |

```bash
# Docker / default
dotnet run --project src/PharmacyManagement.Api --launch-profile PharmacyManagement.Api

# Local SQL Express (also loads optional appsettings.LocalExpress.json)
dotnet run --project src/PharmacyManagement.Api --launch-profile LocalExpress
```

#### Visual Studio / Windows

1. Select the **LocalExpress** profile (or **https (LocalExpress)** if you need HTTPS).
2. Before trusting Swagger UI, open the OpenAPI JSON directly:  
   http://127.0.0.1:5288/swagger/v1/swagger.json  
   The first key must be `"openapi"` (e.g. `"openapi": "3.0.4"`). If you see an `ApiResponse` error object, schema generation failed — check the console stack.
3. Then open http://127.0.0.1:5288/swagger (or `https://localhost:7288/swagger` on the https profiles).  
   Swagger UI uses a **relative** endpoint (`v1/swagger.json`) so it works under HTTPS / path-base.
4. If the browser opens the wrong host/port, stop debugging and confirm the active launch profile’s `applicationUrl`.

### Development login

Seed user `admin` — in **Development**, password is `Admin@12345` (`AuthBootstrap:DevelopmentAdminPassword`).  
**Before production:** replace the seed `PasswordHash` with a real ASP.NET Identity hash and disable/remove the development plaintext bootstrap password.

### FBR / Fiscal (Phase 13)

Submit/retry: `POST /api/v1/fiscal/documents/{id}/submit` and `/retry`.

| Mode | When |
|------|------|
| **Mock** (default) | `Fiscal:BaseUrl` empty / unset — no external calls |
| **Live HTTP** | `Fiscal:BaseUrl` set — posts JSON to `{BaseUrl}{SubmitPath}` |

Configuration (`appsettings` or environment variables — **do not commit secrets**):

| Key | Env | Purpose |
|-----|-----|---------|
| `Fiscal:BaseUrl` | `Fiscal__BaseUrl` | e.g. `https://fbr-api.example.com` |
| `Fiscal:ApiKey` | `Fiscal__ApiKey` | Sent as `X-API-Key` (or Bearer if `AuthScheme=Bearer`) |
| `Fiscal:SubmitPath` | `Fiscal__SubmitPath` | Default `/api/invoice/submit` |
| `Fiscal:AuthScheme` | `Fiscal__AuthScheme` | `ApiKey` (default) or `Bearer` |
| `Fiscal:TimeoutSeconds` | `Fiscal__TimeoutSeconds` | Default `30` |

### Modules

| Module | Base route | Permission |
|--------|------------|------------|
| Auth / Org / Products | `/api/v1/...` | ORG.*, PROD.* |
| Purchasing / GRN | `/api/v1/purchase-orders`, `/api/v1/goods-receipts` | PROC.PO, PROC.GRN |
| **Supplier payments** | `/api/v1/supplier-payments` | **PROC.SUPPLIER_PAY** |
| **Supplier returns** | `/api/v1/supplier-returns` | **PROC.SUPPLIER_RETURN** |
| **Supplier ledger** | `/api/v1/suppliers/{id}/ledger` | **PROC.SUPPLIER_PAY** |
| Inventory | `/api/v1/inventory`, stock transfers/adjustments/counts | INV.* |
| Sales / Held / Returns | `/api/v1/sales`, `/api/v1/held-sales`, `/api/v1/sale-returns` | POS.* |
| Cash / Customers | `/api/v1/cash-shifts`, `/api/v1/customers` | FIN.CASH, CUST.* |
| Expenses | `/api/v1/expenses`, `/api/v1/expense-categories` | FIN.EXPENSE |
| **Price lists** | `/api/v1/price-lists` | **PRICE.VIEW / PRICE.EDIT** |
| **Product prices** | `/api/v1/product-prices` | **PRICE.VIEW / PRICE.EDIT** |
| **Tax profiles / rates** | `/api/v1/tax-profiles`, `/api/v1/tax-profiles/{id}/rates` | **TAX.VIEW / TAX.EDIT** |
| **Product tax links** | `/api/v1/products/{id}/tax-profiles` | **TAX.VIEW / TAX.EDIT** |
| **Reorder rules** | `/api/v1/reorder-rules`, `/api/v1/reorder-rules/low-stock` | **INV.REORDER** |
| **Alert rules** | `/api/v1/alert-rules` | **ALERT.VIEW / ALERT.MANAGE** |
| **Alerts** | `/api/v1/alerts`, `.../evaluate`, `.../acknowledge`, `.../resolve` | **ALERT.VIEW / ALERT.MANAGE** |
| **Notification templates/logs** | `/api/v1/notification-templates`, `/api/v1/notification-logs` | **ALERT.VIEW / ALERT.MANAGE** |
| **Users / Roles** | `/api/v1/users`, `/api/v1/roles`, `/api/v1/permissions` | **SEC.USERS / SEC.ROLES** |
| **Settings / Reason codes / Audit** | `/api/v1/tenant-settings`, `/api/v1/branch-settings`, `/api/v1/reason-codes`, `/api/v1/audit-logs` | **ORG.VIEW / ORG.EDIT / SEC.USERS** |
| **Devices / assignments / settings / events** | `/api/v1/device-types`, `/api/v1/devices`, `/api/v1/device-assignments` | **HW.VIEW / HW.MANAGE** |
| **Print templates / barcode jobs** | `/api/v1/print-templates`, `/api/v1/barcode-print-jobs` | **PRINT.MANAGE** |
| **Attachments** | `/api/v1/attachments`, `/api/v1/entity-attachments` | **HW.VIEW / HW.MANAGE** |
| **Account types / COA** | `/api/v1/account-types`, `/api/v1/chart-of-accounts` | **FIN.COA** |
| **Journal entries** | `/api/v1/journal-entries` (+ `/post`, `/reverse`) | **FIN.JOURNAL** |
| **Sync** | `/api/v1/sync-nodes`, `/api/v1/sync-batches`, `/api/v1/sync/push\|pull`, `/api/v1/idempotency-keys` | **SYNC.VIEW / SYNC.MANAGE** |
| Rx / Controlled / Fiscal | `/api/v1/prescriptions`, `/api/v1/controlled-registers`, `/api/v1/fiscal/documents` | RX.*, CTRL.*, FISCAL.* |

## Angular UI

```bash
cd web
npm install
npm start
```

- App: http://127.0.0.1:43123  
- Talks to API at `http://127.0.0.1:5340` by default (`web/src/environments/environment.ts` — update to match your API port)  
- Screens: **Dashboard**, **POS**, **Products**, **Inventory**, **Purchases/GRN**, **Suppliers** (+ create/ledger), **Customers** (+ ledger), **Sales returns** (list stub), **Purchase returns**, **Expenses**, **Finance**, **Price lists / Product prices / Tax**, **Reorder rules**, **Alerts**, **Reports**, **Users & Roles**, **Branches**, **Hardware**, **Sync**, **Settings**

## Production checklist

1. **Password hash** — Replace seed admin `PasswordHash` with a real Identity hash; remove or blank `AuthBootstrap:DevelopmentAdminPassword` outside Development.
2. **JWT** — Set a long random `Jwt:Key` via secret store / env (`Jwt__Key`); rotate if ever exposed.
3. **Connection strings** — Use production SQL credentials via env (`ConnectionStrings__PharmacyManagement`); never commit production secrets.
4. **CORS** — Restrict `Cors:AllowedOrigins` to your real Angular HTTPS origins only.
5. **HTTPS** — Terminate TLS at reverse proxy or Kestrel; do not expose plain HTTP publicly.
6. **FBR** — Set `Fiscal__BaseUrl` + `Fiscal__ApiKey` only in the production secret store; leave empty to keep mock (never for real fiscal compliance).
7. **Health / Swagger** — Keep `/health` for probes; disable or protect Swagger in production if required by policy.
8. **Database-first** — Apply DDL scripts from `database/`; do not enable EF migrations against production.
