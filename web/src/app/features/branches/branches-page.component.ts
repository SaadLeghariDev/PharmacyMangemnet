import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import { BranchDto } from '../../core/models/api.models';
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
  selector: 'app-branches-page',
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
  templateUrl: './branches-page.component.html',
  styleUrl: './branches-page.component.scss',
})
export class BranchesPageComponent implements OnInit {
  readonly columns: AppTableColumn[] = [
    { key: 'name', label: 'Branch' },
    { key: 'location', label: 'Location' },
    { key: 'type', label: 'Type' },
    { key: 'contact', label: 'Contact' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' },
  ];

  readonly activeOptions: AppSelectOption[] = [
    { value: '', label: 'All statuses' },
    { value: 'true', label: 'Active' },
    { value: 'false', label: 'Inactive' },
  ];

  search = '';
  activeFilter: string | number = '';
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<BranchDto[]>([]);
  readonly all = signal<BranchDto[]>([]);

  readonly modalOpen = signal(false);
  readonly saving = signal(false);
  editingId: number | null = null;

  formCode = '';
  formName = '';
  formBranchType = 'Retail';
  formCity = '';
  formProvince = '';
  formPhone = '';
  formEmail = '';
  formAddress = '';
  formActive = true;

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.api.listBranches().subscribe({
      next: (items) => {
        this.all.set(items);
        this.applyLocalFilter();
        this.loading.set(false);
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  applyFilters(): void {
    this.applyLocalFilter();
  }

  clearFilters(): void {
    this.search = '';
    this.activeFilter = '';
    this.applyLocalFilter();
  }

  openCreate(): void {
    this.editingId = null;
    this.formCode = '';
    this.formName = '';
    this.formBranchType = 'Retail';
    this.formCity = '';
    this.formProvince = '';
    this.formPhone = '';
    this.formEmail = '';
    this.formAddress = '';
    this.formActive = true;
    this.modalOpen.set(true);
  }

  openEdit(row: BranchDto): void {
    this.editingId = row.id;
    this.formCode = row.code;
    this.formName = row.name;
    this.formBranchType = row.branchType || 'Retail';
    this.formCity = row.city || '';
    this.formProvince = row.province || '';
    this.formPhone = row.phone || '';
    this.formEmail = row.email || '';
    this.formAddress = row.address || '';
    this.formActive = row.isActive;
    this.modalOpen.set(true);
  }

  closeModal(): void {
    this.modalOpen.set(false);
  }

  save(): void {
    this.saving.set(true);
    if (this.editingId == null) {
      this.api
        .createBranch({
          code: this.formCode.trim(),
          name: this.formName.trim(),
          branchType: this.formBranchType.trim() || null,
          city: this.formCity.trim() || null,
          province: this.formProvince.trim() || null,
          phone: this.formPhone.trim() || null,
          email: this.formEmail.trim() || null,
          address: this.formAddress.trim() || null,
        })
        .subscribe({
          next: () => this.afterSave('Branch created'),
          error: (err: unknown) => this.failSave(err),
        });
      return;
    }
    this.api
      .updateBranch(this.editingId, {
        name: this.formName.trim(),
        branchType: this.formBranchType.trim() || null,
        city: this.formCity.trim() || null,
        province: this.formProvince.trim() || null,
        phone: this.formPhone.trim() || null,
        email: this.formEmail.trim() || null,
        address: this.formAddress.trim() || null,
        isActive: this.formActive,
      })
      .subscribe({
        next: () => this.afterSave('Branch updated'),
        error: (err: unknown) => this.failSave(err),
      });
  }

  deactivate(row: BranchDto): void {
    this.api.deactivateBranch(row.id).subscribe({
      next: () => {
        this.snackbar.success('Branch deactivated');
        this.load();
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  private applyLocalFilter(): void {
    let items = this.all();
    const s = this.search.trim().toLowerCase();
    if (s) {
      items = items.filter(
        (b) =>
          b.name.toLowerCase().includes(s) ||
          b.code.toLowerCase().includes(s) ||
          (b.city || '').toLowerCase().includes(s),
      );
    }
    if (this.activeFilter === 'true') items = items.filter((b) => b.isActive);
    if (this.activeFilter === 'false') items = items.filter((b) => !b.isActive);
    this.rows.set(items);
  }

  private afterSave(message: string): void {
    this.saving.set(false);
    this.modalOpen.set(false);
    this.snackbar.success(message);
    this.load();
  }

  private failSave(err: unknown): void {
    this.saving.set(false);
    this.fail(err);
  }

  private fail(err: unknown): void {
    this.loading.set(false);
    this.saving.set(false);
    const message = err instanceof Error ? err.message : 'Could not load branches.';
    this.error.set(message);
    this.snackbar.error(message);
  }
}
