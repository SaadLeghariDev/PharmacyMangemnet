import {
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule, CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { NgApexchartsModule } from 'ng-apexcharts';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexFill,
  ApexLegend,
  ApexNonAxisChartSeries,
  ApexPlotOptions,
  ApexStroke,
  ApexTooltip,
  ApexXAxis,
  ApexYAxis,
} from 'ng-apexcharts';
import { filter, fromEvent, interval } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  BranchDto,
  DashboardRiskItemDto,
  DashboardSummaryDto,
} from '../../core/models/api.models';
import {
  AppButtonComponent,
  AppEmptyStateComponent,
  AppLoadingStateComponent,
  AppPageHeaderComponent,
  AppSelectComponent,
  AppSelectOption,
  AppTableColumn,
  AppTableComponent,
} from '../../shared';

type PeriodPreset = 'today' | '7d' | '30d';

export type SalesChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  yaxis: ApexYAxis;
  dataLabels: ApexDataLabels;
  stroke: ApexStroke;
  fill: ApexFill;
  tooltip: ApexTooltip;
  colors: string[];
};

export type PaymentChartOptions = {
  series: ApexNonAxisChartSeries;
  chart: ApexChart;
  labels: string[];
  legend: ApexLegend;
  dataLabels: ApexDataLabels;
  plotOptions: ApexPlotOptions;
  colors: string[];
  tooltip: ApexTooltip;
};

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    CurrencyPipe,
    DatePipe,
    DecimalPipe,
    NgApexchartsModule,
    AppPageHeaderComponent,
    AppButtonComponent,
    AppSelectComponent,
    AppTableComponent,
    AppEmptyStateComponent,
    AppLoadingStateComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly api = inject(PharmacyApiService);
  private readonly auth = inject(AuthService);

  readonly saleColumns: AppTableColumn[] = [
    { key: 'invoice', label: 'Invoice' },
    { key: 'date', label: 'Time' },
    { key: 'amount', label: 'Net' },
  ];
  readonly topProductColumns: AppTableColumn[] = [
    { key: 'name', label: 'Medicine' },
    { key: 'qty', label: 'Qty' },
    { key: 'revenue', label: 'Revenue' },
  ];

  period: PeriodPreset = 'today';
  branchId: number | string = '';

  readonly loading = signal(true);
  readonly error = signal('');
  readonly summary = signal<DashboardSummaryDto | null>(null);
  readonly branches = signal<BranchDto[]>([]);
  readonly branchOptions = signal<AppSelectOption[]>([{ value: '', label: 'All branches' }]);

  readonly canViewSales = computed(() => this.hasPermission('POS.SALE'));
  readonly canViewInventory = computed(() => this.hasPermission('INV.VIEW'));
  readonly canViewAlerts = computed(() => this.hasPermission('ALERT.VIEW'));

  readonly salesChart = signal<SalesChartOptions | null>(null);
  readonly paymentChart = signal<PaymentChartOptions | null>(null);

  readonly kpis = computed(() => this.summary()?.kpis ?? null);
  readonly salesDeltaPct = computed(() => {
    const k = this.kpis();
    if (!k) return null;
    return this.pctChange(k.salesTotal, k.previousPeriodSalesTotal);
  });
  readonly billsDeltaPct = computed(() => {
    const k = this.kpis();
    if (!k) return null;
    return this.pctChange(k.billsCount, k.previousPeriodBillsCount);
  });

  readonly lowStockRisk = computed(() =>
    (this.summary()?.riskItems ?? []).filter((r) => r.kind === 'LowStock'),
  );
  readonly nearExpiryRisk = computed(() =>
    (this.summary()?.riskItems ?? []).filter((r) => r.kind === 'NearExpiry'),
  );

  ngOnInit(): void {
    this.api.listBranches().subscribe({
      next: (items) => {
        this.branches.set(items);
        this.branchOptions.set([
          { value: '', label: 'All branches' },
          ...items.map((b) => ({ value: b.id, label: `${b.code} · ${b.name}` })),
        ]);
      },
      error: () => undefined,
    });

    this.load(true);

    interval(45_000)
      .pipe(
        filter(() => document.visibilityState === 'visible'),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.load(false));

    fromEvent(document, 'visibilitychange')
      .pipe(
        filter(() => document.visibilityState === 'visible'),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.load(false));
  }

  setPeriod(preset: PeriodPreset): void {
    if (this.period === preset) return;
    this.period = preset;
    this.load(true);
  }

  onBranchChange(): void {
    this.load(true);
  }

  load(showSpinner = true): void {
    if (!this.canViewSales() && !this.canViewInventory() && !this.canViewAlerts()) {
      this.loading.set(false);
      this.error.set('You do not have permission to view dashboard data.');
      return;
    }

    if (showSpinner) this.loading.set(true);
    this.error.set('');
    const { from, to } = this.rangeForPeriod(this.period);
    const branch =
      this.branchId === '' || this.branchId == null ? null : Number(this.branchId);

    this.api
      .getDashboardSummary({
        from: from.toISOString(),
        to: to.toISOString(),
        branchId: Number.isFinite(branch as number) ? branch : null,
      })
      .subscribe({
        next: (data) => {
          this.summary.set(data);
          this.updateCharts(data);
          this.loading.set(false);
        },
        error: (err: unknown) => {
          this.loading.set(false);
          this.error.set(err instanceof Error ? err.message : 'Could not load dashboard.');
        },
      });
  }

  deltaLabel(pct: number | null): string {
    if (pct == null || Number.isNaN(pct)) return 'vs prior period';
    if (!Number.isFinite(pct)) return 'new';
    const sign = pct > 0 ? '+' : '';
    return `${sign}${pct.toFixed(1)}% vs prior`;
  }

  deltaTone(pct: number | null): 'up' | 'down' | 'flat' {
    if (pct == null || !Number.isFinite(pct) || pct === 0) return 'flat';
    return pct > 0 ? 'up' : 'down';
  }

  riskLink(item: DashboardRiskItemDto): string {
    return item.kind === 'NearExpiry' ? '/alerts' : '/inventory';
  }

  private hasPermission(code: string): boolean {
    const perms = this.auth.user()?.permissions ?? [];
    return perms.includes(code);
  }

  private pctChange(current: number, previous: number): number | null {
    if (previous === 0) return current === 0 ? 0 : Number.POSITIVE_INFINITY;
    return ((current - previous) / Math.abs(previous)) * 100;
  }

  private rangeForPeriod(preset: PeriodPreset): { from: Date; to: Date } {
    const to = new Date();
    const from = new Date(to);
    if (preset === 'today') {
      from.setHours(0, 0, 0, 0);
    } else if (preset === '7d') {
      from.setDate(from.getDate() - 6);
      from.setHours(0, 0, 0, 0);
    } else {
      from.setDate(from.getDate() - 29);
      from.setHours(0, 0, 0, 0);
    }
    return { from, to };
  }

  private updateCharts(data: DashboardSummaryDto): void {
    const daySeries =
      data.salesByDay.length > 0
        ? data.salesByDay
        : data.salesByHour.length > 0
          ? data.salesByHour
          : [];

    this.salesChart.set({
      series: [{ name: 'Sales', data: daySeries.map((p) => p.value) }],
      chart: {
        type: 'area',
        height: 280,
        toolbar: { show: false },
        fontFamily: 'inherit',
      },
      xaxis: {
        categories: daySeries.map((p) => p.label),
        labels: { style: { colors: '#64748b' } },
      },
      yaxis: {
        labels: {
          style: { colors: '#64748b' },
          formatter: (v) => this.compactMoney(v),
        },
      },
      dataLabels: { enabled: false },
      stroke: { curve: 'smooth', width: 2 },
      fill: {
        type: 'gradient',
        gradient: {
          shadeIntensity: 1,
          opacityFrom: 0.35,
          opacityTo: 0.05,
          stops: [0, 90, 100],
        },
      },
      tooltip: {
        y: { formatter: (v) => `PKR ${Number(v).toLocaleString()}` },
      },
      colors: ['#0f766e'],
    });

    const payments = data.paymentBreakdown;
    this.paymentChart.set({
      series: payments.length ? payments.map((p) => p.amount) : [0],
      chart: {
        type: 'donut',
        height: 280,
        fontFamily: 'inherit',
      },
      labels: payments.length ? payments.map((p) => p.methodName) : ['No payments'],
      legend: { position: 'bottom' },
      dataLabels: { enabled: payments.length > 0 },
      plotOptions: {
        pie: {
          donut: { size: '65%' },
        },
      },
      colors: ['#0f766e', '#2563eb', '#d97706', '#16a34a', '#64748b'],
      tooltip: {
        y: { formatter: (v) => `PKR ${Number(v).toLocaleString()}` },
      },
    });
  }

  private compactMoney(v: number): string {
    if (Math.abs(v) >= 1_000_000) return `${(v / 1_000_000).toFixed(1)}M`;
    if (Math.abs(v) >= 1_000) return `${(v / 1_000).toFixed(1)}k`;
    return `${Math.round(v)}`;
  }
}
