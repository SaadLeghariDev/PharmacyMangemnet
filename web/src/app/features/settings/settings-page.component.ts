import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  AuditLogDto,
  BranchDto,
  BranchSettingDto,
  ReasonCodeDto,
  TenantSettingDto,
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
  selector: 'app-settings-page',
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
  templateUrl: './settings-page.component.html',
  styleUrl: './settings-page.component.scss',
})
export class SettingsPageComponent implements OnInit {
  tab: 'tenant' | 'branch' | 'reasons' | 'audit' = 'tenant';

  readonly settingColumns: AppTableColumn[] = [
    { key: 'key', label: 'Key' },
    { key: 'value', label: 'Value' },
    { key: 'encrypted', label: 'Encrypted' },
    { key: 'actions', label: '' },
  ];

  readonly branchSettingColumns: AppTableColumn[] = [
    { key: 'branch', label: 'Branch' },
    { key: 'key', label: 'Key' },
    { key: 'value', label: 'Value' },
    { key: 'actions', label: '' },
  ];

  readonly reasonColumns: AppTableColumn[] = [
    { key: 'type', label: 'Type' },
    { key: 'code', label: 'Code' },
    { key: 'name', label: 'Name' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' },
  ];

  readonly auditColumns: AppTableColumn[] = [
    { key: 'when', label: 'When' },
    { key: 'entity', label: 'Entity' },
    { key: 'action', label: 'Action' },
    { key: 'user', label: 'User' },
    { key: 'detail', label: 'Detail' },
  ];

  readonly reasonTypeOptions: AppSelectOption[] = [
    { value: '', label: 'All types' },
    { value: 'Return', label: 'Return' },
    { value: 'Adjustment', label: 'Adjustment' },
    { value: 'Count', label: 'Count' },
    { value: 'Void', label: 'Void' },
    { value: 'Other', label: 'Other' },
  ];

  search = '';
  reasonTypeFilter: string | number = '';
  branchFilter: string | number = '';
  branchOptions: AppSelectOption[] = [{ value: '', label: 'All branches' }];

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly tenantSettings = signal<TenantSettingDto[]>([]);
  readonly branchSettings = signal<BranchSettingDto[]>([]);
  readonly reasons = signal<ReasonCodeDto[]>([]);
  readonly audits = signal<AuditLogDto[]>([]);
  readonly branches = signal<BranchDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);

  readonly modalOpen = signal(false);
  readonly saving = signal(false);
  modalKind: 'tenant' | 'branch' | 'reason' = 'tenant';
  editingReasonId: number | null = null;

  formKey = '';
  formValue = '';
  formEncrypted = false;
  formBranchId: string | number = '';
  formReasonType: string | number = 'Adjustment';
  formReasonCode = '';
  formReasonName = '';
  formReasonDescription = '';
  formReasonActive = true;

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.api.listBranches().subscribe({
      next: (branches) => {
        this.branches.set(branches);
        this.branchOptions = [
          { value: '', label: 'All branches' },
          ...branches.map((b) => ({ value: b.id, label: `${b.code} — ${b.name}` })),
        ];
        if (branches.length) this.formBranchId = branches[0].id;
        this.load(1);
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  setTab(tab: 'tenant' | 'branch' | 'reasons' | 'audit'): void {
    this.tab = tab;
    this.search = '';
    this.reasonTypeFilter = '';
    this.load(1);
  }

  applyFilters(): void {
    this.load(1);
  }

  clearFilters(): void {
    this.search = '';
    this.reasonTypeFilter = '';
    this.branchFilter = '';
    this.load(1);
  }

  load(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');

    if (this.tab === 'tenant') {
      this.api.searchTenantSettings(this.search, page, 20).subscribe({
        next: (r) => {
          this.tenantSettings.set(r.items);
          this.totalCount.set(r.totalCount);
          this.hasNext.set(r.hasNext);
          this.loading.set(false);
        },
        error: (err: unknown) => this.fail(err),
      });
      return;
    }

    if (this.tab === 'branch') {
      const branchId = this.branchFilter === '' ? undefined : Number(this.branchFilter);
      this.api.searchBranchSettings(branchId, this.search, page, 20).subscribe({
        next: (r) => {
          this.branchSettings.set(r.items);
          this.totalCount.set(r.totalCount);
          this.hasNext.set(r.hasNext);
          this.loading.set(false);
        },
        error: (err: unknown) => this.fail(err),
      });
      return;
    }

    if (this.tab === 'reasons') {
      this.api
        .searchReasonCodes({
          page,
          pageSize: 20,
          search: this.search,
          reasonType: this.reasonTypeFilter ? String(this.reasonTypeFilter) : null,
        })
        .subscribe({
          next: (r) => {
            this.reasons.set(r.items);
            this.totalCount.set(r.totalCount);
            this.hasNext.set(r.hasNext);
            this.loading.set(false);
          },
          error: (err: unknown) => this.fail(err),
        });
      return;
    }

    this.api
      .searchAuditLogs({
        page,
        pageSize: 20,
        search: this.search,
        branchId: this.branchFilter === '' ? null : Number(this.branchFilter),
      })
      .subscribe({
        next: (r) => {
          this.audits.set(r.items);
          this.totalCount.set(r.totalCount);
          this.hasNext.set(r.hasNext);
          this.loading.set(false);
        },
        error: (err: unknown) => this.fail(err),
      });
  }

  branchCode(id: number): string {
    return this.branches().find((b) => b.id === id)?.code ?? String(id);
  }

  openUpsertTenant(row?: TenantSettingDto): void {
    this.modalKind = 'tenant';
    this.formKey = row?.settingKey ?? '';
    this.formValue = row?.settingValue ?? '';
    this.formEncrypted = row?.isEncrypted ?? false;
    this.modalOpen.set(true);
  }

  openUpsertBranch(row?: BranchSettingDto): void {
    this.modalKind = 'branch';
    this.formBranchId = row?.branchId ?? this.branches()[0]?.id ?? '';
    this.formKey = row?.settingKey ?? '';
    this.formValue = row?.settingValue ?? '';
    this.formEncrypted = row?.isEncrypted ?? false;
    this.modalOpen.set(true);
  }

  openCreateReason(): void {
    this.modalKind = 'reason';
    this.editingReasonId = null;
    this.formReasonType = 'Adjustment';
    this.formReasonCode = '';
    this.formReasonName = '';
    this.formReasonDescription = '';
    this.formReasonActive = true;
    this.modalOpen.set(true);
  }

  openEditReason(row: ReasonCodeDto): void {
    this.modalKind = 'reason';
    this.editingReasonId = row.id;
    this.formReasonType = row.reasonType;
    this.formReasonCode = row.code;
    this.formReasonName = row.name;
    this.formReasonDescription = row.description ?? '';
    this.formReasonActive = row.isActive;
    this.modalOpen.set(true);
  }

  closeModal(): void {
    this.modalOpen.set(false);
  }

  save(): void {
    this.saving.set(true);
    if (this.modalKind === 'tenant') {
      this.api
        .upsertTenantSetting({
          settingKey: this.formKey.trim(),
          settingValue: this.formValue,
          isEncrypted: this.formEncrypted,
        })
        .subscribe({
          next: () => this.afterSave('Tenant setting saved'),
          error: (err: unknown) => this.failSave(err),
        });
      return;
    }
    if (this.modalKind === 'branch') {
      this.api
        .upsertBranchSetting(Number(this.formBranchId), {
          settingKey: this.formKey.trim(),
          settingValue: this.formValue,
          isEncrypted: this.formEncrypted,
        })
        .subscribe({
          next: () => this.afterSave('Branch setting saved'),
          error: (err: unknown) => this.failSave(err),
        });
      return;
    }
    if (this.editingReasonId == null) {
      this.api
        .createReasonCode({
          reasonType: String(this.formReasonType),
          code: this.formReasonCode.trim(),
          name: this.formReasonName.trim(),
          description: this.formReasonDescription.trim() || null,
          isActive: this.formReasonActive,
        })
        .subscribe({
          next: () => this.afterSave('Reason code created'),
          error: (err: unknown) => this.failSave(err),
        });
      return;
    }
    this.api
      .updateReasonCode(this.editingReasonId, {
        name: this.formReasonName.trim(),
        description: this.formReasonDescription.trim() || null,
        isActive: this.formReasonActive,
      })
      .subscribe({
        next: () => this.afterSave('Reason code updated'),
        error: (err: unknown) => this.failSave(err),
      });
  }

  private afterSave(message: string): void {
    this.saving.set(false);
    this.modalOpen.set(false);
    this.snackbar.success(message);
    this.load(this.page());
  }

  private failSave(err: unknown): void {
    this.saving.set(false);
    this.fail(err);
  }

  private fail(err: unknown): void {
    this.loading.set(false);
    this.saving.set(false);
    const message = err instanceof Error ? err.message : 'Request failed.';
    this.error.set(message);
    this.snackbar.error(message);
  }
}
