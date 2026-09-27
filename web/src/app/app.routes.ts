import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';
import { LoginComponent } from './features/login/login.component';
import { PosComponent } from './features/pos/pos.component';
import { ExpensesPageComponent } from './features/expenses/expenses-page.component';
import { ProductsPageComponent } from './features/products/products-page.component';
import { InventoryPageComponent } from './features/inventory/inventory-page.component';
import { PurchasesPageComponent } from './features/purchases/purchases-page.component';
import { CustomersPageComponent } from './features/customers/customers-page.component';
import { CustomerLedgerPageComponent } from './features/customers/customer-ledger-page.component';
import { ReportsPageComponent } from './features/reports/reports-page.component';
import { SupplierPaymentsPageComponent } from './features/supplier-payments/supplier-payments-page.component';
import { PurchaseReturnsPageComponent } from './features/purchase-returns/purchase-returns-page.component';
import { SuppliersPageComponent } from './features/suppliers/suppliers-page.component';
import { SupplierLedgerPageComponent } from './features/suppliers/supplier-ledger-page.component';
import { PriceListsPageComponent } from './features/price-lists/price-lists-page.component';
import { ProductPricesPageComponent } from './features/product-prices/product-prices-page.component';
import { TaxProfilesPageComponent } from './features/tax-profiles/tax-profiles-page.component';
import { ReorderRulesPageComponent } from './features/reorder-rules/reorder-rules-page.component';
import { AlertsPageComponent } from './features/alerts/alerts-page.component';
import { UsersPageComponent } from './features/users/users-page.component';
import { BranchesPageComponent } from './features/branches/branches-page.component';
import { SettingsPageComponent } from './features/settings/settings-page.component';
import { HardwarePageComponent } from './features/hardware/hardware-page.component';
import { FinancePageComponent } from './features/finance/finance-page.component';
import { SyncPageComponent } from './features/sync/sync-page.component';
import { ListPageComponent } from './features/stub/list-page.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { ShellComponent } from './layout/shell.component';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    component: LoginComponent,
  },
  {
    path: '',
    canActivate: [authGuard],
    component: ShellComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'pos', component: PosComponent },
      { path: 'products', component: ProductsPageComponent },
      { path: 'inventory', component: InventoryPageComponent },
      { path: 'reorder-rules', component: ReorderRulesPageComponent },
      { path: 'purchases', component: PurchasesPageComponent },
      { path: 'suppliers', component: SuppliersPageComponent },
      { path: 'suppliers/:id/ledger', component: SupplierLedgerPageComponent },
      { path: 'supplier-payments', component: SupplierPaymentsPageComponent },
      { path: 'customers', component: CustomersPageComponent },
      { path: 'customers/:id/ledger', component: CustomerLedgerPageComponent },
      {
        path: 'sales-returns',
        component: ListPageComponent,
        data: { title: 'Sales Returns', kind: 'sales-returns' },
      },
      { path: 'purchase-returns', component: PurchaseReturnsPageComponent },
      { path: 'expenses', component: ExpensesPageComponent },
      { path: 'finance', component: FinancePageComponent },
      { path: 'price-lists', component: PriceListsPageComponent },
      { path: 'product-prices', component: ProductPricesPageComponent },
      { path: 'tax-profiles', component: TaxProfilesPageComponent },
      { path: 'alerts', component: AlertsPageComponent },
      { path: 'reports', component: ReportsPageComponent },
      { path: 'users', component: UsersPageComponent },
      { path: 'branches', component: BranchesPageComponent },
      { path: 'hardware', component: HardwarePageComponent },
      { path: 'sync', component: SyncPageComponent },
      { path: 'settings', component: SettingsPageComponent },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];
