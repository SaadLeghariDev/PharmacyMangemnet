import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { map, Observable } from 'rxjs';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import { CreateSupplierRequest, SupplierDto } from '../../core/models/api.models';
import {
  AppBadgeComponent,
  AppButtonComponent,
  AppEmptyStateComponent,
  AppInputComponent,
  AppLoadingStateComponent,
  AppModalComponent,
  AppPageHeaderComponent,
  AppTableColumn,
  AppTableComponent,
  AppTypeaheadComponent,
  AppTypeaheadItem,
  SnackbarService,
} from '../../shared';

@Component({
  selector: 'app-suppliers-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    AppPageHeaderComponent,
    AppButtonComponent,
    AppInputComponent,
    AppTableComponent,
    AppEmptyStateComponent,
    AppLoadingStateComponent,
    AppModalComponent,
    AppBadgeComponent,
    AppTypeaheadComponent,
  ],
  templateUrl: './suppliers-page.component.html',
  styleUrl: './suppliers-page.component.scss',
})
export class SuppliersPageComponent implements OnInit {
  readonly columns: AppTableColumn[] = [
    { key: 'code', label: 'Code' },
    { key: 'name', label: 'Name' },
    { key: 'phone', label: 'Phone' },
    { key: 'terms', label: 'Terms (days)' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: 'Actions' },
  ];

  search = '';
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<SupplierDto[]>([]);

  readonly createOpen = signal(false);
  readonly saving = signal(false);
  createErrors: Record<string, string> = {};
  createCode = '';
  createName = '';
  createPhone = '';
  createTermsDays: number | null = 30;
  createCreditLimit: number | null = 0;

  readonly supplierSuggestFn = (q: string): Observable<AppTypeaheadItem<SupplierDto>[]> =>
    this.api.searchSuppliers(q, 8).pipe(
      map((items) =>
        items.map((s) => ({
          id: s.id,
          label: s.name,
          detail: [s.code, s.phone].filter(Boolean).join(' · '),
          data: s,
        })),
      ),
    );

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
    this.api.searchSuppliers(this.search).subscribe({
      next: (items) => {
        this.rows.set(items);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.loading.set(false);
        const message =
          err instanceof Error ? err.message : 'Could not load suppliers.';
        this.error.set(message);
        this.snackbar.error(message);
      },
    });
  }

  onSupplierSuggest(item: AppTypeaheadItem<SupplierDto>): void {
    this.search = item.data?.code || item.label;
    this.load();
  }

  onSearchSubmit(q: string): void {
    this.search = q;
    this.load();
  }

  openCreate(): void {
    this.createErrors = {};
    this.createCode = '';
    this.createName = '';
    this.createPhone = '';
    this.createTermsDays = 30;
    this.createCreditLimit = 0;
    this.createOpen.set(true);
  }

  closeCreate(): void {
    if (!this.saving()) this.createOpen.set(false);
  }

  submitCreate(): void {
    this.createErrors = {};
    if (!this.createCode.trim()) this.createErrors['code'] = 'Code is required.';
    if (!this.createName.trim()) this.createErrors['name'] = 'Name is required.';
    if (Object.keys(this.createErrors).length) return;

    const body: CreateSupplierRequest = {
      code: this.createCode.trim(),
      name: this.createName.trim(),
      phone: this.createPhone.trim() || null,
      paymentTermsDays: Number(this.createTermsDays ?? 0),
      creditLimit: Number(this.createCreditLimit ?? 0),
    };
    this.saving.set(true);
    this.api.createSupplier(body).subscribe({
      next: () => {
        this.saving.set(false);
        this.createOpen.set(false);
        this.snackbar.success('Supplier created.');
        this.load();
      },
      error: (err: unknown) => {
        this.saving.set(false);
        this.snackbar.error(err instanceof Error ? err.message : 'Could not create supplier.');
      },
    });
  }
}
