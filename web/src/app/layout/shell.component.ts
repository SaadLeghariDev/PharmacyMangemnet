import { Component, DestroyRef, HostListener, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../core/services/auth.service';
import { AppSnackbarComponent } from '../shared';

interface NavItem {
  path: string;
  label: string;
  icon: string;
}

interface NavGroup {
  label: string;
  items: NavItem[];
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, AppSnackbarComponent],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
})
export class ShellComponent {
  private readonly destroyRef = inject(DestroyRef);
  readonly navOpen = signal(false);

  readonly navGroups: NavGroup[] = [
    {
      label: 'Overview',
      items: [{ path: '/dashboard', label: 'Dashboard', icon: 'DB' }],
    },
    {
      label: 'Sales',
      items: [
        { path: '/pos', label: 'Sales / POS', icon: 'POS' },
        { path: '/customers', label: 'Customers', icon: 'CU' },
        { path: '/sales-returns', label: 'Sales Returns', icon: 'SR' },
      ],
    },
    {
      label: 'Catalog',
      items: [
        { path: '/products', label: 'Products', icon: 'PR' },
        { path: '/inventory', label: 'Inventory', icon: 'INV' },
        { path: '/reorder-rules', label: 'Reorder Rules', icon: 'RR' },
        { path: '/price-lists', label: 'Price Lists', icon: 'PL' },
        { path: '/product-prices', label: 'Product Prices', icon: 'PP' },
        { path: '/tax-profiles', label: 'Tax Profiles', icon: 'TX' },
      ],
    },
    {
      label: 'Purchasing',
      items: [
        { path: '/purchases', label: 'Purchases', icon: 'PO' },
        { path: '/suppliers', label: 'Suppliers', icon: 'SU' },
        { path: '/supplier-payments', label: 'Supplier Payments', icon: 'SP' },
        { path: '/purchase-returns', label: 'Purchase Returns', icon: 'PRR' },
      ],
    },
    {
      label: 'Finance',
      items: [
        { path: '/expenses', label: 'Expenses', icon: 'EX' },
        { path: '/finance', label: 'Finance', icon: 'FN' },
        { path: '/reports', label: 'Reports', icon: 'RP' },
        { path: '/alerts', label: 'Alerts', icon: 'AL' },
      ],
    },
    {
      label: 'Admin',
      items: [
        { path: '/users', label: 'Users & Roles', icon: 'UR' },
        { path: '/branches', label: 'Branches', icon: 'BR' },
        { path: '/hardware', label: 'Hardware', icon: 'HW' },
        { path: '/sync', label: 'Sync', icon: 'SY' },
        { path: '/settings', label: 'Settings', icon: 'ST' },
      ],
    },
  ];

  constructor(
    readonly auth: AuthService,
    private readonly router: Router,
  ) {
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.navOpen.set(false));
  }

  @HostListener('document:keydown.escape')
  onEsc(): void {
    if (this.navOpen()) this.navOpen.set(false);
  }

  toggleNav(): void {
    this.navOpen.update((v) => !v);
  }

  closeNav(): void {
    this.navOpen.set(false);
  }

  logout(): void {
    this.auth.logout();
  }
}
