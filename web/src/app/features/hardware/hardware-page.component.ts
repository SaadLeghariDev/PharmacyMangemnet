import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { map, Observable } from 'rxjs';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  AttachmentDto,
  BarcodePrintJobDto,
  BranchDto,
  DeviceDto,
  DeviceTypeDto,
  PrintTemplateDto,
  ProductDto,
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
  AppTypeaheadComponent,
  AppTypeaheadItem,
  SnackbarService,
} from '../../shared';

@Component({
  selector: 'app-hardware-page',
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
    AppTypeaheadComponent,
    AppBadgeComponent,
  ],
  templateUrl: './hardware-page.component.html',
  styleUrl: './hardware-page.component.scss',
})
export class HardwarePageComponent implements OnInit {
  tab: 'devices' | 'templates' | 'jobs' | 'attachments' = 'devices';

  readonly deviceColumns: AppTableColumn[] = [
    { key: 'name', label: 'Device' },
    { key: 'type', label: 'Type' },
    { key: 'branch', label: 'Branch' },
    { key: 'conn', label: 'Connection' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' },
  ];

  readonly templateColumns: AppTableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'type', label: 'Type' },
    { key: 'width', label: 'Paper width' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' },
  ];

  readonly jobColumns: AppTableColumn[] = [
    { key: 'id', label: 'Job' },
    { key: 'product', label: 'Product' },
    { key: 'printer', label: 'Printer' },
    { key: 'qty', label: 'Qty' },
    { key: 'status', label: 'Status' },
    { key: 'created', label: 'Created' },
    { key: 'actions', label: '' },
  ];

  readonly attachmentColumns: AppTableColumn[] = [
    { key: 'file', label: 'File' },
    { key: 'path', label: 'Storage path / URL' },
    { key: 'size', label: 'Size' },
    { key: 'created', label: 'Created' },
    { key: 'actions', label: '' },
  ];

  readonly activeOptions: AppSelectOption[] = [
    { value: '', label: 'All' },
    { value: 'true', label: 'Active' },
    { value: 'false', label: 'Inactive' },
  ];

  readonly jobStatusOptions: AppSelectOption[] = [
    { value: '', label: 'All statuses' },
    { value: 'Queued', label: 'Queued' },
    { value: 'Printing', label: 'Printing' },
    { value: 'Printed', label: 'Printed' },
    { value: 'Failed', label: 'Failed' },
    { value: 'Cancelled', label: 'Cancelled' },
  ];

  readonly templateTypeOptions: AppSelectOption[] = [
    { value: 'Barcode', label: 'Barcode' },
    { value: 'Receipt', label: 'Receipt' },
    { value: 'Label', label: 'Label' },
  ];

  search = '';
  branchFilter: string | number = '';
  typeFilter: string | number = '';
  activeFilter: string | number = '';
  statusFilter: string | number = '';
  branchOptions: AppSelectOption[] = [{ value: '', label: 'All branches' }];
  branchPickOptions: AppSelectOption[] = [];
  typeOptions: AppSelectOption[] = [{ value: '', label: 'All types' }];
  typePickOptions: AppSelectOption[] = [];
  deviceOptions: AppSelectOption[] = [];
  templateOptions: AppSelectOption[] = [];
  productOptions: AppSelectOption[] = [];

  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly hasNext = signal(false);
  readonly totalCount = signal(0);
  readonly devices = signal<DeviceDto[]>([]);
  readonly templates = signal<PrintTemplateDto[]>([]);
  readonly jobs = signal<BarcodePrintJobDto[]>([]);
  readonly attachments = signal<AttachmentDto[]>([]);
  readonly deviceTypes = signal<DeviceTypeDto[]>([]);
  readonly branches = signal<BranchDto[]>([]);

  modalOpen = false;
  modalTitle = '';
  saving = false;
  editingDeviceId: number | null = null;
  editingTemplateId: number | null = null;

  deviceForm = this.emptyDeviceForm();
  templateForm = this.emptyTemplateForm();
  jobForm = this.emptyJobForm();
  attachmentForm = this.emptyAttachmentForm();
  linkForm = { attachmentId: 0, entityName: '', entityId: 0 };
  linkModalOpen = false;

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
    this.api.listDeviceTypes().subscribe({
      next: (page) => {
        this.deviceTypes.set(page.items);
        this.typeOptions = [
          { value: '', label: 'All types' },
          ...page.items.map((t) => ({ value: t.id, label: `${t.code} — ${t.name}` })),
        ];
        this.typePickOptions = page.items.map((t) => ({ value: t.id, label: `${t.code} — ${t.name}` }));
      },
    });
    this.load(1);
  }

  setTab(tab: typeof this.tab): void {
    this.tab = tab;
    this.search = '';
    this.statusFilter = '';
    this.load(1);
  }

  applyFilters(): void {
    this.load(1);
  }

  clearFilters(): void {
    this.search = '';
    this.branchFilter = '';
    this.typeFilter = '';
    this.activeFilter = '';
    this.statusFilter = '';
    this.load(1);
  }

  load(page = 1): void {
    this.loading.set(true);
    this.error.set('');
    this.page.set(page);

    if (this.tab === 'devices') {
      this.api
        .searchDevices({
          page,
          pageSize: 20,
          search: this.search || undefined,
          branchId: this.numOrNull(this.branchFilter),
          deviceTypeId: this.numOrNull(this.typeFilter),
          isActive: this.boolOrNull(this.activeFilter),
        })
        .subscribe({
          next: (r) => {
            this.devices.set(r.items);
            this.hasNext.set(r.hasNext);
            this.totalCount.set(r.totalCount);
            this.deviceOptions = r.items.map((d) => ({ value: d.id, label: d.name }));
            this.loading.set(false);
          },
          error: (e) => this.fail(e),
        });
      return;
    }

    if (this.tab === 'templates') {
      this.api
        .searchPrintTemplates({
          page,
          pageSize: 20,
          search: this.search || undefined,
          isActive: this.boolOrNull(this.activeFilter),
        })
        .subscribe({
          next: (r) => {
            this.templates.set(r.items);
            this.hasNext.set(r.hasNext);
            this.totalCount.set(r.totalCount);
            this.templateOptions = r.items.map((t) => ({ value: t.id, label: t.name }));
            this.loading.set(false);
          },
          error: (e) => this.fail(e),
        });
      return;
    }

    if (this.tab === 'jobs') {
      this.api
        .searchBarcodePrintJobs({
          page,
          pageSize: 20,
          branchId: this.numOrNull(this.branchFilter),
          status: (this.statusFilter as string) || null,
        })
        .subscribe({
          next: (r) => {
            this.jobs.set(r.items);
            this.hasNext.set(r.hasNext);
            this.totalCount.set(r.totalCount);
            this.loading.set(false);
          },
          error: (e) => this.fail(e),
        });
      return;
    }

    this.api.searchAttachments(this.search || undefined, page, 20).subscribe({
      next: (r) => {
        this.attachments.set(r.items);
        this.hasNext.set(r.hasNext);
        this.totalCount.set(r.totalCount);
        this.loading.set(false);
      },
      error: (e) => this.fail(e),
    });
  }

  openCreate(): void {
    if (this.tab === 'devices') {
      this.editingDeviceId = null;
      this.deviceForm = this.emptyDeviceForm();
      if (this.branches().length) this.deviceForm.branchId = this.branches()[0].id;
      if (this.deviceTypes().length) this.deviceForm.deviceTypeId = this.deviceTypes()[0].id;
      this.modalTitle = 'New device';
      this.modalOpen = true;
      return;
    }
    if (this.tab === 'templates') {
      this.editingTemplateId = null;
      this.templateForm = this.emptyTemplateForm();
      this.modalTitle = 'New print template';
      this.modalOpen = true;
      return;
    }
    if (this.tab === 'jobs') {
      this.jobForm = this.emptyJobForm();
      if (this.branches().length) this.jobForm.branchId = this.branches()[0].id;
      this.refreshJobLookups();
      this.modalTitle = 'Queue barcode print job';
      this.modalOpen = true;
      return;
    }
    this.attachmentForm = this.emptyAttachmentForm();
    this.modalTitle = 'New attachment metadata';
    this.modalOpen = true;
  }

  openEditDevice(row: DeviceDto): void {
    this.editingDeviceId = row.id;
    this.deviceForm = {
      branchId: row.branchId,
      counterId: row.counterId ?? null,
      deviceTypeId: row.deviceTypeId,
      name: row.name,
      manufacturer: row.manufacturer ?? '',
      model: row.model ?? '',
      serialNumber: row.serialNumber ?? '',
      connectionType: row.connectionType ?? '',
      ip: row.ip ?? '',
      port: row.port ?? null,
      comPort: row.comPort ?? '',
      isDefault: row.isDefault,
      isActive: row.isActive,
    };
    this.modalTitle = 'Edit device';
    this.modalOpen = true;
  }

  openEditTemplate(row: PrintTemplateDto): void {
    this.editingTemplateId = row.id;
    this.templateForm = {
      templateType: row.templateType,
      name: row.name,
      templateContent: row.templateContent,
      paperWidth: row.paperWidth ?? null,
      isDefault: row.isDefault,
      isActive: row.isActive,
    };
    this.modalTitle = 'Edit print template';
    this.modalOpen = true;
  }

  save(): void {
    this.saving = true;
    if (this.tab === 'devices') {
      const body = {
        ...this.deviceForm,
        manufacturer: this.deviceForm.manufacturer || null,
        model: this.deviceForm.model || null,
        serialNumber: this.deviceForm.serialNumber || null,
        connectionType: this.deviceForm.connectionType || null,
        ip: this.deviceForm.ip || null,
        comPort: this.deviceForm.comPort || null,
      };
      const req$ = this.editingDeviceId
        ? this.api.updateDevice(this.editingDeviceId, {
            counterId: body.counterId,
            deviceTypeId: body.deviceTypeId,
            name: body.name,
            manufacturer: body.manufacturer,
            model: body.model,
            serialNumber: body.serialNumber,
            connectionType: body.connectionType,
            ip: body.ip,
            port: body.port,
            comPort: body.comPort,
            isDefault: body.isDefault,
            isActive: body.isActive,
          })
        : this.api.createDevice(body);
      req$.subscribe({
        next: () => {
          this.saving = false;
          this.modalOpen = false;
          this.snack.success(this.editingDeviceId ? 'Device updated' : 'Device created');
          this.load(this.page());
        },
        error: (e) => {
          this.saving = false;
          this.snack.error(e?.message || 'Save failed');
        },
      });
      return;
    }

    if (this.tab === 'templates') {
      const body = {
        templateType: this.templateForm.templateType,
        name: this.templateForm.name,
        templateContent: this.templateForm.templateContent,
        paperWidth: this.templateForm.paperWidth,
        isDefault: this.templateForm.isDefault,
        isActive: this.templateForm.isActive,
      };
      const req$ = this.editingTemplateId
        ? this.api.updatePrintTemplate(this.editingTemplateId, body)
        : this.api.createPrintTemplate(body);
      req$.subscribe({
        next: () => {
          this.saving = false;
          this.modalOpen = false;
          this.snack.success(this.editingTemplateId ? 'Template updated' : 'Template created');
          this.load(this.page());
        },
        error: (e) => {
          this.saving = false;
          this.snack.error(e?.message || 'Save failed');
        },
      });
      return;
    }

    if (this.tab === 'jobs') {
      this.api
        .createBarcodePrintJob({
          branchId: this.jobForm.branchId,
          printerDeviceId: this.jobForm.printerDeviceId,
          productId: this.jobForm.productId,
          quantity: this.jobForm.quantity,
          templateId: this.jobForm.templateId,
        })
        .subscribe({
          next: () => {
            this.saving = false;
            this.modalOpen = false;
            this.snack.success('Print job queued');
            this.load(this.page());
          },
          error: (e) => {
            this.saving = false;
            this.snack.error(e?.message || 'Queue failed');
          },
        });
      return;
    }

    this.api
      .createAttachment({
        fileName: this.attachmentForm.fileName,
        storagePath: this.attachmentForm.storagePath,
        contentType: this.attachmentForm.contentType || null,
        fileSize: this.attachmentForm.fileSize,
        hash: this.attachmentForm.hash || null,
      })
      .subscribe({
        next: () => {
          this.saving = false;
          this.modalOpen = false;
          this.snack.success('Attachment metadata saved');
          this.load(this.page());
        },
        error: (e) => {
          this.saving = false;
          this.snack.error(e?.message || 'Save failed');
        },
      });
  }

  simulateJob(row: BarcodePrintJobDto, fail = false): void {
    this.api.simulateBarcodePrintJob(row.id, { fail }).subscribe({
      next: () => {
        this.snack.success(fail ? 'Job marked failed' : 'Job simulated as printed');
        this.load(this.page());
      },
      error: (e) => this.snack.error(e?.message || 'Simulate failed'),
    });
  }

  openLink(row: AttachmentDto): void {
    this.linkForm = { attachmentId: row.id, entityName: 'Product', entityId: 0 };
    this.linkModalOpen = true;
  }

  saveLink(): void {
    this.saving = true;
    this.api.linkEntityAttachment(this.linkForm).subscribe({
      next: () => {
        this.saving = false;
        this.linkModalOpen = false;
        this.snack.success('Attachment linked to entity');
      },
      error: (e) => {
        this.saving = false;
        this.snack.error(e?.message || 'Link failed');
      },
    });
  }

  statusTone(status: string): 'success' | 'warning' | 'danger' | 'info' | 'neutral' {
    if (status === 'Printed' || status === 'Active') return 'success';
    if (status === 'Queued' || status === 'Printing') return 'info';
    if (status === 'Failed' || status === 'Cancelled') return 'danger';
    return 'neutral';
  }

  productSuggestFn = (q: string): Observable<AppTypeaheadItem<ProductDto>[]> =>
    this.api.searchProducts(q, 20).pipe(
      map((rows) =>
        rows.map((p) => ({
          id: p.id,
          label: p.name,
          detail: p.sku,
          data: p,
        })),
      ),
    );

  onJobProductPick(item: AppTypeaheadItem<ProductDto>): void {
    this.jobForm.productId = Number(item.id);
    this.jobForm.productSearch = `${item.detail || ''} — ${item.label}`.replace(/^ — /, '');
    this.productOptions = [{ value: item.id, label: `${item.detail} — ${item.label}` }];
  }

  productSearch(term: string): void {
    if (!term.trim()) return;
    this.api.searchProducts(term, 20).subscribe({
      next: (rows: ProductDto[]) => {
        this.productOptions = rows.map((p) => ({ value: p.id, label: `${p.sku} — ${p.name}` }));
      },
    });
  }

  refreshJobLookups(): void {
    this.api.searchDevices({ pageSize: 50, isActive: true, branchId: this.jobForm.branchId || null }).subscribe({
      next: (r) => {
        this.deviceOptions = r.items.map((d) => ({ value: d.id, label: d.name }));
        if (r.items.length && !this.jobForm.printerDeviceId) this.jobForm.printerDeviceId = r.items[0].id;
      },
    });
    this.api.searchPrintTemplates({ pageSize: 50, isActive: true }).subscribe({
      next: (r) => {
        this.templateOptions = r.items.map((t) => ({ value: t.id, label: t.name }));
        if (r.items.length && !this.jobForm.templateId) this.jobForm.templateId = r.items[0].id;
      },
    });
  }

  private fail(e: unknown): void {
    this.loading.set(false);
    this.error.set(e instanceof Error ? e.message : 'Load failed');
  }

  private numOrNull(v: string | number): number | null {
    if (v === '' || v == null) return null;
    const n = Number(v);
    return Number.isFinite(n) ? n : null;
  }

  private boolOrNull(v: string | number): boolean | null {
    if (v === '' || v == null) return null;
    return String(v) === 'true';
  }

  private emptyDeviceForm() {
    return {
      branchId: 0,
      counterId: null as number | null,
      deviceTypeId: 0,
      name: '',
      manufacturer: '',
      model: '',
      serialNumber: '',
      connectionType: '',
      ip: '',
      port: null as number | null,
      comPort: '',
      isDefault: false,
      isActive: true,
    };
  }

  private emptyTemplateForm() {
    return {
      templateType: 'Barcode',
      name: '',
      templateContent: '^XA^FO50,50^BY3^BCN,100^FD{{BARCODE}}^FS^XZ',
      paperWidth: 50 as number | null,
      isDefault: false,
      isActive: true,
    };
  }

  private emptyJobForm() {
    return {
      branchId: 0,
      printerDeviceId: 0,
      productId: 0,
      quantity: 1,
      templateId: 0,
      productSearch: '',
    };
  }

  private emptyAttachmentForm() {
    return {
      fileName: '',
      storagePath: 'stub://base64:',
      contentType: 'application/octet-stream',
      fileSize: 0,
      hash: '',
    };
  }
}
