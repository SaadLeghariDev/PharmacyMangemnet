import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  CartLine,
  HeldSaleDto,
  ProductDto,
  SaleReceiptDto,
} from '../../core/models/api.models';
import {
  AppBadgeComponent,
  AppButtonComponent,
  AppConfirmDialogComponent,
  AppEmptyStateComponent,
  AppInputComponent,
  AppLoadingStateComponent,
  AppPageHeaderComponent,
  AppSelectComponent,
  AppSelectOption,
  SnackbarService,
} from '../../shared';

interface PosContext {
  branchId: number;
  branchName: string;
  counterId: number;
  terminalId: number;
  terminalCode: string;
  warehouseId: number;
  warehouseName: string;
}

@Component({
  selector: 'app-pos',
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
    AppBadgeComponent,
    AppEmptyStateComponent,
    AppLoadingStateComponent,
    AppConfirmDialogComponent,
  ],
  templateUrl: './pos.component.html',
  styleUrl: './pos.component.scss',
})
export class PosComponent implements OnInit {
  search = '';
  tendered: number | null = null;
  paymentMethodId: number | string = environment.defaultCashPaymentMethodId;

  readonly context = signal<PosContext | null>(null);
  readonly lines = signal<CartLine[]>([]);
  readonly suggestions = signal<ProductDto[]>([]);
  readonly heldSales = signal<HeldSaleDto[]>([]);
  readonly loadingContext = signal(true);
  readonly busy = signal(false);
  readonly receipt = signal<SaleReceiptDto | null>(null);
  readonly activeHoldId = signal<number | null>(null);
  readonly confirmRemove = signal<CartLine | null>(null);
  readonly confirmClear = signal(false);

  readonly paymentOptions: AppSelectOption[] = [
    { value: 1, label: 'Cash' },
    { value: 2, label: 'Card' },
    { value: 3, label: 'Bank Transfer' },
  ];

  readonly subtotal = computed(() =>
    this.lines().reduce((sum, l) => sum + l.quantity * l.unitPrice, 0),
  );
  readonly discountTotal = computed(() =>
    this.lines().reduce((sum, l) => sum + l.discountAmount, 0),
  );
  readonly taxTotal = computed(() => this.lines().reduce((sum, l) => sum + l.taxAmount, 0));
  readonly payable = computed(
    () => Math.round((this.subtotal() - this.discountTotal() + this.taxTotal()) * 100) / 100,
  );
  readonly changePreview = computed(() => {
    if (this.tendered == null) return null;
    return Math.max(0, Math.round((Number(this.tendered) - this.payable()) * 100) / 100);
  });

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.bootstrapContext();
  }

  lineTotal(line: CartLine): number {
    return Math.round((line.quantity * line.unitPrice - line.discountAmount + line.taxAmount) * 100) / 100;
  }

  bootstrapContext(): void {
    this.loadingContext.set(true);
    forkJoin({
      branches: this.api.listBranches(),
      counters: this.api.listCounters(),
      warehouses: this.api.listWarehouses(),
      terminals: this.api.listTerminals(),
    }).subscribe({
      next: ({ branches, counters, warehouses, terminals }) => {
        const branch = branches.find((b) => b.isActive) ?? branches[0];
        if (!branch) {
          this.snackbar.error('No branch available. Seed organization data first.');
          this.loadingContext.set(false);
          return;
        }
        const counter =
          counters.find((c) => c.branchId === branch.id && c.isActive) ??
          counters.find((c) => c.branchId === branch.id);
        const terminal =
          terminals.find((t) => t.branchId === branch.id && t.isActive) ??
          terminals.find((t) => t.branchId === branch.id);
        const warehouse =
          warehouses.find((w) => w.branchId === branch.id && w.isMain && w.isActive) ??
          warehouses.find((w) => w.branchId === branch.id && w.isActive);

        if (!counter || !terminal || !warehouse) {
          this.snackbar.error('Branch is missing counter, terminal, or warehouse.');
          this.loadingContext.set(false);
          return;
        }

        this.context.set({
          branchId: branch.id,
          branchName: branch.name,
          counterId: counter.id,
          terminalId: terminal.id,
          terminalCode: terminal.terminalCode,
          warehouseId: warehouse.id,
          warehouseName: warehouse.name,
        });
        this.refreshHolds();
        this.loadingContext.set(false);
      },
      error: (err: unknown) => {
        this.loadingContext.set(false);
        this.snackbar.error(this.errText(err));
      },
    });
  }

  onSearchKey(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.resolveSearch();
    }
  }

  resolveSearch(): void {
    const term = this.search.trim();
    if (!term || !this.context()) return;
    this.busy.set(true);
    this.suggestions.set([]);

    this.api.getProductByBarcode(term).subscribe({
      next: (hit) => {
        this.addLine({
          productId: hit.productId,
          productUnitId: hit.productUnitId,
          sku: hit.sku,
          productName: hit.productName,
          quantity: 1,
          unitPrice: 0,
          discountAmount: 0,
          taxAmount: 0,
        });
        this.search = '';
        this.busy.set(false);
      },
      error: () => {
        this.api.getProductBySku(term).subscribe({
          next: (product) => {
            this.addProduct(product);
            this.search = '';
            this.busy.set(false);
          },
          error: () => {
            this.api.searchProducts(term).subscribe({
              next: (items) => {
                const saleable = items.filter((p) => p.isSaleable && p.isActive);
                this.suggestions.set(saleable);
                if (saleable.length === 1) {
                  this.addProduct(saleable[0]);
                  this.search = '';
                  this.suggestions.set([]);
                } else if (!saleable.length) {
                  this.snackbar.warning(`No product matched “${term}”.`);
                }
                this.busy.set(false);
              },
              error: (err: unknown) => {
                this.busy.set(false);
                this.snackbar.error(this.errText(err));
              },
            });
          },
        });
      },
    });
  }

  pickSuggestion(product: ProductDto): void {
    this.addProduct(product);
    this.suggestions.set([]);
    this.search = '';
  }

  addProduct(product: ProductDto): void {
    if (!product.defaultSaleUnitId) {
      this.snackbar.warning(`${product.name} has no sale unit. Scan its barcode.`);
      return;
    }
    this.addLine({
      productId: product.id,
      productUnitId: product.defaultSaleUnitId,
      sku: product.sku,
      productName: product.name,
      quantity: 1,
      unitPrice: 0,
      discountAmount: 0,
      taxAmount: 0,
    });
  }

  addLine(line: CartLine): void {
    const ctx = this.context();
    const existing = this.lines().find(
      (l) => l.productId === line.productId && l.productUnitId === line.productUnitId,
    );
    if (existing) {
      this.updateQty(existing, existing.quantity + 1);
      return;
    }
    this.lines.update((rows) => [...rows, line]);
    this.receipt.set(null);
    if (ctx) {
      this.api.getFefo(line.productId, ctx.warehouseId, 1).subscribe({
        next: (candidates) => {
          const first = candidates[0];
          this.lines.update((rows) =>
            rows.map((r) =>
              r.productId === line.productId && r.productUnitId === line.productUnitId
                ? {
                    ...r,
                    unitPrice: first?.salePrice ?? r.unitPrice,
                    batchNumber: first?.batchNumber,
                    expiryDate: first?.expiryDate,
                  }
                : r,
            ),
          );
        },
        error: () => undefined,
      });
    }
  }

  updateQty(line: CartLine, quantity: number | string): void {
    const qty = Math.max(0.001, Number(quantity) || 1);
    this.lines.update((rows) =>
      rows.map((r) =>
        r.productId === line.productId && r.productUnitId === line.productUnitId
          ? { ...r, quantity: qty }
          : r,
      ),
    );
  }

  updateDiscount(line: CartLine, discount: number | string): void {
    const d = Math.max(0, Number(discount) || 0);
    this.lines.update((rows) =>
      rows.map((r) =>
        r.productId === line.productId && r.productUnitId === line.productUnitId
          ? { ...r, discountAmount: d }
          : r,
      ),
    );
  }

  askRemove(line: CartLine): void {
    this.confirmRemove.set(line);
  }

  confirmRemoveLine(): void {
    const line = this.confirmRemove();
    if (!line) return;
    this.lines.update((rows) =>
      rows.filter((r) => !(r.productId === line.productId && r.productUnitId === line.productUnitId)),
    );
    this.confirmRemove.set(null);
  }

  askClear(): void {
    if (!this.lines().length) return;
    this.confirmClear.set(true);
  }

  doClear(): void {
    this.lines.set([]);
    this.tendered = null;
    this.receipt.set(null);
    this.activeHoldId.set(null);
    this.confirmClear.set(false);
  }

  holdCart(): void {
    const ctx = this.context();
    const cart = this.lines();
    if (!ctx || !cart.length) return;
    this.busy.set(true);
    this.api
      .holdSale({
        branchId: ctx.branchId,
        terminalId: ctx.terminalId,
        cartData: this.api.serializeCart(cart),
        totalAmount: this.payable(),
      })
      .subscribe({
        next: (held) => {
          this.busy.set(false);
          this.doClear();
          this.snackbar.success(`Cart held (#${held.id}).`);
          this.refreshHolds();
        },
        error: (err: unknown) => {
          this.busy.set(false);
          this.snackbar.error(this.errText(err));
        },
      });
  }

  refreshHolds(): void {
    const ctx = this.context();
    if (!ctx) return;
    this.api.listHeldSales(ctx.branchId, ctx.terminalId).subscribe({
      next: (items) => this.heldSales.set(items),
      error: () => this.heldSales.set([]),
    });
  }

  resumeHold(held: HeldSaleDto): void {
    this.busy.set(true);
    this.api.resumeHeldSale(held.id).subscribe({
      next: (resumed) => {
        this.lines.set(this.api.parseCart(resumed.cartData));
        this.activeHoldId.set(resumed.id);
        this.receipt.set(null);
        this.busy.set(false);
        this.snackbar.info(`Resumed hold #${resumed.id}`);
        this.refreshHolds();
      },
      error: (err: unknown) => {
        this.busy.set(false);
        this.snackbar.error(this.errText(err));
      },
    });
  }

  discardHold(held: HeldSaleDto): void {
    this.api.discardHeldSale(held.id).subscribe({
      next: () => {
        if (this.activeHoldId() === held.id) this.activeHoldId.set(null);
        this.snackbar.info(`Discarded hold #${held.id}`);
        this.refreshHolds();
      },
      error: (err: unknown) => this.snackbar.error(this.errText(err)),
    });
  }

  completeSale(): void {
    const ctx = this.context();
    const cart = this.lines();
    if (!ctx || !cart.length) return;

    const estimate = this.payable();
    const paid =
      this.tendered == null || Number(this.tendered) <= 0 ? estimate : Number(this.tendered);
    if (paid + 0.0001 < estimate) {
      this.snackbar.error('Tendered amount is less than the payable total.');
      return;
    }

    this.busy.set(true);
    const idempotencyKey = `pos-${ctx.terminalId}-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;

    this.api
      .createSale({
        branchId: ctx.branchId,
        counterId: ctx.counterId,
        terminalId: ctx.terminalId,
        warehouseId: ctx.warehouseId,
        saleType: 'Retail',
        currencyCode: 'PKR',
        roundOff: 0,
        idempotencyKey,
        lines: cart.map((l) => ({
          productId: l.productId,
          productUnitId: l.productUnitId,
          quantity: l.quantity,
          unitPrice: l.unitPrice > 0 ? l.unitPrice : null,
          discountAmount: l.discountAmount,
          taxAmount: l.taxAmount,
        })),
        payments: [
          {
            paymentMethodId: Number(this.paymentMethodId) || environment.defaultCashPaymentMethodId,
            amount: paid,
          },
        ],
      })
      .subscribe({
        next: (sale) => {
          const holdId = this.activeHoldId();
          if (holdId != null) {
            this.api.discardHeldSale(holdId).subscribe({ error: () => undefined });
            this.activeHoldId.set(null);
          }
          this.api.getReceipt(sale.id).subscribe({
            next: (receipt) => {
              this.receipt.set(receipt);
              this.lines.set([]);
              this.tendered = null;
              this.busy.set(false);
              this.snackbar.success(`Sale complete — ${sale.invoiceNumber}`);
              this.refreshHolds();
            },
            error: () => {
              this.receipt.set({
                sale,
                branchName: ctx.branchName,
                terminalCode: ctx.terminalCode,
              });
              this.lines.set([]);
              this.tendered = null;
              this.busy.set(false);
              this.snackbar.success(`Sale complete — ${sale.invoiceNumber}`);
            },
          });
        },
        error: (err: unknown) => {
          this.busy.set(false);
          this.snackbar.error(this.errText(err));
        },
      });
  }

  private errText(err: unknown): string {
    if (err && typeof err === 'object' && 'error' in err) {
      const body = (err as { error?: { message?: string; errors?: string[] } }).error;
      if (body?.errors?.length) return body.errors.join('; ');
      if (body?.message) return body.message;
    }
    if (err instanceof Error) return err.message;
    return 'Request failed';
  }
}
