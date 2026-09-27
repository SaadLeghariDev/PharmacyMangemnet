import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  BranchDto,
  ExpenseCategoryDto,
  ExpenseDto,
  PosTerminalDto,
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
  selector: 'app-expenses-page',
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
  ],
  templateUrl: './expenses-page.component.html',
  styleUrl: './expenses-page.component.scss',
})
export class ExpensesPageComponent implements OnInit {
  readonly columns: AppTableColumn[] = [
    { key: 'number', label: 'Number' },
    { key: 'date', label: 'Date' },
    { key: 'category', label: 'Category' },
    { key: 'branch', label: 'Branch' },
    { key: 'payment', label: 'Payment' },
    { key: 'amount', label: 'Amount' },
    { key: 'description', label: 'Description' },
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
  categoryFilter: string | number = '';
  paymentFilter: string | number = '';

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<ExpenseDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);

  readonly branches = signal<BranchDto[]>([]);
  readonly categories = signal<ExpenseCategoryDto[]>([]);
  readonly terminals = signal<PosTerminalDto[]>([]);

  readonly createOpen = signal(false);
  readonly categoriesOpen = signal(false);
  readonly saving = signal(false);

  createBranchId: string | number = '';
  createCategoryId: string | number = '';
  createPaymentMethodId: string | number = environment.defaultCashPaymentMethodId;
  createTerminalId: string | number = '';
  createDate = new Date().toISOString().slice(0, 10);
  createAmount: number | null = null;
  createDescription = '';
  createErrors: Record<string, string> = {};

  /** Bound for template reactivity when payment method changes. */
  get isCashSelected(): boolean {
    return Number(this.createPaymentMethodId) === environment.defaultCashPaymentMethodId;
  }

  catName = '';
  catCode = '';
  catEditingId: number | null = null;
  catErrors: Record<string, string> = {};

  readonly branchOptions = computed<AppSelectOption[]>(() => [
    { value: '', label: 'All branches' },
    ...this.branches().map((b) => ({ value: b.id, label: `${b.code} — ${b.name}` })),
  ]);

  readonly categoryOptions = computed<AppSelectOption[]>(() => [
    { value: '', label: 'All categories' },
    ...this.categories().map((c) => ({ value: c.id, label: `${c.code} — ${c.name}` })),
  ]);

  readonly createBranchOptions = computed<AppSelectOption[]>(() =>
    this.branches().map((b) => ({ value: b.id, label: `${b.code} — ${b.name}` })),
  );

  readonly createCategoryOptions = computed<AppSelectOption[]>(() =>
    this.categories().map((c) => ({ value: c.id, label: `${c.code} — ${c.name}` })),
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
      categories: this.api.listExpenseCategories(),
      terminals: this.api.listTerminals(),
    }).subscribe({
      next: ({ branches, categories, terminals }) => {
        this.branches.set(branches.filter((b) => b.isActive));
        this.categories.set(categories);
        this.terminals.set(terminals.filter((t) => t.isActive));
        if (!this.createBranchId && branches.length) this.createBranchId = branches[0].id;
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
      .searchExpenses({
        page,
        pageSize: 20,
        search: this.search,
        branchId: this.branchFilter === '' ? null : Number(this.branchFilter),
        categoryId: this.categoryFilter === '' ? null : Number(this.categoryFilter),
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
    this.categoryFilter = '';
    this.paymentFilter = '';
    this.load(1);
  }

  openCreate(): void {
    this.createErrors = {};
    this.createDate = new Date().toISOString().slice(0, 10);
    this.createAmount = null;
    this.createDescription = '';
    this.createPaymentMethodId = environment.defaultCashPaymentMethodId;
    if (!this.createBranchId && this.branches().length) this.createBranchId = this.branches()[0].id;
    if (!this.createCategoryId && this.categories().length)
      this.createCategoryId = this.categories()[0].id;
    if (!this.createTerminalId && this.terminals().length)
      this.createTerminalId = this.terminals()[0].id;
    this.createOpen.set(true);
  }

  closeCreate(): void {
    if (!this.saving()) this.createOpen.set(false);
  }

  submitCreate(): void {
    this.createErrors = {};
    if (!this.createBranchId) this.createErrors['branch'] = 'Branch is required.';
    if (!this.createCategoryId) this.createErrors['category'] = 'Expense category is required.';
    if (!this.createDate) this.createErrors['date'] = 'Expense date is required.';
    if (this.createAmount == null || Number(this.createAmount) <= 0)
      this.createErrors['amount'] = 'Amount must be greater than zero.';
    if (!this.createPaymentMethodId)
      this.createErrors['payment'] = 'Payment method is required.';
    if (this.isCashSelected && !this.createTerminalId)
      this.createErrors['terminal'] = 'Terminal is required for cash expenses.';

    if (Object.keys(this.createErrors).length) return;

    this.saving.set(true);
    this.api
      .createExpense({
        branchId: Number(this.createBranchId),
        categoryId: Number(this.createCategoryId),
        expenseDate: new Date(this.createDate).toISOString(),
        amount: Number(this.createAmount),
        paymentMethodId: Number(this.createPaymentMethodId),
        description: this.createDescription.trim() || null,
        terminalId: this.isCashSelected ? Number(this.createTerminalId) : null,
      })
      .subscribe({
        next: (expense) => {
          this.saving.set(false);
          this.createOpen.set(false);
          this.snackbar.success(`Expense ${expense.expenseNumber} recorded.`);
          this.load(1);
        },
        error: (err: unknown) => {
          this.saving.set(false);
          const message = err instanceof Error ? err.message : 'Could not create expense.';
          this.snackbar.error(message);
        },
      });
  }

  openCategories(): void {
    this.resetCategoryForm();
    this.categoriesOpen.set(true);
    this.refreshCategories();
  }

  closeCategories(): void {
    if (!this.saving()) this.categoriesOpen.set(false);
  }

  editCategory(cat: ExpenseCategoryDto): void {
    this.catEditingId = cat.id;
    this.catName = cat.name;
    this.catCode = cat.code;
    this.catErrors = {};
  }

  resetCategoryForm(): void {
    this.catEditingId = null;
    this.catName = '';
    this.catCode = '';
    this.catErrors = {};
  }

  saveCategory(): void {
    this.catErrors = {};
    if (!this.catName.trim()) this.catErrors['name'] = 'Category name is required.';
    if (!this.catCode.trim()) this.catErrors['code'] = 'Category code is required.';
    if (Object.keys(this.catErrors).length) return;

    this.saving.set(true);
    const body = { name: this.catName.trim(), code: this.catCode.trim() };
    const req$ =
      this.catEditingId == null
        ? this.api.createExpenseCategory(body)
        : this.api.updateExpenseCategory(this.catEditingId, body);

    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.snackbar.success(this.catEditingId == null ? 'Category created.' : 'Category updated.');
        this.resetCategoryForm();
        this.refreshCategories();
      },
      error: (err: unknown) => {
        this.saving.set(false);
        const message = err instanceof Error ? err.message : 'Could not save category.';
        this.snackbar.error(message);
      },
    });
  }

  private refreshCategories(): void {
    this.api.listExpenseCategories().subscribe({
      next: (items) => this.categories.set(items),
      error: (err: unknown) => {
        const message = err instanceof Error ? err.message : 'Could not load categories.';
        this.snackbar.error(message);
      },
    });
  }

  private fail(err: unknown): void {
    this.loading.set(false);
    const message =
      err instanceof Error
        ? err.message
        : 'Could not load expenses. Check the API connection and try again.';
    this.error.set(message);
    this.snackbar.error(message);
  }
}
