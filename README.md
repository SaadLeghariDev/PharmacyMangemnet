# Pharmacy Management System

Database-first pharmacy management platform.

- **Phase 1** — SQL Server DDL  
- **Phase 2A–2E** — ASP.NET API (Auth, Products, Purchasing, Inventory, Sales/POS, Cash, Customers, Prescriptions, Controlled, Fiscal stub)  
- **Phase 3** — Angular counter UI (`web/`) — login, shell, usable POS  
- **Phase 4** — Expenses + ExpenseCategories (API + Angular)
- **Phase 5** — Supplier payments / returns / ledger (API + Angular)
- **Phase 6** — Price lists, product prices, tax profiles/rates, sale InvoiceTaxes (API + Angular)

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

## API (Phase 6+)

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
| Rx / Controlled / Fiscal | `/api/v1/prescriptions`, `/api/v1/controlled-registers`, `/api/v1/fiscal/documents` | RX.*, CTRL.*, FISCAL.* |

At most one `PriceLists.IsDefault` per tenant. Sale complete resolves unit price preferring the default price list, computes tax from product tax profiles + active rates (ignores client `TaxAmount`), writes `InvoiceTaxes`, and includes tax in server totals. Product prices are deactivated via `EffectiveTo` (no hard delete).

## Angular UI

```bash
cd web
npm install
npm start
```

- App: http://127.0.0.1:43123  
- Talks to API at `http://127.0.0.1:5329` (`web/src/environments/environment.ts`)  
- Working slices: **Login**, **app shell**, **POS / Sales**, **Expenses**, **Suppliers** (+ ledger), **Supplier payments**, **Purchase returns**, **Price lists**, **Product prices**, **Tax profiles**
