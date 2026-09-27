import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  ProductDto,
  ProductTaxProfileDto,
  TaxProfileDto,
  TaxRateDto,
} from '../../core/models/api.models';
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
  selector: 'app-tax-profiles-page',
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
    AppModalComponent,
    AppBadgeComponent,
  ],
  templateUrl: './tax-profiles-page.component.html',
  styleUrl: './tax-profiles-page.component.scss',
})
export class TaxProfilesPageComponent implements OnInit {
  readonly columns: AppTableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'type', label: 'Type' },
    { key: 'description', label: 'Description' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' },
  ];

  search = '';
  activeFilter: string | number = '';

  readonly activeOptions: AppSelectOption[] = [
    { value: '', label: 'All statuses' },
    { value: 'true', label: 'Active' },
    { value: 'false', label: 'Inactive' },
  ];

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<TaxProfileDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);
  readonly products = signal<ProductDto[]>([]);

  readonly profileModalOpen = signal(false);
  readonly ratesModalOpen = signal(false);
  readonly assignModalOpen = signal(false);
  readonly saving = signal(false);

  editingId: number | null = null;
  formName = '';
  formTaxType = '';
  formDescription = '';
  formIsActive = true;
  formErrors: Record<string, string> = {};

  ratesProfileId: number | null = null;
  ratesProfileName = '';
  readonly rates = signal<TaxRateDto[]>([]);
  rateEditingId: number | null = null;
  rateValue: number | null = null;
  rateFrom = new Date().toISOString().slice(0, 10);
  rateTo = '';
  rateActive = true;
  rateErrors: Record<string, string> = {};

  assignProductId: string | number = '';
  readonly assigned = signal<ProductTaxProfileDto[]>([]);
  selectedProfileIds = new Set<number>();

  readonly productOptions = computed<AppSelectOption[]>(() =>
    this.products().map((p) => ({ value: p.id, label: `${p.sku} — ${p.name}` })),
  );

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.api.searchProducts('', 50).subscribe({
      next: (products) => this.products.set(products),
      error: () => undefined,
    });
    this.load(1);
  }

  load(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    this.api
      .searchTaxProfiles({
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
    this.formTaxType = '';
    this.formDescription = '';
    this.formIsActive = true;
    this.formErrors = {};
    this.profileModalOpen.set(true);
  }

  openEdit(row: TaxProfileDto): void {
    this.editingId = row.id;
    this.formName = row.name;
    this.formTaxType = row.taxType ?? '';
    this.formDescription = row.description ?? '';
    this.formIsActive = row.isActive;
    this.formErrors = {};
    this.profileModalOpen.set(true);
  }

  closeProfileModal(): void {
    if (this.saving()) return;
    this.profileModalOpen.set(false);
  }

  saveProfile(): void {
    this.formErrors = {};
    if (!this.formName.trim()) this.formErrors['name'] = 'Tax profile name is required.';
    if (Object.keys(this.formErrors).length) return;

    const payload = {
      name: this.formName.trim(),
      taxType: this.formTaxType.trim() || null,
      description: this.formDescription.trim() || null,
      isActive: this.formIsActive,
    };

    this.saving.set(true);
    const req$ =
      this.editingId == null
        ? this.api.createTaxProfile(payload)
        : this.api.updateTaxProfile(this.editingId, payload);

    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.profileModalOpen.set(false);
        this.snackbar.success(this.editingId == null ? 'Tax profile created.' : 'Tax profile updated.');
        this.load(this.page());
      },
      error: (err: unknown) => {
        this.saving.set(false);
        this.fail(err);
      },
    });
  }

  openRates(row: TaxProfileDto): void {
    this.ratesProfileId = row.id;
    this.ratesProfileName = row.name;
    this.resetRateForm();
    this.ratesModalOpen.set(true);
    this.reloadRates();
  }

  closeRates(): void {
    if (this.saving()) return;
    this.ratesModalOpen.set(false);
  }

  reloadRates(): void {
    if (this.ratesProfileId == null) return;
    this.api.listTaxRates(this.ratesProfileId).subscribe({
      next: (rates) => this.rates.set(rates),
      error: (err: unknown) => this.fail(err),
    });
  }

  editRate(rate: TaxRateDto): void {
    this.rateEditingId = rate.id;
    this.rateValue = rate.rate;
    this.rateFrom = String(rate.effectiveFrom).slice(0, 10);
    this.rateTo = rate.effectiveTo ? String(rate.effectiveTo).slice(0, 10) : '';
    this.rateActive = rate.isActive;
    this.rateErrors = {};
  }

  resetRateForm(): void {
    this.rateEditingId = null;
    this.rateValue = null;
    this.rateFrom = new Date().toISOString().slice(0, 10);
    this.rateTo = '';
    this.rateActive = true;
    this.rateErrors = {};
  }

  saveRate(): void {
    if (this.ratesProfileId == null) return;
    this.rateErrors = {};
    if (this.rateValue == null || this.rateValue < 0) {
      this.rateErrors['rate'] = 'Tax rate must be zero or greater.';
    }
    if (!this.rateFrom) this.rateErrors['from'] = 'Effective from is required.';
    if (Object.keys(this.rateErrors).length) return;

    const payload = {
      rate: Number(this.rateValue),
      effectiveFrom: this.rateFrom,
      effectiveTo: this.rateTo || null,
      isActive: this.rateActive,
    };

    this.saving.set(true);
    const req$ =
      this.rateEditingId == null
        ? this.api.addTaxRate(this.ratesProfileId, payload)
        : this.api.updateTaxRate(this.ratesProfileId, this.rateEditingId, payload);

    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.snackbar.success(this.rateEditingId == null ? 'Tax rate added.' : 'Tax rate updated.');
        this.resetRateForm();
        this.reloadRates();
      },
      error: (err: unknown) => {
        this.saving.set(false);
        this.fail(err);
      },
    });
  }

  openAssign(): void {
    this.assignProductId = this.products()[0]?.id ?? '';
    this.assignModalOpen.set(true);
    this.loadAssigned();
  }

  closeAssign(): void {
    if (this.saving()) return;
    this.assignModalOpen.set(false);
  }

  loadAssigned(): void {
    if (!this.assignProductId) {
      this.assigned.set([]);
      this.selectedProfileIds = new Set();
      return;
    }
    this.api.getProductTaxProfiles(Number(this.assignProductId)).subscribe({
      next: (items) => {
        this.assigned.set(items);
        this.selectedProfileIds = new Set(items.map((i) => i.taxProfileId));
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  toggleProfile(id: number, checked: boolean): void {
    if (checked) this.selectedProfileIds.add(id);
    else this.selectedProfileIds.delete(id);
  }

  isSelected(id: number): boolean {
    return this.selectedProfileIds.has(id);
  }

  saveAssign(): void {
    if (!this.assignProductId) return;
    this.saving.set(true);
    this.api
      .replaceProductTaxProfiles(Number(this.assignProductId), {
        taxProfileIds: [...this.selectedProfileIds],
      })
      .subscribe({
        next: (items) => {
          this.saving.set(false);
          this.assigned.set(items);
          this.snackbar.success('Product tax profiles updated.');
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
