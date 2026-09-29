import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, map, Observable } from 'rxjs';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  BranchDto,
  CreateGoodsReceiptRequest,
  CreatePurchaseOrderRequest,
  GoodsReceiptDto,
  ProductDto,
  PurchaseOrderDto,
  SupplierDto,
  WarehouseDto,
  WarehouseLocationDto,
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
  AppTypeaheadComponent,
  AppTypeaheadItem,
  SnackbarService,
} from '../../shared';
import { AppBadgeTone } from '../../shared/components/badge/app-badge.component';

type PurchasesTab = 'po' | 'grn';

@Component({
  selector: 'app-purchases-page',
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
    AppTypeaheadComponent,
  ],
  templateUrl: './purchases-page.component.html',
  styleUrl: './purchases-page.component.scss',
})
export class PurchasesPageComponent implements OnInit {
  tab: PurchasesTab = 'po';

  readonly poColumns: AppTableColumn[] = [
    { key: 'number', label: 'PO #' },
    { key: 'date', label: 'Date' },
    { key: 'supplier', label: 'Supplier' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: 'Actions' },
  ];

  readonly grnColumns: AppTableColumn[] = [
    { key: 'number', label: 'GRN #' },
    { key: 'date', label: 'Date' },
    { key: 'supplier', label: 'Supplier' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: 'Actions' },
  ];

  search = '';
  statusFilter = '';

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly poRows = signal<PurchaseOrderDto[]>([]);
  readonly grnRows = signal<GoodsReceiptDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);

  readonly branches = signal<BranchDto[]>([]);
  readonly warehouses = signal<WarehouseDto[]>([]);
  readonly suppliers = signal<SupplierDto[]>([]);
  readonly locations = signal<WarehouseLocationDto[]>([]);

  readonly poModalOpen = signal(false);
  readonly grnModalOpen = signal(false);
  readonly saving = signal(false);
  formErrors: Record<string, string> = {};

  poBranchId: string | number = '';
  poWarehouseId: string | number = '';
  poSupplierId: string | number = '';
  poProductId: number | null = null;
  poProductLabel = '';
  poProductUnitId: number | null = null;
  poQty: number | null = null;
  poUnitPrice: number | null = null;

  grnBranchId: string | number = '';
  grnWarehouseId: string | number = '';
  grnSupplierId: string | number = '';
  grnProductId: number | null = null;
  grnProductLabel = '';
  grnProductUnitId: number | null = null;
  grnQty: number | null = null;
  grnUnitCost: number | null = null;
  grnBatchNumber = '';
  grnExpiryDate = '';
  grnMrp: number | null = null;
  grnSalePrice: number | null = null;
  grnLocationId: string | number = '';

  readonly branchOptions = computed<AppSelectOption[]>(() =>
    this.branches().map((b) => ({ value: b.id, label: `${b.code} — ${b.name}` })),
  );

  readonly warehouseOptions = computed<AppSelectOption[]>(() =>
    this.warehouses().map((w) => ({ value: w.id, label: `${w.code} — ${w.name}` })),
  );

  readonly supplierOptions = computed<AppSelectOption[]>(() =>
    this.suppliers().map((s) => ({ value: s.id, label: `${s.code} — ${s.name}` })),
  );

  readonly locationOptions = computed<AppSelectOption[]>(() =>
    this.locations().map((l) => ({ value: l.id, label: `${l.code} — ${l.name}` })),
  );

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

  onPoProductPick(item: AppTypeaheadItem<ProductDto>): void {
    this.poProductId = Number(item.id);
    this.poProductLabel = `${item.label} (${item.detail || item.id})`;
  }

  onGrnProductPick(item: AppTypeaheadItem<ProductDto>): void {
    this.grnProductId = Number(item.id);
    this.grnProductLabel = `${item.label} (${item.detail || item.id})`;
  }

  readonly statusOptions: AppSelectOption[] = [
    { value: '', label: 'All statuses' },
    { value: 'Draft', label: 'Draft' },
    { value: 'Submitted', label: 'Submitted' },
    { value: 'Approved', label: 'Approved' },
    { value: 'Posted', label: 'Posted' },
  ];

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    forkJoin({
      branches: this.api.listBranches(),
      warehouses: this.api.listWarehouses(),
      suppliers: this.api.searchSuppliers('', 200),
    }).subscribe({
      next: ({ branches, warehouses, suppliers }) => {
        this.branches.set(branches.filter((b) => b.isActive));
        this.warehouses.set(warehouses.filter((w) => w.isActive));
        this.suppliers.set(suppliers.filter((s) => s.isActive));
        if (branches.length) this.poBranchId = this.grnBranchId = branches[0].id;
        if (warehouses.length) this.poWarehouseId = this.grnWarehouseId = warehouses[0].id;
        if (suppliers.length) this.poSupplierId = this.grnSupplierId = suppliers[0].id;
        this.loadPo(1);
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  setTab(tab: PurchasesTab): void {
    this.tab = tab;
    this.error.set('');
    if (tab === 'po') this.loadPo(1);
    else this.loadGrn(1);
  }

  applyFilters(): void {
    if (this.tab === 'po') this.loadPo(1);
    else this.loadGrn(1);
  }

  loadPo(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.api
      .searchPurchaseOrders({
        page,
        pageSize: 20,
        search: this.search,
        status: this.statusFilter || null,
      })
      .subscribe({
        next: (r) => {
          this.poRows.set(r.items);
          this.totalCount.set(r.totalCount);
          this.hasNext.set(r.hasNext);
          this.loading.set(false);
        },
        error: (err: unknown) => this.fail(err),
      });
  }

  loadGrn(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.api
      .searchGoodsReceipts({
        page,
        pageSize: 20,
        search: this.search,
        status: this.statusFilter || null,
      })
      .subscribe({
        next: (r) => {
          this.grnRows.set(r.items);
          this.totalCount.set(r.totalCount);
          this.hasNext.set(r.hasNext);
          this.loading.set(false);
        },
        error: (err: unknown) => this.fail(err),
      });
  }

  openPoCreate(): void {
    this.formErrors = {};
    this.poProductId = null;
    this.poProductLabel = '';
    this.poModalOpen.set(true);
  }

  closePoCreate(): void {
    if (!this.saving()) this.poModalOpen.set(false);
  }

  submitPoCreate(): void {
    this.formErrors = {};
    if (!this.poBranchId) this.formErrors['branch'] = 'Branch required.';
    if (!this.poWarehouseId) this.formErrors['warehouse'] = 'Warehouse required.';
    if (!this.poSupplierId) this.formErrors['supplier'] = 'Supplier required.';
    if (this.poProductId == null) this.formErrors['product'] = 'Select a product.';
    if (this.poProductUnitId == null) this.formErrors['unit'] = 'Product unit ID required.';
    if (this.poQty == null || this.poQty <= 0) this.formErrors['qty'] = 'Quantity required.';
    if (this.poUnitPrice == null || this.poUnitPrice < 0) this.formErrors['price'] = 'Unit price required.';
    if (Object.keys(this.formErrors).length) return;

    const body: CreatePurchaseOrderRequest = {
      branchId: Number(this.poBranchId),
      warehouseId: Number(this.poWarehouseId),
      supplierId: Number(this.poSupplierId),
      lines: [
        {
          productId: Number(this.poProductId),
          productUnitId: Number(this.poProductUnitId),
          quantity: Number(this.poQty),
          freeQuantity: 0,
          unitPrice: Number(this.poUnitPrice),
          discountAmount: 0,
          taxAmount: 0,
        },
      ],
    };
    this.saving.set(true);
    this.api.createPurchaseOrder(body).subscribe({
      next: (po) => {
        this.saving.set(false);
        this.poModalOpen.set(false);
        this.snackbar.success(`PO ${po.poNumber} created.`);
        this.loadPo(1);
      },
      error: (err: unknown) => this.saveFail(err),
    });
  }

  submitPo(row: PurchaseOrderDto): void {
    this.api.submitPurchaseOrder(row.id).subscribe({
      next: () => {
        this.snackbar.success('Purchase order submitted.');
        this.loadPo(this.page());
      },
      error: (err: unknown) => this.actionFail(err),
    });
  }

  approvePo(row: PurchaseOrderDto): void {
    this.api.approvePurchaseOrder(row.id).subscribe({
      next: () => {
        this.snackbar.success('Purchase order approved.');
        this.loadPo(this.page());
      },
      error: (err: unknown) => this.actionFail(err),
    });
  }

  openGrnCreate(): void {
    this.formErrors = {};
    this.grnProductId = null;
    this.grnProductLabel = '';
    this.refreshLocations();
    this.grnModalOpen.set(true);
  }

  closeGrnCreate(): void {
    if (!this.saving()) this.grnModalOpen.set(false);
  }

  onGrnWarehouseChange(): void {
    this.refreshLocations();
  }

  refreshLocations(): void {
    const whId = this.grnWarehouseId === '' ? undefined : Number(this.grnWarehouseId);
    this.api.listWarehouseLocations(whId).subscribe({
      next: (items) => {
        this.locations.set(items.filter((l) => l.isActive));
        if (items.length) this.grnLocationId = items[0].id;
      },
      error: () => this.locations.set([]),
    });
  }

  submitGrnCreate(): void {
    this.formErrors = {};
    if (!this.grnBranchId) this.formErrors['branch'] = 'Branch required.';
    if (!this.grnWarehouseId) this.formErrors['warehouse'] = 'Warehouse required.';
    if (!this.grnSupplierId) this.formErrors['supplier'] = 'Supplier required.';
    if (this.grnProductId == null) this.formErrors['product'] = 'Select a product.';
    if (this.grnProductUnitId == null) this.formErrors['unit'] = 'Product unit ID required.';
    if (this.grnQty == null || this.grnQty <= 0) this.formErrors['qty'] = 'Quantity required.';
    if (this.grnUnitCost == null) this.formErrors['cost'] = 'Unit cost required.';
    if (!this.grnBatchNumber.trim()) this.formErrors['batch'] = 'Batch number required.';
    if (!this.grnExpiryDate) this.formErrors['expiry'] = 'Expiry date required.';
    if (!this.grnLocationId) this.formErrors['location'] = 'Location required.';
    if (Object.keys(this.formErrors).length) return;

    const body: CreateGoodsReceiptRequest = {
      branchId: Number(this.grnBranchId),
      warehouseId: Number(this.grnWarehouseId),
      supplierId: Number(this.grnSupplierId),
      lines: [
        {
          productId: Number(this.grnProductId),
          productUnitId: Number(this.grnProductUnitId),
          orderedQuantity: Number(this.grnQty),
          receivedQuantity: Number(this.grnQty),
          freeQuantity: 0,
          unitCost: Number(this.grnUnitCost),
          discountAmount: 0,
          taxAmount: 0,
          batches: [
            {
              batchNumber: this.grnBatchNumber.trim(),
              expiryDate: this.grnExpiryDate,
              mrp: Number(this.grnMrp ?? 0),
              salePrice: Number(this.grnSalePrice ?? 0),
              quantity: Number(this.grnQty),
              freeQuantity: 0,
              warehouseLocationId: Number(this.grnLocationId),
            },
          ],
        },
      ],
    };
    this.saving.set(true);
    this.api.createGoodsReceiptDraft(body).subscribe({
      next: (grn) => {
        this.saving.set(false);
        this.grnModalOpen.set(false);
        this.snackbar.success(`GRN draft ${grn.grnNumber} created.`);
        this.loadGrn(1);
      },
      error: (err: unknown) => this.saveFail(err),
    });
  }

  postGrn(row: GoodsReceiptDto): void {
    this.api.postGoodsReceipt(row.id).subscribe({
      next: () => {
        this.snackbar.success('Goods receipt posted.');
        this.loadGrn(this.page());
      },
      error: (err: unknown) => this.actionFail(err),
    });
  }

  docStatusTone(status: string): AppBadgeTone {
    switch (status) {
      case 'Approved':
      case 'Posted':
        return 'success';
      case 'Submitted':
        return 'warning';
      case 'Cancelled':
        return 'danger';
      default:
        return 'neutral';
    }
  }

  supplierName(id: number): string {
    return this.suppliers().find((s) => s.id === id)?.name || `#${id}`;
  }

  private fail(err: unknown): void {
    this.loading.set(false);
    const message = err instanceof Error ? err.message : 'Could not load purchases.';
    this.error.set(message);
    this.snackbar.error(message);
  }

  private saveFail(err: unknown): void {
    this.saving.set(false);
    this.snackbar.error(err instanceof Error ? err.message : 'Save failed.');
  }

  private actionFail(err: unknown): void {
    this.snackbar.error(err instanceof Error ? err.message : 'Action failed.');
  }
}
