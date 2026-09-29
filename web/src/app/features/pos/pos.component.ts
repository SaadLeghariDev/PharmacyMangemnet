import {
  Component,
  HostListener,
  OnInit,
  ViewChild,
  computed,
  signal,
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  Observable,
  forkJoin,
  of,
  switchMap,
  catchError,
  map,
} from 'rxjs';
import { environment } from '../../../environments/environment';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  CartLine,
  CreateSalePaymentRequest,
  CustomerDto,
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
  AppModalComponent,
  AppTypeaheadComponent,
  AppTypeaheadItem,
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

type PayMethod = 1 | 2 | 3;

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CurrencyPipe,
    DatePipe,
    DecimalPipe,
    AppButtonComponent,
    AppInputComponent,
    AppBadgeComponent,
    AppEmptyStateComponent,
    AppLoadingStateComponent,
    AppConfirmDialogComponent,
    AppModalComponent,
    AppTypeaheadComponent,
  ],
  templateUrl: './pos.component.html',
  styleUrl: './pos.component.scss',
})
export class PosComponent implements OnInit {
  @ViewChild('productTypeahead') productTypeahead?: AppTypeaheadComponent<ProductDto>;

  search = '';
  customerSearch = '';
  newCustName = '';
  newCustPhone = '';

  readonly cashMethodId = 1 as PayMethod;
  readonly cardMethodId = 2 as PayMethod;
  readonly bankMethodId = 3 as PayMethod;

  readonly context = signal<PosContext | null>(null);
  readonly contextError = signal('');
  readonly lines = signal<CartLine[]>([]);
  readonly selectedCustomer = signal<CustomerDto | null>(null);
  readonly receiptCustomerPhone = signal<string | null>(null);
  readonly heldSales = signal<HeldSaleDto[]>([]);
  readonly loadingContext = signal(true);
  readonly busy = signal(false);
  readonly receipt = signal<SaleReceiptDto | null>(null);
  readonly receiptOpen = signal(false);
  readonly activeHoldId = signal<number | null>(null);
  readonly confirmRemove = signal<CartLine | null>(null);
  readonly confirmClear = signal(false);
  readonly createCustomerOpen = signal(false);
  readonly savingCustomer = signal(false);
  readonly payMethod = signal<PayMethod>(1);
  readonly splitEnabled = signal(false);
  readonly invoiceDiscountPct = signal(0);
  readonly cashTendered = signal<number | null>(null);
  readonly cashPay = signal<number | null>(null);
  readonly cardPay = signal<number | null>(null);

  readonly paymentSummary = computed(() => {
    const payments = this.receipt()?.sale?.payments ?? [];
    const names = payments
      .filter((p) => p.status === 'Completed')
      .map((p) => p.paymentMethodName || p.paymentMethodCode || 'Payment')
      .filter(Boolean);
    return [...new Set(names)].join(' · ') || '—';
  });

  readonly itemCount = computed(() =>
    this.lines().reduce((n, l) => n + l.quantity, 0),
  );
  readonly subtotal = computed(() =>
    this.lines().reduce((sum, l) => sum + l.quantity * l.unitPrice, 0),
  );
  readonly lineDiscountTotal = computed(() =>
    this.lines().reduce((sum, l) => sum + l.discountAmount, 0),
  );
  readonly invoiceDiscountAmount = computed(() => {
    const pct = Math.max(0, Math.min(100, Number(this.invoiceDiscountPct()) || 0));
    return Math.round(((this.subtotal() * pct) / 100) * 100) / 100;
  });
  readonly discountTotal = computed(
    () => Math.round((this.lineDiscountTotal() + this.invoiceDiscountAmount()) * 100) / 100,
  );
  readonly taxTotal = computed(() => this.lines().reduce((sum, l) => sum + l.taxAmount, 0));
  readonly payable = computed(
    () => Math.round((this.subtotal() - this.discountTotal() + this.taxTotal()) * 100) / 100,
  );

  readonly cashPortion = computed(() => {
    if (this.splitEnabled()) {
      return Math.max(0, Number(this.cashPay()) || 0);
    }
    return this.payMethod() === this.cashMethodId ? this.payable() : 0;
  });

  readonly changePreview = computed(() => {
    if (this.cashPortion() <= 0) return null;
    const tendered = Number(this.cashTendered());
    if (!Number.isFinite(tendered) || this.cashTendered() == null) return null;
    return Math.max(0, Math.round((tendered - this.cashPortion()) * 100) / 100);
  });

  readonly splitRemainder = computed(() => {
    if (!this.splitEnabled()) return 0;
    const cash = Math.max(0, Number(this.cashPay()) || 0);
    const card = Math.max(0, Number(this.cardPay()) || 0);
    return Math.round((this.payable() - cash - card) * 100) / 100;
  });

  readonly productSuggestFn = (q: string): Observable<AppTypeaheadItem<ProductDto>[]> => {
    const ctx = this.context();
    return this.api.searchProducts(q, 12).pipe(
      map((items) => items.filter((p) => p.isSaleable && p.isActive).slice(0, 8)),
      switchMap((items) => {
        if (!items.length) return of([]);
        if (!ctx) {
          return of(items.map((p) => this.toProductItem(p, null, null)));
        }
        return forkJoin(
          items.map((p) =>
            this.api.getFefo(p.id, ctx.warehouseId, 1).pipe(
              map((cands) =>
                this.toProductItem(
                  p,
                  cands[0]?.salePrice ?? null,
                  cands[0]?.availableQuantity ?? 0,
                ),
              ),
              catchError(() => of(this.toProductItem(p, null, 0))),
            ),
          ),
        );
      }),
      catchError(() => {
        this.snackbar.error('Product search failed.');
        return of([]);
      }),
    );
  };

  readonly customerSuggestFn = (q: string): Observable<AppTypeaheadItem<CustomerDto>[]> =>
    this.api.searchCustomers({ page: 1, pageSize: 8, search: q, isActive: true }).pipe(
      map((res) =>
        res.items.map((c) => ({
          id: c.id,
          label: c.name,
          detail: [c.customerCode, c.phone].filter(Boolean).join(' · '),
          data: c,
        })),
      ),
      catchError(() => {
        this.snackbar.error('Customer search failed.');
        return of([]);
      }),
    );

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.bootstrapContext();
  }

  @HostListener('document:keydown', ['$event'])
  onGlobalKey(event: KeyboardEvent): void {
    if (this.createCustomerOpen() || this.confirmClear() || this.confirmRemove() || this.receiptOpen()) {
      return;
    }

    if (event.key === 'F2') {
      event.preventDefault();
      this.focusSearch();
      return;
    }
    if (event.key === 'F4') {
      event.preventDefault();
      this.holdCart();
      return;
    }
    if (event.key === 'F8') {
      event.preventDefault();
      this.completeSale();
      return;
    }
    if (event.key === 'Escape' && this.lines().length) {
      event.preventDefault();
      this.askClear();
    }
  }

  focusSearch(): void {
    this.productTypeahead?.focus();
  }

  private toProductItem(
    p: ProductDto,
    salePrice: number | null,
    availableQuantity: number | null,
  ): AppTypeaheadItem<ProductDto> {
    const detailBits = [this.productDetailLine(p), p.sku].filter(Boolean);
    let badge: string | undefined;
    let badgeTone: 'ok' | 'warn' | 'muted' | undefined;
    if (availableQuantity == null) {
      badge = undefined;
    } else if (availableQuantity <= 0) {
      badge = 'No stock';
      badgeTone = 'warn';
    } else {
      badge = `Qty ${availableQuantity}`;
      badgeTone = 'ok';
    }
    return {
      id: p.id,
      label: p.name,
      detail: detailBits.join(' · '),
      trailing: salePrice != null ? `PKR ${salePrice.toFixed(2)}` : '—',
      badge,
      badgeTone,
      data: p,
    };
  }

  productDetailLine(p: ProductDto): string {
    const bits = [
      p.genericName,
      p.brandName,
      [p.strength, p.strengthUnit].filter(Boolean).join(' '),
      p.form,
    ].filter(Boolean);
    return bits.join(' · ');
  }

  onProductTypeaheadPick(item: AppTypeaheadItem<ProductDto>): void {
    if (item.data) this.pickSuggestion(item.data);
  }

  onProductTypeaheadSubmit(q: string): void {
    this.search = q;
    this.resolveSearch();
  }

  onCustomerTypeaheadPick(item: AppTypeaheadItem<CustomerDto>): void {
    if (item.data) this.pickCustomer(item.data);
  }

  onCustomerTypeaheadSubmit(q: string): void {
    this.customerSearch = q;
    if (!q.trim()) return;
    this.api.searchCustomers({ page: 1, pageSize: 8, search: q, isActive: true }).subscribe({
      next: (res) => {
        if (res.items.length === 1) this.pickCustomer(res.items[0]);
        else if (!res.items.length) this.snackbar.warning('No customers found.');
        else this.snackbar.info('Select a customer from the suggestions.');
      },
      error: (err: unknown) => this.snackbar.error(this.errText(err)),
    });
  }

  lineTotal(line: CartLine): number {
    return Math.round((line.quantity * line.unitPrice - line.discountAmount + line.taxAmount) * 100) / 100;
  }

  bootstrapContext(): void {
    this.loadingContext.set(true);
    this.contextError.set('');
    this.context.set(null);
    forkJoin({
      branches: this.api.listBranches(),
      counters: this.api.listCounters(),
      warehouses: this.api.listWarehouses(),
      terminals: this.api.listTerminals(),
    }).subscribe({
      next: ({ branches, counters, warehouses, terminals }) => {
        const branch = branches.find((b) => b.isActive) ?? branches[0];
        if (!branch) {
          this.contextError.set('No branch available. Seed organization data first.');
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
          this.contextError.set('Branch is missing counter, terminal, or warehouse.');
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
        this.focusSearch();
      },
      error: (err: unknown) => {
        this.loadingContext.set(false);
        this.contextError.set(this.errText(err));
      },
    });
  }

  setPayMethod(method: PayMethod): void {
    this.payMethod.set(method);
    if (method !== this.cashMethodId) {
      this.cashTendered.set(null);
    }
  }

  toggleSplit(on: boolean): void {
    this.splitEnabled.set(on);
    if (on) {
      this.cashPay.set(this.payable());
      this.cardPay.set(0);
      this.cashTendered.set(null);
    } else {
      this.cashPay.set(null);
      this.cardPay.set(null);
    }
  }

  setExactTender(): void {
    this.cashTendered.set(this.cashPortion());
  }

  addTenderChip(amount: number): void {
    const current = Number(this.cashTendered()) || 0;
    this.cashTendered.set(Math.round((current + amount) * 100) / 100);
  }

  roundUpTender(): void {
    const base = this.cashPortion();
    this.cashTendered.set(Math.ceil(base));
  }

  setInvoiceDiscountPct(v: number | string): void {
    this.invoiceDiscountPct.set(Math.max(0, Math.min(100, Number(v) || 0)));
  }

  setCashTendered(v: number | string | null): void {
    if (v === '' || v == null) {
      this.cashTendered.set(null);
      return;
    }
    this.cashTendered.set(Number(v));
  }

  setCashPay(v: number | string | null): void {
    this.cashPay.set(v === '' || v == null ? null : Number(v));
  }

  setCardPay(v: number | string | null): void {
    this.cardPay.set(v === '' || v == null ? null : Number(v));
  }

  pickCustomer(customer: CustomerDto): void {
    this.selectedCustomer.set(customer);
    this.customerSearch = '';
  }

  clearCustomer(): void {
    this.selectedCustomer.set(null);
  }

  openCreateCustomer(): void {
    this.newCustName = '';
    this.newCustPhone = '';
    this.createCustomerOpen.set(true);
  }

  saveNewCustomer(): void {
    const name = this.newCustName.trim();
    if (!name) {
      this.snackbar.warning('Customer name is required.');
      return;
    }
    this.savingCustomer.set(true);
    this.api
      .createCustomer({
        name,
        phone: this.newCustPhone.trim() || null,
        creditLimit: 0,
        isPatient: false,
      })
      .subscribe({
        next: (cust) => {
          this.savingCustomer.set(false);
          this.createCustomerOpen.set(false);
          this.selectedCustomer.set(cust);
          this.snackbar.success(`Customer ${cust.name} added.`);
        },
        error: (err: unknown) => {
          this.savingCustomer.set(false);
          this.snackbar.error(this.errText(err));
        },
      });
  }

  printReceipt(): void {
    if (!this.receipt()) return;
    window.print();
  }

  closeReceipt(): void {
    this.receiptOpen.set(false);
  }

  newSaleFromReceipt(): void {
    this.receiptOpen.set(false);
    this.receipt.set(null);
    this.receiptCustomerPhone.set(null);
    this.focusSearch();
  }

  resolveSearch(): void {
    const term = this.search.trim();
    if (!term || !this.context()) return;
    this.busy.set(true);

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
        this.focusSearch();
      },
      error: () => {
        this.api.getProductBySku(term).subscribe({
          next: (product) => {
            this.addProduct(product);
            this.search = '';
            this.busy.set(false);
            this.focusSearch();
          },
          error: () => {
            this.api.searchProducts(term).subscribe({
              next: (items) => {
                const saleable = items.filter((p) => p.isSaleable && p.isActive);
                this.busy.set(false);
                if (saleable.length === 1) {
                  this.addProduct(saleable[0]);
                  this.search = '';
                  this.focusSearch();
                } else if (!saleable.length) {
                  this.snackbar.warning('No products found.');
                } else {
                  this.snackbar.info('Multiple matches — pick one from the suggestions.');
                }
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
    this.search = '';
    this.focusSearch();
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
    this.receiptOpen.set(false);
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

  bumpQty(line: CartLine, delta: number): void {
    this.updateQty(line, line.quantity + delta);
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
    this.cashTendered.set(null);
    this.cashPay.set(null);
    this.cardPay.set(null);
    this.invoiceDiscountPct.set(0);
    this.splitEnabled.set(false);
    this.receipt.set(null);
    this.receiptOpen.set(false);
    this.activeHoldId.set(null);
    this.selectedCustomer.set(null);
    this.customerSearch = '';
    this.confirmClear.set(false);
    this.focusSearch();
  }

  holdCart(): void {
    const ctx = this.context();
    const cart = this.lines();
    if (!ctx || !cart.length || this.busy()) return;
    this.busy.set(true);
    this.api
      .holdSale({
        branchId: ctx.branchId,
        terminalId: ctx.terminalId,
        customerId: this.selectedCustomer()?.id ?? null,
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
        this.receiptOpen.set(false);
        if (resumed.customerId) {
          this.api.getCustomer(resumed.customerId).subscribe({
            next: (c) => this.selectedCustomer.set(c),
            error: () => this.selectedCustomer.set(null),
          });
        } else {
          this.selectedCustomer.set(null);
        }
        this.busy.set(false);
        this.snackbar.info(`Resumed hold #${resumed.id}`);
        this.refreshHolds();
        this.focusSearch();
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
    if (!ctx || !cart.length || this.busy()) return;

    const estimate = this.payable();
    if (estimate < 0) {
      this.snackbar.error('Payable cannot be negative. Check discounts.');
      return;
    }

    const payments = this.buildPayments(estimate);
    if (!payments) return;

    if (this.cashPortion() > 0) {
      const tendered = Number(this.cashTendered());
      if (this.cashTendered() != null && Number.isFinite(tendered) && tendered + 0.0001 < this.cashPortion()) {
        this.snackbar.error('Cash tendered is less than the cash portion.');
        return;
      }
    }

    this.busy.set(true);
    const idempotencyKey = `pos-${ctx.terminalId}-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
    const linesForApi = this.linesWithInvoiceDiscount(cart);

    this.api
      .createSale({
        branchId: ctx.branchId,
        counterId: ctx.counterId,
        terminalId: ctx.terminalId,
        warehouseId: ctx.warehouseId,
        customerId: this.selectedCustomer()?.id ?? null,
        saleType: 'Retail',
        currencyCode: 'PKR',
        roundOff: 0,
        idempotencyKey,
        lines: linesForApi.map((l) => ({
          productId: l.productId,
          productUnitId: l.productUnitId,
          quantity: l.quantity,
          unitPrice: l.unitPrice > 0 ? l.unitPrice : null,
          discountAmount: l.discountAmount,
          taxAmount: l.taxAmount,
        })),
        payments,
      })
      .subscribe({
        next: (sale) => {
          const holdId = this.activeHoldId();
          if (holdId != null) {
            this.api.discardHeldSale(holdId).subscribe({ error: () => undefined });
            this.activeHoldId.set(null);
          }
          const customerName = this.selectedCustomer()?.name;
          this.api.getReceipt(sale.id).subscribe({
            next: (receipt) => this.afterSaleSuccess(receipt),
            error: () =>
              this.afterSaleSuccess({
                sale,
                customerName: customerName ?? null,
                branchName: ctx.branchName,
                terminalCode: ctx.terminalCode,
              }),
          });
        },
        error: (err: unknown) => {
          this.busy.set(false);
          this.snackbar.error(this.errText(err));
        },
      });
  }

  private afterSaleSuccess(receipt: SaleReceiptDto): void {
    this.receiptCustomerPhone.set(this.selectedCustomer()?.phone ?? null);
    this.receipt.set(receipt);
    this.receiptOpen.set(true);
    this.lines.set([]);
    this.cashTendered.set(null);
    this.cashPay.set(null);
    this.cardPay.set(null);
    this.invoiceDiscountPct.set(0);
    this.splitEnabled.set(false);
    this.selectedCustomer.set(null);
    this.customerSearch = '';
    this.busy.set(false);
    this.snackbar.success(`Sale complete — ${receipt.sale.invoiceNumber}`);
    this.refreshHolds();
  }

  private linesWithInvoiceDiscount(cart: CartLine[]): CartLine[] {
    const invoiceDisc = this.invoiceDiscountAmount();
    const gross = this.subtotal();
    if (invoiceDisc <= 0 || gross <= 0) return cart;

    let allocated = 0;
    return cart.map((l, idx) => {
      const lineGross = l.quantity * l.unitPrice;
      let share =
        idx === cart.length - 1
          ? Math.round((invoiceDisc - allocated) * 100) / 100
          : Math.round(((lineGross / gross) * invoiceDisc) * 100) / 100;
      allocated += share;
      return { ...l, discountAmount: Math.round((l.discountAmount + share) * 100) / 100 };
    });
  }

  private buildPayments(payable: number): CreateSalePaymentRequest[] | null {
    if (this.splitEnabled()) {
      const cash = Math.round((Number(this.cashPay()) || 0) * 100) / 100;
      const card = Math.round((Number(this.cardPay()) || 0) * 100) / 100;
      if (cash < 0 || card < 0) {
        this.snackbar.error('Split amounts cannot be negative.');
        return null;
      }
      if (Math.abs(cash + card - payable) > 0.02) {
        this.snackbar.error('Cash + card must equal payable for split tender.');
        return null;
      }
      const payments: CreateSalePaymentRequest[] = [];
      if (cash > 0) payments.push({ paymentMethodId: this.cashMethodId, amount: cash });
      if (card > 0) payments.push({ paymentMethodId: this.cardMethodId, amount: card });
      if (!payments.length) {
        this.snackbar.error('Enter split payment amounts.');
        return null;
      }
      return payments;
    }

    return [
      {
        paymentMethodId: this.payMethod() || environment.defaultCashPaymentMethodId,
        amount: payable,
      },
    ];
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
