import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import { PriceListDto } from '../../core/models/api.models';
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
  selector: 'app-price-lists-page',
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
    AppModalComponent,
    AppBadgeComponent,
  ],
  templateUrl: './price-lists-page.component.html',
  styleUrl: './price-lists-page.component.scss',
})
export class PriceListsPageComponent implements OnInit {
  readonly columns: AppTableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'type', label: 'Type' },
    { key: 'currency', label: 'Currency' },
    { key: 'default', label: 'Default' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' },
  ];

  readonly activeOptions: AppSelectOption[] = [
    { value: '', label: 'All statuses' },
    { value: 'true', label: 'Active' },
    { value: 'false', label: 'Inactive' },
  ];

  search = '';
  activeFilter: string | number = '';

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<PriceListDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);

  readonly modalOpen = signal(false);
  readonly saving = signal(false);
  editingId: number | null = null;

  formName = '';
  formPriceType = '';
  formCurrency = 'PKR';
  formIsDefault = false;
  formIsActive = true;
  formErrors: Record<string, string> = {};

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.load(1);
  }

  load(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    this.api
      .searchPriceLists({
        page,
        pageSize: 20,
        search: this.search,
        isActive: this.activeFilter === '' ? null : this.activeFilter === 'true',
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
    this.activeFilter = '';
    this.load(1);
  }

  openCreate(): void {
    this.editingId = null;
    this.formName = '';
    this.formPriceType = '';
    this.formCurrency = 'PKR';
    this.formIsDefault = false;
    this.formIsActive = true;
    this.formErrors = {};
    this.modalOpen.set(true);
  }

  openEdit(row: PriceListDto): void {
    this.editingId = row.id;
    this.formName = row.name;
    this.formPriceType = row.priceType ?? '';
    this.formCurrency = row.currencyCode || 'PKR';
    this.formIsDefault = row.isDefault;
    this.formIsActive = row.isActive;
    this.formErrors = {};
    this.modalOpen.set(true);
  }

  closeModal(): void {
    if (this.saving()) return;
    this.modalOpen.set(false);
  }

  save(): void {
    this.formErrors = {};
    if (!this.formName.trim()) this.formErrors['name'] = 'Price list name is required.';
    if (!this.formCurrency.trim() || this.formCurrency.trim().length !== 3) {
      this.formErrors['currency'] = 'Currency code must be 3 characters.';
    }
    if (Object.keys(this.formErrors).length) return;

    const payload = {
      name: this.formName.trim(),
      priceType: this.formPriceType.trim() || null,
      currencyCode: this.formCurrency.trim().toUpperCase(),
      isDefault: this.formIsDefault,
      isActive: this.formIsActive,
    };

    this.saving.set(true);
    const req$ =
      this.editingId == null
        ? this.api.createPriceList(payload)
        : this.api.updatePriceList(this.editingId, payload);

    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.modalOpen.set(false);
        this.snackbar.success(this.editingId == null ? 'Price list created.' : 'Price list updated.');
        this.load(this.page());
      },
      error: (err: unknown) => {
        this.saving.set(false);
        this.fail(err);
      },
    });
  }

  private fail(err: unknown): void {
    const message = err instanceof Error ? err.message : 'Request failed';
    this.error.set(message);
    this.loading.set(false);
    this.snackbar.error(message);
  }
}
