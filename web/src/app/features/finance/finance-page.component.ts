import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  AccountTypeDto,
  BranchDto,
  ChartOfAccountDto,
  JournalEntryDto,
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

interface JournalLineForm {
  accountId: string | number;
  debit: number | null;
  credit: number | null;
  description: string;
}

@Component({
  selector: 'app-finance-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DatePipe,
    DecimalPipe,
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
  templateUrl: './finance-page.component.html',
  styleUrl: './finance-page.component.scss',
})
export class FinancePageComponent implements OnInit {
  tab: 'coa' | 'journals' = 'coa';

  readonly coaColumns: AppTableColumn[] = [
    { key: 'code', label: 'Code' },
    { key: 'name', label: 'Name' },
    { key: 'type', label: 'Type' },
    { key: 'parent', label: 'Parent' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' },
  ];

  readonly journalColumns: AppTableColumn[] = [
    { key: 'number', label: 'Number' },
    { key: 'date', label: 'Date' },
    { key: 'branch', label: 'Branch' },
    { key: 'description', label: 'Description' },
    { key: 'totals', label: 'Debit / Credit' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' },
  ];

  readonly activeOptions: AppSelectOption[] = [
    { value: '', label: 'All' },
    { value: 'true', label: 'Active' },
    { value: 'false', label: 'Inactive' },
  ];

  readonly statusOptions: AppSelectOption[] = [
    { value: '', label: 'All statuses' },
    { value: 'Draft', label: 'Draft' },
    { value: 'Posted', label: 'Posted' },
    { value: 'Reversed', label: 'Reversed' },
  ];

  search = '';
  typeFilter: string | number = '';
  activeFilter: string | number = '';
  branchFilter: string | number = '';
  statusFilter: string | number = '';
  fromDate = '';
  toDate = '';

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly accounts = signal<ChartOfAccountDto[]>([]);
  readonly journals = signal<JournalEntryDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);

  readonly branches = signal<BranchDto[]>([]);
  readonly accountTypes = signal<AccountTypeDto[]>([]);
  readonly accountOptionsAll = signal<ChartOfAccountDto[]>([]);

  readonly createOpen = signal(false);
  readonly detailOpen = signal(false);
  readonly saving = signal(false);
  readonly selectedJournal = signal<JournalEntryDto | null>(null);

  editingAccountId: number | null = null;
  coaCode = '';
  coaName = '';
  coaTypeId: string | number = '';
  coaParentId: string | number = '';
  coaActive = true;
  coaErrors: Record<string, string> = {};

  jeBranchId: string | number = '';
  jeDate = new Date().toISOString().slice(0, 10);
  jeDescription = '';
  jeLines: JournalLineForm[] = [
    { accountId: '', debit: null, credit: null, description: '' },
    { accountId: '', debit: null, credit: null, description: '' },
  ];
  jeErrors: Record<string, string> = {};

  readonly typeOptions = computed<AppSelectOption[]>(() => [
    { value: '', label: 'All types' },
    ...this.accountTypes().map((t) => ({ value: t.id, label: t.name })),
  ]);

  readonly typePickOptions = computed<AppSelectOption[]>(() =>
    this.accountTypes().map((t) => ({ value: t.id, label: t.name })),
  );

  readonly branchOptions = computed<AppSelectOption[]>(() => [
    { value: '', label: 'All branches' },
    ...this.branches().map((b) => ({ value: b.id, label: `${b.code} — ${b.name}` })),
  ]);

  readonly branchPickOptions = computed<AppSelectOption[]>(() =>
    this.branches().map((b) => ({ value: b.id, label: `${b.code} — ${b.name}` })),
  );

  readonly parentOptions = computed<AppSelectOption[]>(() => [
    { value: '', label: 'No parent' },
    ...this.accountOptionsAll()
      .filter((a) => a.isActive && a.id !== this.editingAccountId)
      .map((a) => ({ value: a.id, label: `${a.code} — ${a.name}` })),
  ]);

  readonly accountPickOptions = computed<AppSelectOption[]>(() =>
    this.accountOptionsAll()
      .filter((a) => a.isActive)
      .map((a) => ({ value: a.id, label: `${a.code} — ${a.name}` })),
  );

  lineDebitTotal(): number {
    return this.jeLines.reduce((s, l) => s + (Number(l.debit) || 0), 0);
  }

  lineCreditTotal(): number {
    return this.jeLines.reduce((s, l) => s + (Number(l.credit) || 0), 0);
  }

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
      types: this.api.listAccountTypes(),
      accounts: this.api.searchChartOfAccounts({ page: 1, pageSize: 100, isActive: true }),
    }).subscribe({
      next: ({ branches, types, accounts }) => {
        this.branches.set(branches.filter((b) => b.isActive));
        this.accountTypes.set(types);
        this.accountOptionsAll.set(accounts.items);
        if (!this.jeBranchId && branches.length) this.jeBranchId = branches[0].id;
        if (!this.coaTypeId && types.length) this.coaTypeId = types[0].id;
        this.load(1);
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  setTab(tab: 'coa' | 'journals'): void {
    this.tab = tab;
    this.search = '';
    this.error.set('');
    this.load(1);
  }

  applyFilters(): void {
    this.load(1);
  }

  clearFilters(): void {
    this.search = '';
    this.typeFilter = '';
    this.activeFilter = '';
    this.branchFilter = '';
    this.statusFilter = '';
    this.fromDate = '';
    this.toDate = '';
    this.load(1);
  }

  load(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    if (this.tab === 'coa') {
      this.api
        .searchChartOfAccounts({
          page,
          pageSize: 20,
          search: this.search,
          accountTypeId: this.typeFilter === '' ? null : Number(this.typeFilter),
          isActive: this.activeFilter === '' ? null : this.activeFilter === 'true',
        })
        .subscribe({
          next: (res) => {
            this.accounts.set(res.items);
            this.totalCount.set(res.totalCount);
            this.hasNext.set(page * res.pageSize < res.totalCount);
            this.loading.set(false);
          },
          error: (err: unknown) => this.fail(err),
        });
    } else {
      this.api
        .searchJournalEntries({
          page,
          pageSize: 20,
          search: this.search,
          branchId: this.branchFilter === '' ? null : Number(this.branchFilter),
          status: this.statusFilter === '' ? null : String(this.statusFilter),
          fromDate: this.fromDate || null,
          toDate: this.toDate ? `${this.toDate}T23:59:59` : null,
        })
        .subscribe({
          next: (res) => {
            this.journals.set(res.items);
            this.totalCount.set(res.totalCount);
            this.hasNext.set(page * res.pageSize < res.totalCount);
            this.loading.set(false);
          },
          error: (err: unknown) => this.fail(err),
        });
    }
  }

  openCreate(): void {
    this.coaErrors = {};
    this.jeErrors = {};
    if (this.tab === 'coa') {
      this.editingAccountId = null;
      this.coaCode = '';
      this.coaName = '';
      this.coaTypeId = this.accountTypes()[0]?.id ?? '';
      this.coaParentId = '';
      this.coaActive = true;
    } else {
      this.jeBranchId = this.branches()[0]?.id ?? '';
      this.jeDate = new Date().toISOString().slice(0, 10);
      this.jeDescription = '';
      this.jeLines = [
        { accountId: '', debit: null, credit: null, description: '' },
        { accountId: '', debit: null, credit: null, description: '' },
      ];
    }
    this.createOpen.set(true);
  }

  openEditAccount(row: ChartOfAccountDto): void {
    this.editingAccountId = row.id;
    this.coaCode = row.code;
    this.coaName = row.name;
    this.coaTypeId = row.accountTypeId;
    this.coaParentId = row.parentAccountId ?? '';
    this.coaActive = row.isActive;
    this.coaErrors = {};
    this.createOpen.set(true);
  }

  addLine(): void {
    this.jeLines = [...this.jeLines, { accountId: '', debit: null, credit: null, description: '' }];
  }

  removeLine(index: number): void {
    if (this.jeLines.length <= 2) return;
    this.jeLines = this.jeLines.filter((_, i) => i !== index);
  }

  saveCreate(): void {
    if (this.tab === 'coa') this.saveAccount();
    else this.saveJournal();
  }

  saveAccount(): void {
    this.coaErrors = {};
    if (!this.coaCode.trim() && !this.editingAccountId) this.coaErrors['code'] = 'Account code is required.';
    if (!this.coaName.trim()) this.coaErrors['name'] = 'Account name is required.';
    if (!this.coaTypeId) this.coaErrors['type'] = 'Account type is required.';
    if (Object.keys(this.coaErrors).length) return;

    this.saving.set(true);
    const parentId = this.coaParentId === '' ? null : Number(this.coaParentId);
    const done = (msg: string) => {
      this.saving.set(false);
      this.createOpen.set(false);
      this.snackbar.success(msg);
      this.refreshAccountOptions();
      this.load(this.page());
    };
    const fail = (err: unknown) => {
      this.saving.set(false);
      this.fail(err);
    };

    if (this.editingAccountId) {
      this.api
        .updateChartOfAccount(this.editingAccountId, {
          name: this.coaName.trim(),
          accountTypeId: Number(this.coaTypeId),
          parentAccountId: parentId,
          isActive: this.coaActive,
        })
        .subscribe({ next: () => done('Account updated'), error: fail });
    } else {
      this.api
        .createChartOfAccount({
          code: this.coaCode.trim(),
          name: this.coaName.trim(),
          accountTypeId: Number(this.coaTypeId),
          parentAccountId: parentId,
          isActive: this.coaActive,
        })
        .subscribe({ next: () => done('Account created'), error: fail });
    }
  }

  saveJournal(): void {
    this.jeErrors = {};
    if (!this.jeBranchId) this.jeErrors['branch'] = 'Branch is required.';
    if (!this.jeDate) this.jeErrors['date'] = 'Entry date is required.';
    if (this.jeLines.length < 2) this.jeErrors['lines'] = 'At least two lines are required.';

    const lines = this.jeLines.map((l) => ({
      accountId: Number(l.accountId),
      debit: Number(l.debit) || 0,
      credit: Number(l.credit) || 0,
      description: l.description.trim() || null,
    }));

    for (let i = 0; i < lines.length; i++) {
      const l = lines[i];
      if (!l.accountId) {
        this.jeErrors['lines'] = `Line ${i + 1}: account is required.`;
        break;
      }
      if (!((l.debit > 0 && l.credit === 0) || (l.credit > 0 && l.debit === 0))) {
        this.jeErrors['lines'] = `Line ${i + 1}: enter either debit or credit.`;
        break;
      }
    }

    const debit = lines.reduce((s, l) => s + l.debit, 0);
    const credit = lines.reduce((s, l) => s + l.credit, 0);
    if (!this.jeErrors['lines'] && Math.round(debit * 10000) !== Math.round(credit * 10000)) {
      this.jeErrors['lines'] = `Journal is not balanced. Debit ${debit} ≠ Credit ${credit}.`;
    }
    if (Object.keys(this.jeErrors).length) return;

    this.saving.set(true);
    this.api
      .createJournalEntry({
        branchId: Number(this.jeBranchId),
        entryDate: new Date(this.jeDate).toISOString(),
        description: this.jeDescription.trim() || null,
        lines,
      })
      .subscribe({
        next: (created) => {
          this.saving.set(false);
          this.createOpen.set(false);
          this.snackbar.success(`Draft ${created.entryNumber} created`);
          this.load(1);
        },
        error: (err: unknown) => {
          this.saving.set(false);
          this.fail(err);
        },
      });
  }

  deactivate(row: ChartOfAccountDto): void {
    this.api.deactivateChartOfAccount(row.id).subscribe({
      next: () => {
        this.snackbar.success(`Account ${row.code} deactivated`);
        this.refreshAccountOptions();
        this.load(this.page());
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  openDetail(row: JournalEntryDto): void {
    this.api.getJournalEntry(row.id).subscribe({
      next: (detail) => {
        this.selectedJournal.set(detail);
        this.detailOpen.set(true);
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  postJournal(row: JournalEntryDto): void {
    this.api.postJournalEntry(row.id).subscribe({
      next: (posted) => {
        this.snackbar.success(`${posted.entryNumber} posted`);
        if (this.selectedJournal()?.id === row.id) this.selectedJournal.set(posted);
        this.load(this.page());
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  reverseJournal(row: JournalEntryDto): void {
    this.api.reverseJournalEntry(row.id).subscribe({
      next: (rev) => {
        this.snackbar.success(`Reversed as ${rev.entryNumber}`);
        this.detailOpen.set(false);
        this.load(this.page());
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  statusTone(status: string): 'success' | 'warning' | 'danger' | 'info' | 'neutral' {
    switch (status) {
      case 'Posted':
        return 'success';
      case 'Draft':
        return 'warning';
      case 'Reversed':
        return 'neutral';
      default:
        return 'info';
    }
  }

  private refreshAccountOptions(): void {
    this.api.searchChartOfAccounts({ page: 1, pageSize: 100 }).subscribe({
      next: (res) => this.accountOptionsAll.set(res.items),
    });
  }

  private fail(err: unknown): void {
    this.loading.set(false);
    const message = err instanceof Error ? err.message : 'Request failed';
    this.error.set(message);
    this.snackbar.error(message);
  }
}
