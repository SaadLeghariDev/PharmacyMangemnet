import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import { PriceListDto, ProductDto, ProductPriceDto } from '../../core/models/api.models';
import {
  AppBadgeComponent,
  AppButtonComponent,
  AppEmptyStateComponent,
  AppInputComponent,
  AppLoadingStateComponent,
  AppModalComponent,
  AppPageHeaderComponent,
  AppSelectComponent,
  AppSelectOption,
  AppTableColumn,
  AppTableComponent,
  SnackbarService,
} from '../../shared';

@Component({
  selector: 'app-product-prices-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CurrencyPipe,
    DatePipe,
    AppPageHeaderComponent,
    AppButtonComponent,
    AppInputComponent,
    AppSelectComponent,
    AppTableComponent,
    AppEmptyStateComponent,
    AppLoadingStateComponent,
    AppModalComponent,
    AppBadgeComponent,
  ],
  templateUrl: './product-prices-page.component.html',
  styleUrl: './product-prices-page.component.scss',
})
export class ProductPricesPageComponent implements OnInit {
  readonly columns: AppTableColumn[] = [
    { key: 'product', label: 'Product' },
    { key: 'list', label: 'Price list' },
    { key: 'unit', label: 'Unit' },
    { key: 'sale', label: 'Sale' },
    { key: 'mrp', label: 'MRP' },
    { key: 'from', label: 'Effective from' },
    { key: 'to', label: 'Effective to' },
    { key: 'actions', label: '' },
  ];

  search = '';
  priceListFilter: string | number = '';
  activeOnly = true;

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<ProductPriceDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);
  readonly priceLists = signal<PriceListDto[]>([]);
  readonly products = signal<ProductDto[]>([]);

  readonly modalOpen = signal(false);
  readonly saving = signal(false);
  editingId: number | null = null;

  formPriceListId: string | number = '';
  formProductId: string | number = '';
  formProductUnitId: number | null = null;
  formPurchase = 0;
  formSale = 0;
  formMrp = 0;
  formDiscount = 0;
  formFrom = new Date().toISOString().slice(0, 10);
  formTo = '';
  formErrors: Record<string, string> = {};

  readonly priceListOptions = computed<AppSelectOption[]>(() => [
    { value: '', label: 'All price lists' },
    ...this.priceLists().map((p) => ({
      value: p.id,
      label: `${p.name}${p.isDefault ? ' (default)' : ''}`,
    })),
  ]);

  readonly createPriceListOptions = computed<AppSelectOption[]>(() =>
    this.priceLists()
      .filter((p) => p.isActive)
      .map((p) => ({
        value: p.id,
        label: `${p.name}${p.isDefault ? ' (default)' : ''}`,
      })),
  );

  readonly productOptions = computed<AppSelectOption[]>(() =>
    this.products().map((p) => ({
      value: p.id,
      label: `${p.sku} — ${p.name}`,
    })),
  );

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.bootstrap();
  }

  bootstrap(): void {
    this.loading.set(true);
    this.api.searchPriceLists({ pageSize: 100, isActive: true }).subscribe({
      next: (lists) => {
        this.priceLists.set(lists.items);
        this.api.searchProducts('', 50).subscribe({
          next: (products) => {
            this.products.set(products);
            this.load(1);
          },
          error: (err: unknown) => this.fail(err),
        });
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  load(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    this.api
      .searchProductPrices({
        page,
        pageSize: 20,
        search: this.search,
        priceListId: this.priceListFilter === '' ? null : Number(this.priceListFilter),
        activeOnly: this.activeOnly ? true : null,
      })
      .subscribe({
        next: (result) => {
          this.rows.set(result.items);
          this.totalCount.set(result.totalCount);
          this.hasNext.set(result.hasNext);
          this.loading.set(false);
        },
        error: (err: unknown) => this.fail(err),
      });
  }

  applyFilters(): void {
    this.load(1);
  }

  clearFilters(): void {
    this.search = '';
    this.priceListFilter = '';
    this.activeOnly = true;
    this.load(1);
  }

  openCreate(): void {
    this.editingId = null;
    const defaultList = this.priceLists().find((p) => p.isDefault) ?? this.priceLists()[0];
    this.formPriceListId = defaultList?.id ?? '';
    this.formProductId = this.products()[0]?.id ?? '';
    this.formProductUnitId = this.products()[0]?.defaultSaleUnitId ?? null;
    this.formPurchase = 0;
    this.formSale = 0;
    this.formMrp = 0;
    this.formDiscount = 0;
    this.formFrom = new Date().toISOString().slice(0, 10);
    this.formTo = '';
    this.formErrors = {};
    this.modalOpen.set(true);
  }

  onProductChange(): void {
    const product = this.products().find((p) => p.id === Number(this.formProductId));
    this.formProductUnitId = product?.defaultSaleUnitId ?? null;
  }

  openEdit(row: ProductPriceDto): void {
    this.editingId = row.id;
    this.formPriceListId = row.priceListId;
    this.formProductId = row.productId;
    this.formProductUnitId = row.productUnitId;
    this.formPurchase = row.purchasePrice;
    this.formSale = row.salePrice;
    this.formMrp = row.mrp;
    this.formDiscount = row.discountPercent;
    this.formFrom = row.effectiveFrom.slice(0, 10);
    this.formTo = row.effectiveTo ? row.effectiveTo.slice(0, 10) : '';
    this.formErrors = {};
    this.modalOpen.set(true);
  }

  deactivate(row: ProductPriceDto): void {
    const to = new Date().toISOString();
    this.api
      .updateProductPrice(row.id, {
        purchasePrice: row.purchasePrice,
        salePrice: row.salePrice,
        mrp: row.mrp,
        discountPercent: row.discountPercent,
        effectiveFrom: row.effectiveFrom,
        effectiveTo: to,
      })
      .subscribe({
        next: () => {
          this.snackbar.success('Product price deactivated.');
          this.load(this.page());
        },
        error: (err: unknown) => this.fail(err),
      });
  }

  closeModal(): void {
    if (this.saving()) return;
    this.modalOpen.set(false);
  }

  save(): void {
    this.formErrors = {};
    if (!this.formPriceListId) this.formErrors['list'] = 'Price list is required.';
    if (!this.formProductId) this.formErrors['product'] = 'Product is required.';
    if (!this.formProductUnitId || this.formProductUnitId <= 0) {
      this.formErrors['unit'] = 'Product unit is required.';
    }
    if (this.formSale < 0) this.formErrors['sale'] = 'Sale price is required.';
    if (!this.formFrom) this.formErrors['from'] = 'Effective from is required.';
    if (Object.keys(this.formErrors).length) return;

    this.saving.set(true);
    if (this.editingId == null) {
      this.api
        .createProductPrice({
          priceListId: Number(this.formPriceListId),
          productId: Number(this.formProductId),
          productUnitId: Number(this.formProductUnitId),
          purchasePrice: Number(this.formPurchase) || 0,
          salePrice: Number(this.formSale) || 0,
          mrp: Number(this.formMrp) || 0,
          discountPercent: Number(this.formDiscount) || 0,
          effectiveFrom: new Date(this.formFrom).toISOString(),
          effectiveTo: this.formTo ? new Date(this.formTo).toISOString() : null,
        })
        .subscribe({
          next: () => this.afterSave('Product price created.'),
          error: (err: unknown) => {
            this.saving.set(false);
            this.fail(err);
          },
        });
    } else {
      this.api
        .updateProductPrice(this.editingId, {
          purchasePrice: Number(this.formPurchase) || 0,
          salePrice: Number(this.formSale) || 0,
          mrp: Number(this.formMrp) || 0,
          discountPercent: Number(this.formDiscount) || 0,
          effectiveFrom: new Date(this.formFrom).toISOString(),
          effectiveTo: this.formTo ? new Date(this.formTo).toISOString() : null,
        })
        .subscribe({
          next: () => this.afterSave('Product price updated.'),
          error: (err: unknown) => {
            this.saving.set(false);
            this.fail(err);
          },
        });
    }
  }

  private afterSave(message: string): void {
    this.saving.set(false);
    this.modalOpen.set(false);
    this.snackbar.success(message);
    this.load(this.page());
  }

  private fail(err: unknown): void {
    const message = err instanceof Error ? err.message : 'Request failed';
    this.error.set(message);
    this.loading.set(false);
    this.snackbar.error(message);
  }
}
