import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import { SupplierDto } from '../../core/models/api.models';
import {
  AppBadgeComponent,
  AppButtonComponent,
  AppEmptyStateComponent,
  AppInputComponent,
  AppLoadingStateComponent,
  AppPageHeaderComponent,
  AppTableColumn,
  AppTableComponent,
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
    AppBadgeComponent,
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
}
