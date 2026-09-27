import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/services/auth.service';
import { AppSnackbarComponent } from '../shared';

interface NavItem {
  path: string;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, AppSnackbarComponent],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
})
export class ShellComponent {
  readonly nav: NavItem[] = [
    { path: '/dashboard', label: 'Dashboard', icon: 'DB' },
    { path: '/pos', label: 'Sales / POS', icon: 'POS' },
    { path: '/products', label: 'Products', icon: 'PR' },
    { path: '/inventory', label: 'Inventory', icon: 'INV' },
    { path: '/purchases', label: 'Purchases', icon: 'PO' },
    { path: '/suppliers', label: 'Suppliers', icon: 'SU' },
    { path: '/customers', label: 'Customers', icon: 'CU' },
    { path: '/sales-returns', label: 'Sales Returns', icon: 'SR' },
    { path: '/purchase-returns', label: 'Purchase Returns', icon: 'PRR' },
    { path: '/expenses', label: 'Expenses', icon: 'EX' },
    { path: '/price-lists', label: 'Price Lists', icon: 'PL' },
    { path: '/product-prices', label: 'Product Prices', icon: 'PP' },
    { path: '/tax-profiles', label: 'Tax Profiles', icon: 'TX' },
    { path: '/reports', label: 'Reports', icon: 'RP' },
    { path: '/users', label: 'Users & Roles', icon: 'UR' },
    { path: '/branches', label: 'Branches', icon: 'BR' },
    { path: '/settings', label: 'Settings', icon: 'ST' },
  ];

  constructor(readonly auth: AuthService) {}

  logout(): void {
    this.auth.logout();
  }
}
