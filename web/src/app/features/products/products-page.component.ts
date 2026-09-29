import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  CreateProductRequest,
  ProductDto,
  UpdateProductRequest,
} from '../../core/models/api.models';
import { map, Observable } from 'rxjs';
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
  AppTypeaheadComponent,
  AppTypeaheadItem,
  SnackbarService,
} from '../../shared';

@Component({
  selector: 'app-products-page',
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
    AppTypeaheadComponent,
  ],
  templateUrl: './products-page.component.html',
  styleUrl: './products-page.component.scss',
})
export class ProductsPageComponent implements OnInit {
  readonly columns: AppTableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'generic', label: 'Generic' },
    { key: 'sku', label: 'SKU' },
    { key: 'category', label: 'Category' },
    { key: 'form', label: 'Form' },
    { key: 'status', label: 'Status' },
  ];

  readonly statusOptions: AppSelectOption[] = [
    { value: '', label: 'All statuses' },
    { value: 'active', label: 'Active only' },
    { value: 'inactive', label: 'Inactive only' },
  ];

  readonly activeEditOptions: AppSelectOption[] = [
    { value: 1, label: 'Active' },
    { value: 0, label: 'Inactive' },
  ];

  search = '';
  statusFilter: string | number = '';

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<ProductDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);
  readonly optionProducts = signal<ProductDto[]>([]);

  readonly modalOpen = signal(false);
  readonly saving = signal(false);
  editingId: number | null = null;
  formErrors: Record<string, string> = {};

  formSku = '';
  formName = '';
  formGenericName = '';
  formCategoryId: string | number = '';
  formManufacturerId: string | number = '';
  formBrandId: string | number = '';
  formTherapeuticClassId: string | number = '';
  formForm = '';
  formStrength = '';
  formStrengthUnit = '';
  formPackDescription = '';
  formIsActive: string | number = 1;
  formPrescriptionRequired = false;
  formIsControlled = false;
  formIsTemperatureSensitive = false;
  formIsRefrigerated = false;
  formIsReturnable = true;
  formIsSaleable = true;

  readonly categoryOptions = computed(() => this.distinctFkOptions('categoryId', 'categoryName'));
  readonly manufacturerOptions = computed(() =>
    this.distinctFkOptions('manufacturerId', 'manufacturerName'),
  );
  readonly brandOptions = computed(() => this.distinctFkOptions('brandId', 'brandName'));
  readonly therapeuticOptions = computed(() =>
    this.distinctFkOptions('therapeuticClassId', 'therapeuticClassName'),
  );

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.api.searchProductsPaged({ page: 1, pageSize: 100 }).subscribe({
      next: (r) => this.optionProducts.set(r.items),
      error: () => this.optionProducts.set([]),
    });
    this.load(1);
  }

  load(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    let isActive: boolean | null = null;
    if (this.statusFilter === 'active') isActive = true;
    if (this.statusFilter === 'inactive') isActive = false;

    this.api
      .searchProductsPaged({
        page,
        pageSize: 20,
        search: this.search,
        isActive,
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
    this.statusFilter = '';
    this.load(1);
  }

  readonly productSuggestFn = (q: string): Observable<AppTypeaheadItem<ProductDto>[]> =>
    this.api.searchProducts(q, 8).pipe(
      map((items) =>
        items.map((p) => ({
          id: p.id,
          label: p.name,
          detail: [p.sku, p.genericName].filter(Boolean).join(' · '),
          data: p,
        })),
      ),
    );

  onProductSuggest(item: AppTypeaheadItem<ProductDto>): void {
    this.search = item.data?.sku || item.label;
    this.applyFilters();
  }

  onSearchSubmit(q: string): void {
    this.search = q;
    this.applyFilters();
  }

  openCreate(): void {
    this.editingId = null;
    this.resetForm();
    this.formErrors = {};
    this.modalOpen.set(true);
  }

  openEdit(row: ProductDto): void {
    this.editingId = row.id;
    this.formErrors = {};
    this.formSku = row.sku;
    this.formName = row.name;
    this.formGenericName = row.genericName || '';
    this.formCategoryId = row.categoryId;
    this.formManufacturerId = row.manufacturerId;
    this.formBrandId = row.brandId;
    this.formTherapeuticClassId = row.therapeuticClassId;
    this.formForm = row.form || '';
    this.formStrength = row.strength || '';
    this.formStrengthUnit = row.strengthUnit || '';
    this.formPackDescription = row.packDescription || '';
    this.formIsActive = row.isActive ? 1 : 0;
    this.formPrescriptionRequired = row.prescriptionRequired;
    this.formIsControlled = row.isControlled;
    this.formIsTemperatureSensitive = row.isTemperatureSensitive;
    this.formIsRefrigerated = row.isRefrigerated;
    this.formIsReturnable = row.isReturnable;
    this.formIsSaleable = row.isSaleable;
    this.modalOpen.set(true);
  }

  closeModal(): void {
    if (!this.saving()) this.modalOpen.set(false);
  }

  submitForm(): void {
    this.formErrors = {};
    if (this.editingId == null && !this.formSku.trim()) this.formErrors['sku'] = 'SKU is required.';
    if (!this.formName.trim()) this.formErrors['name'] = 'Name is required.';
    if (!this.formCategoryId) this.formErrors['categoryId'] = 'Category is required.';
    if (!this.formManufacturerId) this.formErrors['manufacturerId'] = 'Manufacturer is required.';
    if (!this.formBrandId) this.formErrors['brandId'] = 'Brand is required.';
    if (!this.formTherapeuticClassId)
      this.formErrors['therapeuticClassId'] = 'Therapeutic class is required.';
    if (Object.keys(this.formErrors).length) return;

    this.saving.set(true);
    if (this.editingId == null) {
      const body: CreateProductRequest = {
        sku: this.formSku.trim(),
        name: this.formName.trim(),
        genericName: this.formGenericName.trim() || null,
        categoryId: Number(this.formCategoryId),
        manufacturerId: Number(this.formManufacturerId),
        brandId: Number(this.formBrandId),
        therapeuticClassId: Number(this.formTherapeuticClassId),
        form: this.formForm.trim() || null,
        strength: this.formStrength.trim() || null,
        strengthUnit: this.formStrengthUnit.trim() || null,
        packDescription: this.formPackDescription.trim() || null,
        prescriptionRequired: this.formPrescriptionRequired,
        isControlled: this.formIsControlled,
        isTemperatureSensitive: this.formIsTemperatureSensitive,
        isRefrigerated: this.formIsRefrigerated,
        isReturnable: this.formIsReturnable,
        isSaleable: this.formIsSaleable,
      };
      this.api.createProduct(body).subscribe({
        next: () => {
          this.saving.set(false);
          this.modalOpen.set(false);
          this.snackbar.success('Product created.');
          this.load(1);
        },
        error: (err: unknown) => this.saveFail(err),
      });
    } else {
      const body: UpdateProductRequest = {
        name: this.formName.trim(),
        genericName: this.formGenericName.trim() || null,
        categoryId: Number(this.formCategoryId),
        manufacturerId: Number(this.formManufacturerId),
        brandId: Number(this.formBrandId),
        therapeuticClassId: Number(this.formTherapeuticClassId),
        form: this.formForm.trim() || null,
        strength: this.formStrength.trim() || null,
        strengthUnit: this.formStrengthUnit.trim() || null,
        packDescription: this.formPackDescription.trim() || null,
        prescriptionRequired: this.formPrescriptionRequired,
        isControlled: this.formIsControlled,
        isTemperatureSensitive: this.formIsTemperatureSensitive,
        isRefrigerated: this.formIsRefrigerated,
        isReturnable: this.formIsReturnable,
        isSaleable: this.formIsSaleable,
        isActive: Number(this.formIsActive) === 1,
      };
      this.api.updateProduct(this.editingId, body).subscribe({
        next: () => {
          this.saving.set(false);
          this.modalOpen.set(false);
          this.snackbar.success('Product updated.');
          this.load(this.page());
        },
        error: (err: unknown) => this.saveFail(err),
      });
    }
  }

  deactivate(row: ProductDto): void {
    if (!confirm(`Deactivate ${row.name}?`)) return;
    this.api.deactivateProduct(row.id).subscribe({
      next: () => {
        this.snackbar.success('Product deactivated.');
        this.load(this.page());
      },
      error: (err: unknown) => {
        const message = err instanceof Error ? err.message : 'Could not deactivate product.';
        this.snackbar.error(message);
      },
    });
  }

  private distinctFkOptions(
    idKey: keyof ProductDto,
    nameKey: keyof ProductDto,
  ): AppSelectOption[] {
    const map = new Map<number, string>();
    for (const p of [...this.optionProducts(), ...this.rows()]) {
      const id = p[idKey] as number;
      const name = p[nameKey] as string | null | undefined;
      if (id) map.set(id, name?.trim() || `#${id}`);
    }
    return [...map.entries()]
      .sort((a, b) => a[1].localeCompare(b[1]))
      .map(([value, label]) => ({ value, label }));
  }

  private resetForm(): void {
    this.formSku = '';
    this.formName = '';
    this.formGenericName = '';
    this.formCategoryId = this.categoryOptions()[0]?.value ?? '';
    this.formManufacturerId = this.manufacturerOptions()[0]?.value ?? '';
    this.formBrandId = this.brandOptions()[0]?.value ?? '';
    this.formTherapeuticClassId = this.therapeuticOptions()[0]?.value ?? '';
    this.formForm = '';
    this.formStrength = '';
    this.formStrengthUnit = '';
    this.formPackDescription = '';
    this.formIsActive = 1;
    this.formPrescriptionRequired = false;
    this.formIsControlled = false;
    this.formIsTemperatureSensitive = false;
    this.formIsRefrigerated = false;
    this.formIsReturnable = true;
    this.formIsSaleable = true;
  }

  private fail(err: unknown): void {
    this.loading.set(false);
    const message =
      err instanceof Error ? err.message : 'Could not load products. Check the API connection.';
    this.error.set(message);
    this.snackbar.error(message);
  }

  private saveFail(err: unknown): void {
    this.saving.set(false);
    const message = err instanceof Error ? err.message : 'Could not save product.';
    this.snackbar.error(message);
  }
}