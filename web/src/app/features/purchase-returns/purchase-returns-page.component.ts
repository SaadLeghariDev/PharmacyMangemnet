import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  BranchDto,
  SupplierDto,
  SupplierReturnDto,
  WarehouseDto,
} from '../../core/models/api.models';
import {
  AppBadgeComponent,
  AppButtonComponent,
  AppConfirmDialogComponent,
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

interface DraftLine {
  productId: number | null;
  batchId: number | null;
  productUnitId: number | null;
  quantity: number | null;
  unitCost: number | null;
}

@Component({
  selector: 'app-purchase-returns-page',
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
    AppConfirmDialogComponent,
    AppBadgeComponent,
  ],
  templateUrl: './purchase-returns-page.component.html',
  styleUrl: './purchase-returns-page.component.scss',
})
export class PurchaseReturnsPageComponent implements OnInit {
  readonly columns: AppTableColumn[] = [
    { key: 'number', label: 'Number' },
    { key: 'date', label: 'Date' },
    { key: 'supplier', label: 'Supplier' },
    { key: 'warehouse', label: 'Warehouse' },
    { key: 'status', label: 'Status' },
    { key: 'total', label: 'Total' },
    { key: 'actions', label: 'Actions' },
  ];

  readonly statusOptions: AppSelectOption[] = [
    { value: '', label: 'All statuses' },
    { value: 'Draft', label: 'Draft' },
    { value: 'Posted', label: 'Posted' },
    { value: 'Cancelled', label: 'Cancelled' },
  ];

  search = '';
  statusFilter: string | number = '';
  branchFilter: string | number = '';
  supplierFilter: string | number = '';

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<SupplierReturnDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);

  readonly branches = signal<BranchDto[]>([]);
  readonly suppliers = signal<SupplierDto[]>([]);
  readonly warehouses = signal<WarehouseDto[]>([]);

  readonly createOpen = signal(false);
  readonly saving = signal(false);
  readonly confirmOpen = signal(false);
  readonly confirmBusy = signal(false);
  confirmTitle = '';
  confirmMessage = '';
  confirmDanger = false;
  private confirmAction: (() => void) | null = null;

  createSupplierId: string | number = '';
  createBranchId: string | number = '';
  createWarehouseId: string | number = '';
  createReason = '';
  createLines: DraftLine[] = [{ productId: null, batchId: null, productUnitId: null, quantity: null, unitCost: null }];
  createErrors: Record<string, string> = {};

  readonly branchOptions = computed<AppSelectOption[]>(() => [
    { value: '', label: 'All branches' },
    ...this.branches().map((b) => ({ value: b.id, label: `${b.code} — ${b.name}` })),
  ]);

  readonly supplierOptions = computed<AppSelectOption[]>(() => [
    { value: '', label: 'All suppliers' },
    ...this.suppliers().map((s) => ({ value: s.id, label: `${s.code} — ${s.name}` })),
  ]);

  readonly createBranchOptions = computed<AppSelectOption[]>(() =>
    this.branches().map((b) => ({ value: b.id, label: `${b.code} — ${b.name}` })),
  );

  readonly createSupplierOptions = computed<AppSelectOption[]>(() =>
    this.suppliers()
      .filter((s) => s.isActive)
      .map((s) => ({ value: s.id, label: `${s.code} — ${s.name}` })),
  );

  readonly createWarehouseOptions = computed<AppSelectOption[]>(() => {
    const branchId = Number(this.createBranchId);
    return this.warehouses()
      .filter((w) => !branchId || w.branchId === branchId)
      .map((w) => ({ value: w.id, label: `${w.code} — ${w.name}` }));
  });

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.bootstrap();
  }

  bootstrap(): void {
    this.loading.set(true);
    forkJoin({
      branches: this.api.listBranches(),
      suppliers: this.api.searchSuppliers(),
      warehouses: this.api.listWarehouses(),
    }).subscribe({
      next: ({ branches, suppliers, warehouses }) => {
        this.branches.set(branches.filter((b) => b.isActive));
        this.suppliers.set(suppliers);
        this.warehouses.set(warehouses.filter((w) => w.isActive));
        if (!this.createBranchId && branches.length) this.createBranchId = branches[0].id;
        if (!this.createSupplierId && suppliers.length) this.createSupplierId = suppliers[0].id;
        this.syncWarehouseDefault();
        this.load(1);
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  syncWarehouseDefault(): void {
    const opts = this.createWarehouseOptions();
    if (opts.length) this.createWarehouseId = opts[0].value;
  }

  load(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    this.api
      .searchSupplierReturns({
        page,
        pageSize: 20,
        search: this.search,
        status: this.statusFilter === '' ? null : String(this.statusFilter),
        branchId: this.branchFilter === '' ? null : Number(this.branchFilter),
        supplierId: this.supplierFilter === '' ? null : Number(this.supplierFilter),
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
    this.branchFilter = '';
    this.supplierFilter = '';
    this.load(1);
  }

  openCreate(): void {
    this.createErrors = {};
    this.createReason = '';
    this.createLines = [
      { productId: null, batchId: null, productUnitId: null, quantity: null, unitCost: null },
    ];
    if (!this.createBranchId && this.branches().length) this.createBranchId = this.branches()[0].id;
    if (!this.createSupplierId && this.suppliers().length)
      this.createSupplierId = this.suppliers()[0].id;
    this.syncWarehouseDefault();
    this.createOpen.set(true);
  }

  closeCreate(): void {
    if (!this.saving()) this.createOpen.set(false);
  }

  addLine(): void {
    this.createLines = [
      ...this.createLines,
      { productId: null, batchId: null, productUnitId: null, quantity: null, unitCost: null },
    ];
  }

  removeLine(index: number): void {
    if (this.createLines.length <= 1) return;
    this.createLines = this.createLines.filter((_, i) => i !== index);
  }

  submitCreate(): void {
    this.createErrors = {};
    if (!this.createSupplierId) this.createErrors['supplier'] = 'Supplier is required.';
    if (!this.createBranchId) this.createErrors['branch'] = 'Branch is required.';
    if (!this.createWarehouseId) this.createErrors['warehouse'] = 'Warehouse is required.';

    const lines = this.createLines.map((l, i) => {
      if (!l.productId) this.createErrors[`line${i}`] = 'Product, batch, unit, qty, and cost are required.';
      else if (!l.batchId || !l.productUnitId || !l.quantity || l.quantity <= 0 || l.unitCost == null || l.unitCost < 0)
        this.createErrors[`line${i}`] = 'Product, batch, unit, qty, and cost are required.';
      return {
        productId: Number(l.productId),
        batchId: Number(l.batchId),
        productUnitId: Number(l.productUnitId),
        quantity: Number(l.quantity),
        unitCost: Number(l.unitCost),
      };
    });

    if (Object.keys(this.createErrors).length) return;

    this.saving.set(true);
    this.api
      .createSupplierReturn({
        supplierId: Number(this.createSupplierId),
        branchId: Number(this.createBranchId),
        warehouseId: Number(this.createWarehouseId),
        reason: this.createReason.trim() || null,
        lines,
      })
      .subscribe({
        next: (ret) => {
          this.saving.set(false);
          this.createOpen.set(false);
          this.snackbar.success(`Draft ${ret.returnNumber} created.`);
          this.load(1);
        },
        error: (err: unknown) => {
          this.saving.set(false);
          const message = err instanceof Error ? err.message : 'Could not create return.';
          this.snackbar.error(message);
        },
      });
  }

  askPost(row: SupplierReturnDto): void {
    this.confirmTitle = 'Post purchase return';
    this.confirmMessage = `Post ${row.returnNumber}? Stock will leave inventory and the supplier ledger will be updated.`;
    this.confirmDanger = false;
    this.confirmAction = () => this.doPost(row.id);
    this.confirmOpen.set(true);
  }

  askCancel(row: SupplierReturnDto): void {
    this.confirmTitle = 'Cancel draft return';
    this.confirmMessage = `Cancel draft ${row.returnNumber}? This cannot be undone.`;
    this.confirmDanger = true;
    this.confirmAction = () => this.doCancel(row.id);
    this.confirmOpen.set(true);
  }

  onConfirm(): void {
    this.confirmAction?.();
  }

  closeConfirm(): void {
    if (!this.confirmBusy()) {
      this.confirmOpen.set(false);
      this.confirmAction = null;
    }
  }

  statusTone(status: string): 'success' | 'warning' | 'danger' | 'neutral' {
    switch (status) {
      case 'Posted':
        return 'success';
      case 'Draft':
        return 'warning';
      case 'Cancelled':
        return 'danger';
      default:
        return 'neutral';
    }
  }

  private doPost(id: number): void {
    this.confirmBusy.set(true);
    this.api.postSupplierReturn(id).subscribe({
      next: (ret) => {
        this.confirmBusy.set(false);
        this.confirmOpen.set(false);
        this.snackbar.success(`${ret.returnNumber} posted.`);
        this.load(this.page());
      },
      error: (err: unknown) => {
        this.confirmBusy.set(false);
        const message = err instanceof Error ? err.message : 'Could not post return.';
        this.snackbar.error(message);
      },
    });
  }

  private doCancel(id: number): void {
    this.confirmBusy.set(true);
    this.api.cancelSupplierReturn(id).subscribe({
      next: (ret) => {
        this.confirmBusy.set(false);
        this.confirmOpen.set(false);
        this.snackbar.success(`${ret.returnNumber} cancelled.`);
        this.load(this.page());
      },
      error: (err: unknown) => {
        this.confirmBusy.set(false);
        const message = err instanceof Error ? err.message : 'Could not cancel return.';
        this.snackbar.error(message);
      },
    });
  }

  private fail(err: unknown): void {
    this.loading.set(false);
    const message =
      err instanceof Error
        ? err.message
        : 'Could not load purchase returns. Check the API connection and try again.';
    this.error.set(message);
    this.snackbar.error(message);
  }
}
