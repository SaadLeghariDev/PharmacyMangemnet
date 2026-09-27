import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import { SaleDto } from '../../core/models/api.models';
import {
  AppButtonComponent,
  AppEmptyStateComponent,
  AppLoadingStateComponent,
  AppPageHeaderComponent,
  AppTableColumn,
  AppTableComponent,
} from '../../shared';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    CurrencyPipe,
    DatePipe,
    AppPageHeaderComponent,
    AppButtonComponent,
    AppTableComponent,
    AppEmptyStateComponent,
    AppLoadingStateComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  readonly saleColumns: AppTableColumn[] = [
    { key: 'invoice', label: 'Invoice' },
    { key: 'date', label: 'Time' },
    { key: 'amount', label: 'Net' },
  ];

  readonly loading = signal(true);
  readonly error = signal('');
  readonly todaySalesTotal = signal(0);
  readonly lowStockCount = signal(0);
  readonly nearExpiryCount = signal(0);
  readonly recentSales = signal<SaleDto[]>([]);

  constructor(private readonly api: PharmacyApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    const today = new Date().toISOString().slice(0, 10);
    const from = new Date(today).toISOString();

    forkJoin({
      todaySales: this.api.searchSales({
        page: 1,
        pageSize: 500,
        fromDate: from,
        toDate: `${today}T23:59:59`,
        status: 'Completed',
      }),
      recent: this.api.searchSales({ page: 1, pageSize: 10, status: 'Completed' }),
      lowStock: this.api.searchLowStockCandidates({ page: 1, pageSize: 1 }),
      nearExpiry: this.api.getNearExpiry({ page: 1, pageSize: 1, daysAhead: 90 }),
    }).subscribe({
      next: ({ todaySales, recent, lowStock, nearExpiry }) => {
        this.todaySalesTotal.set(todaySales.items.reduce((s, r) => s + r.netAmount, 0));
        this.recentSales.set(recent.items);
        this.lowStockCount.set(lowStock.totalCount);
        this.nearExpiryCount.set(nearExpiry.totalCount);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.loading.set(false);
        this.error.set(err instanceof Error ? err.message : 'Could not load dashboard.');
      },
    });
  }
}
