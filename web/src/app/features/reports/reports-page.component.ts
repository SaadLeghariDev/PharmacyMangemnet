import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  AlertDto,
  LowStockCandidateDto,
  SaleDto,
} from '../../core/models/api.models';
import {
  AppBadgeComponent,
  AppButtonComponent,
  AppEmptyStateComponent,
  AppInputComponent,
  AppLoadingStateComponent,
  AppPageHeaderComponent,
  AppTableColumn,
  AppTableComponent,
  SnackbarService,
} from '../../shared';

@Component({
  selector: 'app-reports-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CurrencyPipe,
    DatePipe,
    AppPageHeaderComponent,
    AppButtonComponent,
    AppInputComponent,
    AppTableComponent,
    AppEmptyStateComponent,
    AppLoadingStateComponent,
    AppBadgeComponent,
  ],
  templateUrl: './reports-page.component.html',
  styleUrl: './reports-page.component.scss',
})
export class ReportsPageComponent implements OnInit {
  readonly salesColumns: AppTableColumn[] = [
    { key: 'invoice', label: 'Invoice' },
    { key: 'date', label: 'Date' },
    { key: 'status', label: 'Status' },
    { key: 'net', label: 'Net amount' },
  ];

  readonly stockColumns: AppTableColumn[] = [
    { key: 'product', label: 'Product' },
    { key: 'branch', label: 'Branch' },
    { key: 'onHand', label: 'On hand' },
    { key: 'reorder', label: 'Reorder point' },
  ];

  fromDate = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().slice(0, 10);
  toDate = new Date().toISOString().slice(0, 10);

  readonly salesLoading = signal(false);
  readonly stockLoading = signal(false);
  readonly error = signal('');
  readonly sales = signal<SaleDto[]>([]);
  readonly salesTotal = signal(0);
  readonly lowStock = signal<LowStockCandidateDto[]>([]);
  readonly alerts = signal<AlertDto[]>([]);

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.refreshAll();
  }

  refreshAll(): void {
    this.loadSales();
    this.loadStockAlerts();
  }

  loadSales(): void {
    this.salesLoading.set(true);
    this.error.set('');
    this.api
      .searchSales({
        page: 1,
        pageSize: 100,
        fromDate: new Date(this.fromDate).toISOString(),
        toDate: `${this.toDate}T23:59:59`,
        status: 'Completed',
      })
      .subscribe({
        next: (r) => {
          this.sales.set(r.items);
          this.salesTotal.set(r.items.reduce((sum, s) => sum + s.netAmount, 0));
          this.salesLoading.set(false);
        },
        error: (err: unknown) => {
          this.salesLoading.set(false);
          this.fail(err);
        },
      });
  }

  loadStockAlerts(): void {
    this.stockLoading.set(true);
    this.api.searchLowStockCandidates({ page: 1, pageSize: 50 }).subscribe({
      next: (r) => {
        this.lowStock.set(r.items);
        this.stockLoading.set(false);
      },
      error: (err: unknown) => {
        this.stockLoading.set(false);
        this.fail(err);
      },
    });
    this.api.searchAlerts({ page: 1, pageSize: 20, status: 'Open' }).subscribe({
      next: (r) => this.alerts.set(r.items),
      error: () => this.alerts.set([]),
    });
  }

  private fail(err: unknown): void {
    const message = err instanceof Error ? err.message : 'Could not load report data.';
    this.error.set(message);
    this.snackbar.error(message);
  }
}
