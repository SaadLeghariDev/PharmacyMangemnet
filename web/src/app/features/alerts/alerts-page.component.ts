import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import { AlertDto, AlertRuleDto, BranchDto } from '../../core/models/api.models';
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
  selector: 'app-alerts-page',
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
  templateUrl: './alerts-page.component.html',
  styleUrl: './alerts-page.component.scss',
})
export class AlertsPageComponent implements OnInit {
  readonly alertColumns: AppTableColumn[] = [
    { key: 'severity', label: 'Severity' },
    { key: 'type', label: 'Type' },
    { key: 'title', label: 'Title' },
    { key: 'product', label: 'Product' },
    { key: 'status', label: 'Status' },
    { key: 'created', label: 'Created' },
    { key: 'actions', label: '' },
  ];

  readonly ruleColumns: AppTableColumn[] = [
    { key: 'type', label: 'Type' },
    { key: 'branch', label: 'Branch' },
    { key: 'threshold', label: 'Threshold' },
    { key: 'days', label: 'Days before expiry' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' },
  ];

  readonly statusOptions: AppSelectOption[] = [
    { value: '', label: 'All statuses' },
    { value: 'Open', label: 'Open' },
    { value: 'Acknowledged', label: 'Acknowledged' },
    { value: 'Resolved', label: 'Resolved' },
    { value: 'Dismissed', label: 'Dismissed' },
  ];

  readonly typeOptions: AppSelectOption[] = [
    { value: '', label: 'All types' },
    { value: 'LowStock', label: 'Low stock' },
    { value: 'Expiry', label: 'Expiry' },
  ];

  readonly severityOptions: AppSelectOption[] = [
    { value: '', label: 'All severities' },
    { value: 'Info', label: 'Info' },
    { value: 'Warning', label: 'Warning' },
    { value: 'Critical', label: 'Critical' },
  ];

  readonly activeOptions: AppSelectOption[] = [
    { value: '', label: 'All' },
    { value: 'true', label: 'Active' },
    { value: 'false', label: 'Inactive' },
  ];

  tab: 'alerts' | 'rules' = 'alerts';
  search = '';
  statusFilter: string | number = 'Open';
  typeFilter: string | number = '';
  severityFilter: string | number = '';
  branchFilter: string | number = '';
  branchOptions: AppSelectOption[] = [{ value: '', label: 'All branches' }];

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly evaluating = signal(false);
  readonly error = signal('');
  readonly alerts = signal<AlertDto[]>([]);
  readonly rules = signal<AlertRuleDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);

  readonly modalOpen = signal(false);
  readonly saving = signal(false);
  editingRuleId: number | null = null;

  formBranchId: string | number = '';
  formAlertType: string | number = 'LowStock';
  formThreshold: number | null = null;
  formDaysBeforeExpiry: number | null = 30;
  formIsActive = true;
  formErrors: Record<string, string> = {};

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.api.listBranches().subscribe({
      next: (branches: BranchDto[]) => {
        this.branchOptions = [
          { value: '', label: 'All branches' },
          ...branches.map((b) => ({ value: b.id, label: `${b.code} — ${b.name}` })),
        ];
      },
      error: (err: unknown) => this.fail(err),
    });
    this.load(1);
  }

  setTab(tab: 'alerts' | 'rules'): void {
    this.tab = tab;
    this.load(1);
  }

  load(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    const branchId = this.branchFilter === '' ? null : Number(this.branchFilter);

    if (this.tab === 'rules') {
      this.api
        .searchAlertRules({
          page,
          pageSize: 20,
          search: this.search,
          branchId,
          alertType: this.typeFilter === '' ? null : String(this.typeFilter),
          isActive: this.statusFilter === '' ? null : this.statusFilter === 'true',
        })
        .subscribe({
          next: (result) => {
            this.rules.set(result.items);
            this.totalCount.set(result.totalCount);
            this.hasNext.set(result.hasNext);
            this.loading.set(false);
          },
          error: (err: unknown) => this.fail(err),
        });
      return;
    }

    this.api
      .searchAlerts({
        page,
        pageSize: 20,
        search: this.search,
        branchId,
        status: this.statusFilter === '' ? null : String(this.statusFilter),
        severity: this.severityFilter === '' ? null : String(this.severityFilter),
        alertType: this.typeFilter === '' ? null : String(this.typeFilter),
      })
      .subscribe({
        next: (result) => {
          this.alerts.set(result.items);
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
    this.statusFilter = this.tab === 'alerts' ? 'Open' : '';
    this.typeFilter = '';
    this.severityFilter = '';
    this.branchFilter = '';
    this.load(1);
  }

  evaluate(): void {
    this.evaluating.set(true);
    this.api.evaluateAlerts().subscribe({
      next: (result) => {
        this.evaluating.set(false);
        this.snackbar.success(
          `Evaluated ${result.rulesScanned} rule(s); created ${result.alertsCreated} alert(s).`,
        );
        this.tab = 'alerts';
        this.statusFilter = 'Open';
        this.load(1);
      },
      error: (err: unknown) => {
        this.evaluating.set(false);
        this.fail(err);
      },
    });
  }

  acknowledge(row: AlertDto): void {
    this.api.acknowledgeAlert(row.id).subscribe({
      next: () => {
        this.snackbar.success('Alert acknowledged.');
        this.load(this.page());
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  resolve(row: AlertDto): void {
    this.api.resolveAlert(row.id).subscribe({
      next: () => {
        this.snackbar.success('Alert resolved.');
        this.load(this.page());
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  openCreateRule(): void {
    this.editingRuleId = null;
    this.formBranchId = '';
    this.formAlertType = 'LowStock';
    this.formThreshold = null;
    this.formDaysBeforeExpiry = 30;
    this.formIsActive = true;
    this.formErrors = {};
    this.modalOpen.set(true);
  }

  openEditRule(row: AlertRuleDto): void {
    this.editingRuleId = row.id;
    this.formBranchId = row.branchId ?? '';
    this.formAlertType = row.alertType;
    this.formThreshold = row.threshold ?? null;
    this.formDaysBeforeExpiry = row.daysBeforeExpiry ?? null;
    this.formIsActive = row.isActive;
    this.formErrors = {};
    this.modalOpen.set(true);
  }

  closeModal(): void {
    if (this.saving()) return;
    this.modalOpen.set(false);
  }

  saveRule(): void {
    this.formErrors = {};
    const alertType = String(this.formAlertType);
    if (!alertType) this.formErrors['type'] = 'Alert type is required.';
    if (alertType === 'Expiry' && (this.formDaysBeforeExpiry == null || this.formDaysBeforeExpiry < 0)) {
      this.formErrors['days'] = 'Days before expiry is required.';
    }
    if (Object.keys(this.formErrors).length) return;

    const payload = {
      branchId: this.formBranchId === '' ? null : Number(this.formBranchId),
      alertType,
      threshold: this.formThreshold,
      daysBeforeExpiry: alertType === 'Expiry' ? this.formDaysBeforeExpiry : null,
      isActive: this.formIsActive,
    };

    this.saving.set(true);
    const req$ =
      this.editingRuleId == null
        ? this.api.createAlertRule(payload)
        : this.api.updateAlertRule(this.editingRuleId, payload);

    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.modalOpen.set(false);
        this.snackbar.success(this.editingRuleId == null ? 'Alert rule created.' : 'Alert rule updated.');
        this.tab = 'rules';
        this.load(this.page());
      },
      error: (err: unknown) => {
        this.saving.set(false);
        this.fail(err);
      },
    });
  }

  severityTone(severity: string): 'info' | 'warning' | 'danger' | 'success' {
    if (severity === 'Critical') return 'danger';
    if (severity === 'Warning') return 'warning';
    return 'info';
  }

  statusTone(status: string): 'info' | 'warning' | 'danger' | 'success' {
    if (status === 'Open') return 'warning';
    if (status === 'Resolved') return 'success';
    if (status === 'Dismissed') return 'danger';
    return 'info';
  }

  private fail(err: unknown): void {
    const message = err instanceof Error ? err.message : 'Request failed';
    this.error.set(message);
    this.loading.set(false);
    this.snackbar.error(message);
  }
}
