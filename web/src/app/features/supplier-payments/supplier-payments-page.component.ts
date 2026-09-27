import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  BranchDto,
  PosTerminalDto,
  SupplierDto,
  SupplierPaymentDto,
} from '../../core/models/api.models';
import {
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
  selector: 'app-supplier-payments-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
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
  ],
  templateUrl: './supplier-payments-page.component.html',
  styleUrl: './supplier-payments-page.component.scss',
})
export class SupplierPaymentsPageComponent implements OnInit {
  readonly columns: AppTableColumn[] = [
    { key: 'date', label: 'Date' },
    { key: 'supplier', label: 'Supplier' },
    { key: 'branch', label: 'Branch' },
    { key: 'payment', label: 'Payment' },
    { key: 'reference', label: 'Reference' },
    { key: 'amount', label: 'Amount' },
    { key: 'remarks', label: 'Remarks' },
  ];

  readonly paymentOptions: AppSelectOption[] = [
    { value: '', label: 'All payment methods' },
    { value: 1, label: 'Cash' },
    { value: 2, label: 'Card' },
    { value: 3, label: 'Bank Transfer' },
  ];

  readonly createPaymentOptions: AppSelectOption[] = [
    { value: 1, label: 'Cash' },
    { value: 2, label: 'Card' },
    { value: 3, label: 'Bank Transfer' },
  ];

  search = '';
  fromDate = '';
  toDate = '';
  branchFilter: string | number = '';
  supplierFilter: string | number = '';
  paymentFilter: string | number = '';

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<SupplierPaymentDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);

  readonly branches = signal<BranchDto[]>([]);
  readonly suppliers = signal<SupplierDto[]>([]);
  readonly terminals = signal<PosTerminalDto[]>([]);

  readonly createOpen = signal(false);
  readonly saving = signal(false);

  createSupplierId: string | number = '';
  createBranchId: string | number = '';
  createPaymentMethodId: string | number = environment.defaultCashPaymentMethodId;
  createTerminalId: string | number = '';
  createDate = new Date().toISOString().slice(0, 10);
  createAmount: number | null = null;
  createReference = '';
  createRemarks = '';
  createErrors: Record<string, string> = {};

  get isCashSelected(): boolean {
    return Number(this.createPaymentMethodId) === environment.defaultCashPaymentMethodId;
  }

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

  readonly terminalOptions = computed<AppSelectOption[]>(() =>
    this.terminals().map((t) => ({
      value: t.id,
      label: `${t.terminalCode}${t.isPrimary ? ' (primary)' : ''}`,
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
    forkJoin({
      branches: this.api.listBranches(),
      suppliers: this.api.searchSuppliers(),
      terminals: this.api.listTerminals(),
    }).subscribe({
      next: ({ branches, suppliers, terminals }) => {
        this.branches.set(branches.filter((b) => b.isActive));
        this.suppliers.set(suppliers);
        this.terminals.set(terminals.filter((t) => t.isActive));
        if (!this.createBranchId && branches.length) this.createBranchId = branches[0].id;
        if (!this.createSupplierId && suppliers.length) this.createSupplierId = suppliers[0].id;
        if (!this.createTerminalId && terminals.length) this.createTerminalId = terminals[0].id;
        this.load(1);
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  load(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    this.api
      .searchSupplierPayments({
        page,
        pageSize: 20,
        search: this.search,
        branchId: this.branchFilter === '' ? null : Number(this.branchFilter),
        supplierId: this.supplierFilter === '' ? null : Number(this.supplierFilter),
        paymentMethodId: this.paymentFilter === '' ? null : Number(this.paymentFilter),
        fromDate: this.fromDate || null,
        toDate: this.toDate ? `${this.toDate}T23:59:59` : null,
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
    this.fromDate = '';
    this.toDate = '';
    this.branchFilter = '';
    this.supplierFilter = '';
    this.paymentFilter = '';
    this.load(1);
  }

  openCreate(): void {
    this.createErrors = {};
    this.createDate = new Date().toISOString().slice(0, 10);
    this.createAmount = null;
    this.createReference = '';
    this.createRemarks = '';
    this.createPaymentMethodId = environment.defaultCashPaymentMethodId;
    if (!this.createBranchId && this.branches().length) this.createBranchId = this.branches()[0].id;
    if (!this.createSupplierId && this.suppliers().length)
      this.createSupplierId = this.suppliers()[0].id;
    if (!this.createTerminalId && this.terminals().length)
      this.createTerminalId = this.terminals()[0].id;
    this.createOpen.set(true);
  }

  closeCreate(): void {
    if (!this.saving()) this.createOpen.set(false);
  }

  submitCreate(): void {
    this.createErrors = {};
    if (!this.createSupplierId) this.createErrors['supplier'] = 'Supplier is required.';
    if (!this.createBranchId) this.createErrors['branch'] = 'Branch is required.';
    if (!this.createDate) this.createErrors['date'] = 'Payment date is required.';
    if (this.createAmount == null || Number(this.createAmount) <= 0)
      this.createErrors['amount'] = 'Amount must be greater than zero.';
    if (!this.createPaymentMethodId)
      this.createErrors['payment'] = 'Payment method is required.';
    if (this.isCashSelected && !this.createTerminalId)
      this.createErrors['terminal'] = 'Terminal is required for cash payments.';
    if (Object.keys(this.createErrors).length) return;

    this.saving.set(true);
    this.api
      .createSupplierPayment({
        supplierId: Number(this.createSupplierId),
        branchId: Number(this.createBranchId),
        paymentMethodId: Number(this.createPaymentMethodId),
        amount: Number(this.createAmount),
        paymentDate: new Date(this.createDate).toISOString(),
        referenceNumber: this.createReference.trim() || null,
        remarks: this.createRemarks.trim() || null,
        terminalId: this.isCashSelected ? Number(this.createTerminalId) : null,
      })
      .subscribe({
        next: (payment) => {
          this.saving.set(false);
          this.createOpen.set(false);
          this.snackbar.success(`Payment #${payment.id} recorded.`);
          this.load(1);
        },
        error: (err: unknown) => {
          this.saving.set(false);
          const message = err instanceof Error ? err.message : 'Could not record payment.';
          this.snackbar.error(message);
        },
      });
  }

  private fail(err: unknown): void {
    this.loading.set(false);
    const message =
      err instanceof Error
        ? err.message
        : 'Could not load supplier payments. Check the API connection and try again.';
    this.error.set(message);
    this.snackbar.error(message);
  }
}
