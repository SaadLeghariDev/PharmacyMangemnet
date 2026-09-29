import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { map, Observable } from 'rxjs';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  FefoCandidateDto,
  ProductDto,
  StockBalanceDto,
  WarehouseDto,
} from '../../core/models/api.models';
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
  AppTypeaheadComponent,
  AppTypeaheadItem,
  SnackbarService,
} from '../../shared';

type InventoryTab = 'stock' | 'near-expiry' | 'fefo';

@Component({
  selector: 'app-inventory-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DatePipe,
    AppPageHeaderComponent,
    AppButtonComponent,
    AppInputComponent,
    AppSelectComponent,
    AppTableComponent,
    AppEmptyStateComponent,
    AppLoadingStateComponent,
    AppTypeaheadComponent,
  ],
  templateUrl: './inventory-page.component.html',
  styleUrl: './inventory-page.component.scss',
})
export class InventoryPageComponent implements OnInit {
  tab: InventoryTab = 'stock';

  readonly stockColumns: AppTableColumn[] = [
    { key: 'product', label: 'Product' },
    { key: 'batch', label: 'Batch' },
    { key: 'expiry', label: 'Expiry' },
    { key: 'location', label: 'Location' },
    { key: 'onHand', label: 'On hand' },
    { key: 'available', label: 'Available' },
  ];

  readonly fefoColumns: AppTableColumn[] = [
    { key: 'batch', label: 'Batch' },
    { key: 'expiry', label: 'Expiry' },
    { key: 'location', label: 'Location ID' },
    { key: 'qty', label: 'Available' },
    { key: 'cost', label: 'Cost' },
    { key: 'price', label: 'Sale price' },
  ];

  warehouseFilter: string | number = '';
  daysAhead = 90;
  fefoProductId: number | null = null;
  fefoProductLabel = '';
  fefoWarehouseId: number | null = null;
  fefoQty = 1;

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly stockRows = signal<StockBalanceDto[]>([]);
  readonly nearRows = signal<StockBalanceDto[]>([]);
  readonly fefoRows = signal<FefoCandidateDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);
  readonly warehouses = signal<WarehouseDto[]>([]);

  readonly warehouseOptions = computed<AppSelectOption[]>(() => [
    { value: '', label: 'All warehouses' },
    ...this.warehouses().map((w) => ({ value: w.id, label: `${w.code} — ${w.name}` })),
  ]);

  readonly productSuggestFn = (q: string): Observable<AppTypeaheadItem<ProductDto>[]> =>
    this.api.searchProducts(q, 8).pipe(
      map((items) =>
        items.map((p) => ({
          id: p.id,
          label: p.name,
          detail: p.sku,
          data: p,
        })),
      ),
    );

  onFefoProductPick(item: AppTypeaheadItem<ProductDto>): void {
    this.fefoProductId = Number(item.id);
    this.fefoProductLabel = `${item.label} (${item.detail || item.id})`;
  }

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.api.listWarehouses().subscribe({
      next: (items) => this.warehouses.set(items.filter((w) => w.isActive)),
      error: () => this.warehouses.set([]),
    });
    this.loadStock(1);
  }

  setTab(tab: InventoryTab): void {
    this.tab = tab;
    this.error.set('');
    if (tab === 'stock') this.loadStock(1);
    else if (tab === 'near-expiry') this.loadNearExpiry(1);
    else this.fefoRows.set([]);
  }

  applyFilters(): void {
    if (this.tab === 'stock') this.loadStock(1);
    else if (this.tab === 'near-expiry') this.loadNearExpiry(1);
    else this.runFefo();
  }

  loadStock(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    this.api
      .getStock({
        page,
        pageSize: 20,
        warehouseId: this.warehouseFilter === '' ? null : Number(this.warehouseFilter),
      })
      .subscribe({
        next: (r) => {
          this.stockRows.set(r.items);
          this.totalCount.set(r.totalCount);
          this.hasNext.set(r.hasNext);
          this.loading.set(false);
        },
        error: (err: unknown) => this.fail(err),
      });
  }

  loadNearExpiry(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    this.api
      .getNearExpiry({
        page,
        pageSize: 20,
        daysAhead: this.daysAhead,
        warehouseId: this.warehouseFilter === '' ? null : Number(this.warehouseFilter),
      })
      .subscribe({
        next: (r) => {
          this.nearRows.set(r.items);
          this.totalCount.set(r.totalCount);
          this.hasNext.set(r.hasNext);
          this.loading.set(false);
        },
        error: (err: unknown) => this.fail(err),
      });
  }

  runFefo(): void {
    if (this.fefoProductId == null || this.fefoWarehouseId == null) {
      this.snackbar.error('Select a product and enter warehouse ID for FEFO.');
      return;
    }
    this.loading.set(true);
    this.error.set('');
    this.api.getFefo(this.fefoProductId, this.fefoWarehouseId, this.fefoQty).subscribe({
      next: (rows) => {
        this.fefoRows.set(rows);
        this.loading.set(false);
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  private fail(err: unknown): void {
    this.loading.set(false);
    const message =
      err instanceof Error ? err.message : 'Could not load inventory data.';
    this.error.set(message);
    this.snackbar.error(message);
  }
}
