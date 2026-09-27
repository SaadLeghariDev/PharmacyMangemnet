import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';
import { LoginComponent } from './features/login/login.component';
import { PosComponent } from './features/pos/pos.component';
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
      { path: 'suppliers', component: ListPageComponent, data: { title: 'Suppliers', kind: 'suppliers' } },
      { path: 'customers', component: ListPageComponent, data: { title: 'Customers', kind: 'customers' } },
      {
        path: 'sales-returns',
        component: ListPageComponent,
        data: { title: 'Sales Returns', kind: 'sales-returns' },
      },
      {
        path: 'purchase-returns',
        component: ListPageComponent,
        data: { title: 'Purchase Returns', kind: 'purchase-returns' },
      },
      { path: 'expenses', component: ListPageComponent, data: { title: 'Expenses', kind: 'expenses' } },
      { path: 'reports', component: ListPageComponent, data: { title: 'Reports', kind: 'reports' } },
      { path: 'users', component: ListPageComponent, data: { title: 'Users & Roles', kind: 'users' } },
      { path: 'branches', component: ListPageComponent, data: { title: 'Branches', kind: 'branches' } },
      { path: 'settings', component: ListPageComponent, data: { title: 'Settings', kind: 'settings' } },
    ],
  },
  { path: '**', redirectTo: 'pos' },
];
