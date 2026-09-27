import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import {
  AppButtonComponent,
  AppEmptyStateComponent,
  AppInputComponent,
  AppLoadingStateComponent,
  AppPageHeaderComponent,
  AppSelectComponent,
  AppSelectOption,
  AppTableColumn,
  AppTableComponent,
  SnackbarService,
} from '../../shared';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';

export type ListModuleKind =
  | 'products'
  | 'inventory'
  | 'purchases'
  | 'suppliers'
  | 'customers'
  | 'sales-returns'
  | 'purchase-returns'
  | 'expenses'
  | 'reports'
  | 'users'
  | 'branches'
  | 'settings'
  | 'dashboard';

interface ListRow {
  id: string | number;
  c1: string;
  c2: string;
  c3: string;
  c4: string;
  c5: string;
  status?: string;
}

@Component({
  selector: 'app-list-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    AppPageHeaderComponent,
    AppButtonComponent,
    AppInputComponent,
    AppSelectComponent,
    AppTableComponent,
    AppEmptyStateComponent,
    AppLoadingStateComponent,
  ],
  templateUrl: './list-page.component.html',
  styleUrl: './list-page.component.scss',
})
export class ListPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(PharmacyApiService);
  private readonly snackbar = inject(SnackbarService);

  readonly title = toSignal(
    this.route.data.pipe(map((d) => (d['title'] as string) || 'Module')),
    { initialValue: 'Module' },
  );
  readonly kind = toSignal(
    this.route.data.pipe(map((d) => (d['kind'] as ListModuleKind) || 'products')),
    { initialValue: 'products' as ListModuleKind },
  );

  search = '';
  statusFilter: string | number = '';
  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<ListRow[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);

  readonly statusOptions: AppSelectOption[] = [
    { value: '', label: 'All statuses' },
    { value: 'active', label: 'Active' },
    { value: 'inactive', label: 'Inactive' },
  ];

  ngOnInit(): void {
    this.route.data.subscribe(() => this.load(1));
  }

  columns(): AppTableColumn[] {
    switch (this.kind()) {
      case 'products':
        return [
          { key: 'c1', label: 'Name' },
          { key: 'c2', label: 'Generic' },
          { key: 'c3', label: 'SKU' },
          { key: 'c4', label: 'Category' },
          { key: 'c5', label: 'Status' },
        ];
      case 'customers':
        return [
          { key: 'c1', label: 'Name' },
          { key: 'c2', label: 'Code' },
          { key: 'c3', label: 'Phone' },
          { key: 'c4', label: 'Balance' },
          { key: 'c5', label: 'Status' },
        ];
      case 'suppliers':
        return [
          { key: 'c1', label: 'Name' },
          { key: 'c2', label: 'Code' },
          { key: 'c3', label: 'Phone' },
          { key: 'c4', label: 'City' },
          { key: 'c5', label: 'Status' },
        ];
      case 'inventory':
        return [
          { key: 'c1', label: 'Product' },
          { key: 'c2', label: 'SKU' },
          { key: 'c3', label: 'Batch' },
          { key: 'c4', label: 'Expiry' },
          { key: 'c5', label: 'Available' },
        ];
      case 'branches':
        return [
          { key: 'c1', label: 'Name' },
          { key: 'c2', label: 'Code' },
          { key: 'c3', label: 'City' },
          { key: 'c4', label: 'Type' },
          { key: 'c5', label: 'Status' },
        ];
      default:
        return [
          { key: 'c1', label: 'Reference' },
          { key: 'c2', label: 'Detail' },
          { key: 'c3', label: 'Date' },
          { key: 'c4', label: 'Amount' },
          { key: 'c5', label: 'Status' },
        ];
    }
  }

  subtitle(): string {
    switch (this.kind()) {
      case 'products':
        return 'Search and manage the medicine catalog.';
      case 'inventory':
        return 'Stock balances by batch and location.';
      case 'customers':
        return 'Customer accounts and credit balances.';
      case 'suppliers':
        return 'Supplier directory for purchasing.';
      case 'branches':
        return 'Organization branches.';
      default:
        return 'List view — full workflow UI in a later slice.';
    }
  }

  load(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');

    const kind = this.kind();
    if (kind === 'products') {
      this.api.searchProducts(this.search.trim(), 50).subscribe({
        next: (items) => {
          const filtered =
            this.statusFilter === 'inactive'
              ? items.filter((p) => !p.isActive)
              : this.statusFilter === 'active'
                ? items.filter((p) => p.isActive)
                : items;
          this.rows.set(
            filtered.map((p) => ({
              id: p.id,
              c1: p.name,
              c2: p.genericName || '—',
              c3: p.sku,
              c4: p.categoryName || '—',
              c5: p.isActive ? 'Active' : 'Inactive',
              status: p.isActive ? 'active' : 'inactive',
            })),
          );
          this.totalCount.set(filtered.length);
          this.hasNext.set(false);
          this.loading.set(false);
        },
        error: (err: unknown) => this.fail(err),
      });
      return;
    }

    if (kind === 'branches') {
      this.api.listBranches().subscribe({
        next: (items) => {
          this.rows.set(
            items.map((b) => ({
              id: b.id,
              c1: b.name,
              c2: b.code,
              c3: b.city || '—',
              c4: b.branchType || '—',
              c5: b.isActive ? 'Active' : 'Inactive',
            })),
          );
          this.totalCount.set(items.length);
          this.hasNext.set(false);
          this.loading.set(false);
        },
        error: (err: unknown) => this.fail(err),
      });
      return;
    }

    if (kind === 'suppliers') {
      // Lightweight stub until dedicated UI — show empty enterprise pattern when API not wired here
      this.rows.set([]);
      this.totalCount.set(0);
      this.hasNext.set(false);
      this.loading.set(false);
      return;
    }

    if (kind === 'customers') {
      this.rows.set([]);
      this.totalCount.set(0);
      this.hasNext.set(false);
      this.loading.set(false);
      return;
    }

    if (kind === 'inventory') {
      this.rows.set([]);
      this.totalCount.set(0);
      this.hasNext.set(false);
      this.loading.set(false);
      return;
    }

    // Remaining modules: empty enterprise shell
    this.rows.set([]);
    this.totalCount.set(0);
    this.hasNext.set(false);
    this.loading.set(false);
  }

  applyFilters(): void {
    this.load(1);
  }

  clearFilters(): void {
    this.search = '';
    this.statusFilter = '';
    this.load(1);
  }

  private fail(err: unknown): void {
    this.loading.set(false);
    const message =
      err instanceof Error
        ? err.message
        : 'Could not load this list. Check the API connection and try again.';
    this.error.set(message);
    this.snackbar.error(message);
  }
}
