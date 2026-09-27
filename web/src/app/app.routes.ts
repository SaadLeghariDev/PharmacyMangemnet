import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';
import { LoginComponent } from './features/login/login.component';
import { PosComponent } from './features/pos/pos.component';
import { ExpensesPageComponent } from './features/expenses/expenses-page.component';
import { SupplierPaymentsPageComponent } from './features/supplier-payments/supplier-payments-page.component';
import { PurchaseReturnsPageComponent } from './features/purchase-returns/purchase-returns-page.component';
import { SuppliersPageComponent } from './features/suppliers/suppliers-page.component';
import { SupplierLedgerPageComponent } from './features/suppliers/supplier-ledger-page.component';
import { PriceListsPageComponent } from './features/price-lists/price-lists-page.component';
import { ProductPricesPageComponent } from './features/product-prices/product-prices-page.component';
import { TaxProfilesPageComponent } from './features/tax-profiles/tax-profiles-page.component';
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
      { path: '', pathMatch: 'full', redirectTo: 'pos' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'pos', component: PosComponent },
      { path: 'products', component: ListPageComponent, data: { title: 'Products', kind: 'products' } },
      { path: 'inventory', component: ListPageComponent, data: { title: 'Inventory', kind: 'inventory' } },
      { path: 'purchases', component: ListPageComponent, data: { title: 'Purchases', kind: 'purchases' } },
      { path: 'suppliers', component: SuppliersPageComponent },
      { path: 'suppliers/:id/ledger', component: SupplierLedgerPageComponent },
      { path: 'supplier-payments', component: SupplierPaymentsPageComponent },
      { path: 'customers', component: ListPageComponent, data: { title: 'Customers', kind: 'customers' } },
      {
        path: 'sales-returns',
        component: ListPageComponent,
        data: { title: 'Sales Returns', kind: 'sales-returns' },
      },
      { path: 'purchase-returns', component: PurchaseReturnsPageComponent },
      { path: 'expenses', component: ExpensesPageComponent },
      { path: 'price-lists', component: PriceListsPageComponent },
      { path: 'product-prices', component: ProductPricesPageComponent },
      { path: 'tax-profiles', component: TaxProfilesPageComponent },
      { path: 'reports', component: ListPageComponent, data: { title: 'Reports', kind: 'reports' } },
      { path: 'users', component: ListPageComponent, data: { title: 'Users & Roles', kind: 'users' } },
      { path: 'branches', component: ListPageComponent, data: { title: 'Branches', kind: 'branches' } },
      { path: 'settings', component: ListPageComponent, data: { title: 'Settings', kind: 'settings' } },
    ],
  },
  { path: '**', redirectTo: 'pos' },
];
