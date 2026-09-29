import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { map, Observable } from 'rxjs';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import { CreateCustomerRequest, CustomerDto } from '../../core/models/api.models';
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
  AppTypeaheadComponent,
  AppTypeaheadItem,
  SnackbarService,
} from '../../shared';

@Component({
  selector: 'app-customers-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    CurrencyPipe,
    AppPageHeaderComponent,
    AppButtonComponent,
    AppInputComponent,
    AppSelectComponent,
    AppTableComponent,
    AppEmptyStateComponent,
    AppLoadingStateComponent,
    AppModalComponent,
    AppBadgeComponent,
    AppTypeaheadComponent,
  ],
  templateUrl: './customers-page.component.html',
  styleUrl: './customers-page.component.scss',
})
export class CustomersPageComponent implements OnInit {
  readonly columns: AppTableColumn[] = [
    { key: 'code', label: 'Code' },
    { key: 'name', label: 'Name' },
    { key: 'phone', label: 'Phone' },
    { key: 'balance', label: 'Balance' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: 'Actions' },
  ];

  readonly statusOptions: AppSelectOption[] = [
    { value: '', label: 'All' },
    { value: 'active', label: 'Active' },
    { value: 'inactive', label: 'Inactive' },
  ];

  search = '';
  statusFilter: string | number = '';

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<CustomerDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);

  readonly createOpen = signal(false);
  readonly saving = signal(false);
  createErrors: Record<string, string> = {};
  createName = '';
  createPhone = '';
  createCreditLimit: number | null = 0;
  createIsPatient = false;

  readonly customerSuggestFn = (q: string): Observable<AppTypeaheadItem<CustomerDto>[]> =>
    this.api.searchCustomers({ search: q, pageSize: 8 }).pipe(
      map((r) =>
        r.items.map((c) => ({
          id: c.id,
          label: c.name,
          detail: [c.customerCode, c.phone].filter(Boolean).join(' · '),
          data: c,
        })),
      ),
    );

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.load(1);
  }

  load(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    let isActive: boolean | null = null;
    if (this.statusFilter === 'active') isActive = true;
    if (this.statusFilter === 'inactive') isActive = false;

    this.api
      .searchCustomers({ page, pageSize: 20, search: this.search, isActive })
      .subscribe({
        next: (r) => {
          this.rows.set(r.items);
          this.totalCount.set(r.totalCount);
          this.hasNext.set(r.hasNext);
          this.loading.set(false);
        },
        error: (err: unknown) => this.fail(err),
      });
  }

  applyFilters(): void {
    this.load(1);
  }

  onCustomerSuggest(item: AppTypeaheadItem<CustomerDto>): void {
    this.search = item.data?.customerCode || item.label;
    this.applyFilters();
  }

  onSearchSubmit(q: string): void {
    this.search = q;
    this.applyFilters();
  }

  openCreate(): void {
    this.createErrors = {};
    this.createName = '';
    this.createPhone = '';
    this.createCreditLimit = 0;
    this.createIsPatient = false;
    this.createOpen.set(true);
  }

  closeCreate(): void {
    if (!this.saving()) this.createOpen.set(false);
  }

  submitCreate(): void {
    this.createErrors = {};
    if (!this.createName.trim()) this.createErrors['name'] = 'Name is required.';
    if (Object.keys(this.createErrors).length) return;

    const body: CreateCustomerRequest = {
      name: this.createName.trim(),
      phone: this.createPhone.trim() || null,
      creditLimit: Number(this.createCreditLimit ?? 0),
      isPatient: this.createIsPatient,
    };
    this.saving.set(true);
    this.api.createCustomer(body).subscribe({
      next: (c) => {
        this.saving.set(false);
        this.createOpen.set(false);
        this.snackbar.success(`Customer ${c.customerCode} created.`);
        this.load(1);
      },
      error: (err: unknown) => {
        this.saving.set(false);
        this.snackbar.error(err instanceof Error ? err.message : 'Could not create customer.');
      },
    });
  }

  private fail(err: unknown): void {
    this.loading.set(false);
    const message = err instanceof Error ? err.message : 'Could not load customers.';
    this.error.set(message);
    this.snackbar.error(message);
  }
}
