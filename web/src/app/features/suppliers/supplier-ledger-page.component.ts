import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  BranchDto,
  SupplierDto,
  SupplierLedgerEntryDto,
} from '../../core/models/api.models';
import {
  AppButtonComponent,
  AppEmptyStateComponent,
  AppInputComponent,
  AppLoadingStateComponent,
  AppPageHeaderComponent,
  AppSelectComponent,
  AppSelectOption,
  AppTableColumn,
  AppTableComponent,
  SnackbarService,
} from '../../shared';

@Component({
  selector: 'app-supplier-ledger-page',
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
  ],
  templateUrl: './supplier-ledger-page.component.html',
  styleUrl: './supplier-ledger-page.component.scss',
})
export class SupplierLedgerPageComponent implements OnInit {
  readonly columns: AppTableColumn[] = [
    { key: 'date', label: 'Date' },
    { key: 'type', label: 'Type' },
    { key: 'ref', label: 'Reference' },
    { key: 'debit', label: 'Debit' },
    { key: 'credit', label: 'Credit' },
    { key: 'balance', label: 'Balance' },
    { key: 'remarks', label: 'Remarks' },
  ];

  supplierId = 0;
  fromDate = '';
  toDate = '';
  branchFilter: string | number = '';

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<SupplierLedgerEntryDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);
  readonly supplier = signal<SupplierDto | null>(null);
  readonly branches = signal<BranchDto[]>([]);

  readonly branchOptions = computed<AppSelectOption[]>(() => [
    { value: '', label: 'All branches' },
    ...this.branches().map((b) => ({ value: b.id, label: `${b.code} — ${b.name}` })),
  ]);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.supplierId = Number(this.route.snapshot.paramMap.get('id'));
    this.bootstrap();
  }

  bootstrap(): void {
    this.loading.set(true);
    forkJoin({
      supplier: this.api.getSupplier(this.supplierId),
      branches: this.api.listBranches(),
    }).subscribe({
      next: ({ supplier, branches }) => {
        this.supplier.set(supplier);
        this.branches.set(branches.filter((b) => b.isActive));
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
      .getSupplierLedger(this.supplierId, {
        page,
        pageSize: 20,
        branchId: this.branchFilter === '' ? null : Number(this.branchFilter),
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
    this.fromDate = '';
    this.toDate = '';
    this.branchFilter = '';
    this.load(1);
  }

  private fail(err: unknown): void {
    this.loading.set(false);
    const message =
      err instanceof Error ? err.message : 'Could not load supplier ledger.';
    this.error.set(message);
    this.snackbar.error(message);
  }
}
