import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  BranchDto,
  LowStockCandidateDto,
  ProductDto,
  ReorderRuleDto,
  SupplierDto,
  WarehouseDto,
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
  selector: 'app-reorder-rules-page',
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
  templateUrl: './reorder-rules-page.component.html',
  styleUrl: './reorder-rules-page.component.scss',
})
export class ReorderRulesPageComponent implements OnInit {
  readonly columns: AppTableColumn[] = [
    { key: 'product', label: 'Product' },
    { key: 'warehouse', label: 'Warehouse' },
    { key: 'min', label: 'Min' },
    { key: 'reorder', label: 'Reorder pt' },
    { key: 'qty', label: 'Reorder qty' },
    { key: 'supplier', label: 'Supplier' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' },
  ];

  readonly lowStockColumns: AppTableColumn[] = [
    { key: 'product', label: 'Product' },
    { key: 'warehouse', label: 'Warehouse' },
    { key: 'available', label: 'Available' },
    { key: 'point', label: 'Reorder pt' },
    { key: 'shortage', label: 'Shortage' },
    { key: 'suggest', label: 'Suggest qty' },
  ];

  readonly activeOptions: AppSelectOption[] = [
    { value: '', label: 'All statuses' },
    { value: 'true', label: 'Active' },
    { value: 'false', label: 'Inactive' },
  ];

  search = '';
  activeFilter: string | number = '';
  branchFilter: string | number = '';
  tab: 'rules' | 'low-stock' = 'rules';

  branchOptions: AppSelectOption[] = [{ value: '', label: 'All branches' }];
  warehouseOptions: AppSelectOption[] = [{ value: '', label: 'Select warehouse' }];
  productOptions: AppSelectOption[] = [{ value: '', label: 'Select product' }];
  supplierOptions: AppSelectOption[] = [{ value: '', label: 'Select supplier' }];

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<ReorderRuleDto[]>([]);
  readonly lowStockRows = signal<LowStockCandidateDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);

  readonly modalOpen = signal(false);
  readonly saving = signal(false);
  editingId: number | null = null;

  formBranchId: string | number = '';
  formWarehouseId: string | number = '';
  formProductId: string | number = '';
  formSupplierId: string | number = '';
  formMinimum = 0;
  formMaximum = 100;
  formReorderPoint = 20;
  formReorderQty = 50;
  formIsActive = true;
  formErrors: Record<string, string> = {};

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    forkJoin({
      branches: this.api.listBranches(),
      products: this.api.searchProducts('', 50),
      suppliers: this.api.searchSuppliers('', 100),
    }).subscribe({
      next: ({ branches, products, suppliers }) => {
        this.branchOptions = [
          { value: '', label: 'All branches' },
          ...branches.map((b: BranchDto) => ({ value: b.id, label: `${b.code} — ${b.name}` })),
        ];
        this.productOptions = [
          { value: '', label: 'Select product' },
          ...products.map((p: ProductDto) => ({ value: p.id, label: `${p.sku} — ${p.name}` })),
        ];
        this.supplierOptions = [
          { value: '', label: 'Select supplier' },
          ...suppliers.map((s: SupplierDto) => ({ value: s.id, label: `${s.code} — ${s.name}` })),
        ];
      },
      error: (err: unknown) => this.fail(err),
    });
    this.load(1);
  }

  setTab(tab: 'rules' | 'low-stock'): void {
    this.tab = tab;
    this.load(1);
  }

  load(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    const branchId = this.branchFilter === '' ? null : Number(this.branchFilter);

    if (this.tab === 'low-stock') {
      this.api
        .searchLowStockCandidates({ page, pageSize: 20, branchId })
        .subscribe({
          next: (result) => {
            this.lowStockRows.set(result.items);
            this.totalCount.set(result.totalCount);
            this.hasNext.set(result.hasNext);
            this.loading.set(false);
          },
          error: (err: unknown) => this.fail(err),
        });
      return;
    }

    this.api
      .searchReorderRules({
        page,
        pageSize: 20,
        search: this.search,
        branchId,
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
    this.branchFilter = '';
    this.load(1);
  }

  openCreate(): void {
    this.editingId = null;
    this.formBranchId = '';
    this.formWarehouseId = '';
    this.formProductId = '';
    this.formSupplierId = '';
    this.formMinimum = 0;
    this.formMaximum = 100;
    this.formReorderPoint = 20;
    this.formReorderQty = 50;
    this.formIsActive = true;
    this.formErrors = {};
    this.warehouseOptions = [{ value: '', label: 'Select warehouse' }];
    this.modalOpen.set(true);
  }

  openEdit(row: ReorderRuleDto): void {
    this.editingId = row.id;
    this.formBranchId = row.branchId;
    this.formWarehouseId = row.warehouseId;
    this.formProductId = row.productId;
    this.formSupplierId = row.preferredSupplierId;
    this.formMinimum = row.minimumStock;
    this.formMaximum = row.maximumStock;
    this.formReorderPoint = row.reorderPoint;
    this.formReorderQty = row.reorderQuantity;
    this.formIsActive = row.isActive;
    this.formErrors = {};
    this.onBranchChange(row.branchId);
    this.modalOpen.set(true);
  }

  onBranchChange(branchId: string | number): void {
    const id = branchId === '' ? null : Number(branchId);
    if (id == null) {
      this.warehouseOptions = [{ value: '', label: 'Select warehouse' }];
      this.formWarehouseId = '';
      return;
    }
    this.api.listWarehouses(id).subscribe({
      next: (warehouses: WarehouseDto[]) => {
        this.warehouseOptions = [
          { value: '', label: 'Select warehouse' },
          ...warehouses.map((w) => ({ value: w.id, label: `${w.code} — ${w.name}` })),
        ];
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
    if (this.editingId == null) {
      if (this.formBranchId === '') this.formErrors['branch'] = 'Branch is required.';
      if (this.formWarehouseId === '') this.formErrors['warehouse'] = 'Warehouse is required.';
      if (this.formProductId === '') this.formErrors['product'] = 'Product is required.';
    }
    if (this.formSupplierId === '') this.formErrors['supplier'] = 'Preferred supplier is required.';
    if (this.formMaximum < this.formMinimum) {
      this.formErrors['maximum'] = 'Maximum must be ≥ minimum.';
    }
    if (this.formReorderQty <= 0) this.formErrors['qty'] = 'Reorder quantity must be greater than 0.';
    if (Object.keys(this.formErrors).length) return;

    this.saving.set(true);
    const req$ =
      this.editingId == null
        ? this.api.createReorderRule({
            branchId: Number(this.formBranchId),
            warehouseId: Number(this.formWarehouseId),
            productId: Number(this.formProductId),
            minimumStock: Number(this.formMinimum),
            maximumStock: Number(this.formMaximum),
            reorderPoint: Number(this.formReorderPoint),
            reorderQuantity: Number(this.formReorderQty),
            preferredSupplierId: Number(this.formSupplierId),
            isActive: this.formIsActive,
          })
        : this.api.updateReorderRule(this.editingId, {
            minimumStock: Number(this.formMinimum),
            maximumStock: Number(this.formMaximum),
            reorderPoint: Number(this.formReorderPoint),
            reorderQuantity: Number(this.formReorderQty),
            preferredSupplierId: Number(this.formSupplierId),
            isActive: this.formIsActive,
          });

    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.modalOpen.set(false);
        this.snackbar.success(this.editingId == null ? 'Reorder rule created.' : 'Reorder rule updated.');
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
