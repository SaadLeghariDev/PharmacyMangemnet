import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  BranchDto,
  IdempotencyKeyDto,
  PosTerminalDto,
  SyncBatchDto,
  SyncNodeDto,
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
  selector: 'app-sync-page',
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
  templateUrl: './sync-page.component.html',
  styleUrl: './sync-page.component.scss',
})
export class SyncPageComponent implements OnInit {
  tab: 'nodes' | 'batches' | 'keys' = 'nodes';

  readonly nodeColumns: AppTableColumn[] = [
    { key: 'code', label: 'Node' },
    { key: 'branch', label: 'Branch' },
    { key: 'terminal', label: 'Terminal' },
    { key: 'seq', label: 'Last sequence' },
    { key: 'synced', label: 'Last sync' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' },
  ];

  readonly batchColumns: AppTableColumn[] = [
    { key: 'batch', label: 'Batch' },
    { key: 'node', label: 'Node' },
    { key: 'started', label: 'Started' },
    { key: 'items', label: 'Items' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' },
  ];

  readonly keyColumns: AppTableColumn[] = [
    { key: 'key', label: 'Key' },
    { key: 'terminal', label: 'Terminal' },
    { key: 'entity', label: 'Entity' },
    { key: 'entityId', label: 'Entity ID' },
    { key: 'created', label: 'Created' },
  ];

  readonly activeOptions: AppSelectOption[] = [
    { value: '', label: 'All' },
    { value: 'true', label: 'Active' },
    { value: 'false', label: 'Inactive' },
  ];

  readonly batchStatusOptions: AppSelectOption[] = [
    { value: '', label: 'All statuses' },
    { value: 'InProgress', label: 'InProgress' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Failed', label: 'Failed' },
    { value: 'Partial', label: 'Partial' },
  ];

  readonly operationOptions: AppSelectOption[] = [
    { value: 'Insert', label: 'Insert' },
    { value: 'Update', label: 'Update' },
    { value: 'Delete', label: 'Delete' },
  ];

  search = '';
  branchFilter: string | number = '';
  activeFilter: string | number = '';
  nodeFilter: string | number = '';
  statusFilter: string | number = '';
  terminalFilter: string | number = '';
  entityTypeFilter = '';

  branchOptions: AppSelectOption[] = [{ value: '', label: 'All branches' }];
  branchPickOptions: AppSelectOption[] = [];
  terminalPickOptions: AppSelectOption[] = [];
  nodeOptions: AppSelectOption[] = [{ value: '', label: 'All nodes' }];
  nodePickOptions: AppSelectOption[] = [];

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly hasNext = signal(false);
  readonly totalCount = signal(0);
  readonly nodes = signal<SyncNodeDto[]>([]);
  readonly batches = signal<SyncBatchDto[]>([]);
  readonly keys = signal<IdempotencyKeyDto[]>([]);
  readonly branches = signal<BranchDto[]>([]);
  readonly terminals = signal<PosTerminalDto[]>([]);

  modalOpen = false;
  modalMode: 'register' | 'push' | 'detail' = 'register';
  modalTitle = '';
  saving = false;
  detailBatch: SyncBatchDto | null = null;

  nodeForm = { branchId: 0, terminalId: 0, nodeCode: '' };
  pushForm = {
    syncNodeId: 0,
    entityName: 'Sale',
    entityId: 1,
    operation: 'Insert',
    payload: '{"source":"offline"}',
    version: 1,
    simulateFailures: false,
  };

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snack: SnackbarService,
  ) {}

  ngOnInit(): void {
    this.api.listBranches().subscribe({
      next: (rows) => {
        this.branches.set(rows);
        this.branchOptions = [
          { value: '', label: 'All branches' },
          ...rows.map((b) => ({ value: b.id, label: `${b.code} — ${b.name}` })),
        ];
        this.branchPickOptions = rows.map((b) => ({ value: b.id, label: `${b.code} — ${b.name}` }));
      },
    });
    this.refreshNodeOptions();
    this.load(1);
  }

  setTab(tab: 'nodes' | 'batches' | 'keys'): void {
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
    this.branchFilter = '';
    this.activeFilter = '';
    this.nodeFilter = '';
    this.statusFilter = '';
    this.terminalFilter = '';
    this.entityTypeFilter = '';
    this.load(1);
  }

  load(page: number): void {
    this.loading.set(true);
    this.error.set('');
    this.page.set(page);

    if (this.tab === 'nodes') {
      this.api
        .searchSyncNodes({
          page,
          pageSize: 20,
          search: this.search || undefined,
          branchId: this.branchFilter === '' ? null : Number(this.branchFilter),
          isActive:
            this.activeFilter === ''
              ? null
              : String(this.activeFilter) === 'true',
        })
        .subscribe({
          next: (res) => {
            this.nodes.set(res.items);
            this.hasNext.set(res.hasNext);
            this.totalCount.set(res.totalCount);
            this.loading.set(false);
            this.refreshNodeOptions();
          },
          error: (err) => this.fail(err),
        });
      return;
    }

    if (this.tab === 'batches') {
      this.api
        .searchSyncBatches({
          page,
          pageSize: 20,
          search: this.search || undefined,
          syncNodeId: this.nodeFilter === '' ? null : Number(this.nodeFilter),
          status: this.statusFilter === '' ? null : String(this.statusFilter),
        })
        .subscribe({
          next: (res) => {
            this.batches.set(res.items);
            this.hasNext.set(res.hasNext);
            this.totalCount.set(res.totalCount);
            this.loading.set(false);
          },
          error: (err) => this.fail(err),
        });
      return;
    }

    this.api
      .searchIdempotencyKeys({
        page,
        pageSize: 20,
        search: this.search || undefined,
        terminalId: this.terminalFilter === '' ? null : Number(this.terminalFilter),
        entityType: this.entityTypeFilter || undefined,
      })
      .subscribe({
        next: (res) => {
          this.keys.set(res.items);
          this.hasNext.set(res.hasNext);
          this.totalCount.set(res.totalCount);
          this.loading.set(false);
        },
        error: (err) => this.fail(err),
      });
  }

  openRegister(): void {
    this.modalMode = 'register';
    this.modalTitle = 'Register sync node';
    this.nodeForm = {
      branchId: this.branchPickOptions[0] ? Number(this.branchPickOptions[0].value) : 0,
      terminalId: 0,
      nodeCode: '',
    };
    this.onBranchPicked();
    this.modalOpen = true;
  }

  openPush(): void {
    this.modalMode = 'push';
    this.modalTitle = 'Push sync batch';
    this.pushForm = {
      syncNodeId: this.nodePickOptions[0] ? Number(this.nodePickOptions[0].value) : 0,
      entityName: 'Sale',
      entityId: 1,
      operation: 'Insert',
      payload: '{"source":"offline"}',
      version: 1,
      simulateFailures: false,
    };
    this.modalOpen = true;
  }

  openBatchDetail(row: SyncBatchDto): void {
    this.modalMode = 'detail';
    this.modalTitle = `Batch ${row.batchNumber}`;
    this.detailBatch = null;
    this.modalOpen = true;
    this.api.getSyncBatch(row.id).subscribe({
      next: (batch) => (this.detailBatch = batch),
      error: (err) => {
        this.snack.error(err?.message || 'Failed to load batch');
        this.modalOpen = false;
      },
    });
  }

  onBranchPicked(): void {
    const branchId = Number(this.nodeForm.branchId) || 0;
    if (!branchId) {
      this.terminalPickOptions = [];
      this.nodeForm.terminalId = 0;
      return;
    }
    this.api.listTerminals(branchId).subscribe({
      next: (rows) => {
        this.terminals.set(rows);
        this.terminalPickOptions = rows.map((t) => ({
          value: t.id,
          label: t.terminalCode,
        }));
        this.nodeForm.terminalId = rows[0]?.id ?? 0;
      },
    });
  }

  saveModal(): void {
    if (this.modalMode === 'register') {
      if (!this.nodeForm.nodeCode.trim() || !this.nodeForm.branchId || !this.nodeForm.terminalId) {
        this.snack.error('Node code, branch, and terminal are required.');
        return;
      }
      this.saving = true;
      this.api
        .registerSyncNode({
          branchId: Number(this.nodeForm.branchId),
          terminalId: Number(this.nodeForm.terminalId),
          nodeCode: this.nodeForm.nodeCode.trim(),
        })
        .subscribe({
          next: () => {
            this.saving = false;
            this.modalOpen = false;
            this.snack.success('Sync node registered');
            this.load(1);
          },
          error: (err) => {
            this.saving = false;
            this.snack.error(err?.message || 'Register failed');
          },
        });
      return;
    }

    if (this.modalMode === 'push') {
      if (!this.pushForm.syncNodeId || !this.pushForm.entityName.trim()) {
        this.snack.error('Node and entity name are required.');
        return;
      }
      this.saving = true;
      this.api
        .syncPush({
          syncNodeId: Number(this.pushForm.syncNodeId),
          autoComplete: true,
          simulateFailures: this.pushForm.simulateFailures,
          items: [
            {
              entityName: this.pushForm.entityName.trim(),
              entityId: Number(this.pushForm.entityId) || 1,
              operation: String(this.pushForm.operation),
              payload: this.pushForm.payload || null,
              version: Number(this.pushForm.version) || 1,
            },
          ],
        })
        .subscribe({
          next: (res) => {
            this.saving = false;
            this.modalOpen = false;
            this.snack.success(`Push ${res.batch.batchNumber} → ${res.batch.status}`);
            this.tab = 'batches';
            this.load(1);
          },
          error: (err) => {
            this.saving = false;
            this.snack.error(err?.message || 'Push failed');
          },
        });
    }
  }

  pull(node: SyncNodeDto): void {
    this.api.syncPull({ syncNodeId: node.id }).subscribe({
      next: (res) => {
        this.snack.success(
          `Pull ${res.batch.batchNumber}: ${res.batch.itemCount} item(s), seq ${res.node.lastSequence}`,
        );
        this.tab = 'batches';
        this.load(1);
      },
      error: (err) => this.snack.error(err?.message || 'Pull failed'),
    });
  }

  toggleActive(node: SyncNodeDto): void {
    this.api.updateSyncNode(node.id, { isActive: !node.isActive }).subscribe({
      next: () => {
        this.snack.success(node.isActive ? 'Node deactivated' : 'Node activated');
        this.load(this.page());
      },
      error: (err) => this.snack.error(err?.message || 'Update failed'),
    });
  }

  batchTone(status: string): 'success' | 'warning' | 'danger' | 'info' | 'neutral' {
    switch (status) {
      case 'Completed':
        return 'success';
      case 'Partial':
        return 'warning';
      case 'Failed':
        return 'danger';
      case 'InProgress':
        return 'info';
      default:
        return 'neutral';
    }
  }

  itemTone(status: string): 'success' | 'warning' | 'danger' | 'info' | 'neutral' {
    switch (status) {
      case 'Processed':
        return 'success';
      case 'Skipped':
        return 'warning';
      case 'Failed':
        return 'danger';
      case 'Pending':
        return 'info';
      default:
        return 'neutral';
    }
  }

  private refreshNodeOptions(): void {
    this.api.searchSyncNodes({ page: 1, pageSize: 100, isActive: true }).subscribe({
      next: (res) => {
        this.nodeOptions = [
          { value: '', label: 'All nodes' },
          ...res.items.map((n) => ({ value: n.id, label: n.nodeCode })),
        ];
        this.nodePickOptions = res.items.map((n) => ({ value: n.id, label: n.nodeCode }));
      },
    });
  }

  private fail(err: { message?: string }): void {
    this.loading.set(false);
    this.error.set(err?.message || 'Failed to load');
  }
}
