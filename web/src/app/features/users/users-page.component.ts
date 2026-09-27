import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { PharmacyApiService } from '../../core/services/pharmacy-api.service';
import {
  BranchDto,
  PermissionDto,
  RoleDto,
  UserAdminDto,
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
  selector: 'app-users-page',
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
  templateUrl: './users-page.component.html',
  styleUrl: './users-page.component.scss',
})
export class UsersPageComponent implements OnInit {
  tab: 'users' | 'roles' = 'users';

  readonly userColumns: AppTableColumn[] = [
    { key: 'user', label: 'User' },
    { key: 'contact', label: 'Contact' },
    { key: 'roles', label: 'Roles' },
    { key: 'branches', label: 'Branches' },
    { key: 'status', label: 'Status' },
    { key: 'actions', label: '' },
  ];

  readonly roleColumns: AppTableColumn[] = [
    { key: 'name', label: 'Role' },
    { key: 'perms', label: 'Permissions' },
    { key: 'system', label: 'System' },
    { key: 'actions', label: '' },
  ];

  readonly activeOptions: AppSelectOption[] = [
    { value: '', label: 'All statuses' },
    { value: 'true', label: 'Active' },
    { value: 'false', label: 'Inactive' },
  ];

  search = '';
  activeFilter: string | number = '';
  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly users = signal<UserAdminDto[]>([]);
  readonly roles = signal<RoleDto[]>([]);
  readonly roleCatalog = signal<RoleDto[]>([]);
  readonly permissions = signal<PermissionDto[]>([]);
  readonly branches = signal<BranchDto[]>([]);
  readonly totalCount = signal(0);
  readonly hasNext = signal(false);

  readonly modalOpen = signal(false);
  readonly assignOpen = signal(false);
  readonly saving = signal(false);
  modalMode: 'user' | 'role' | 'assign-roles' | 'assign-branches' | 'assign-perms' = 'user';
  editingUserId: number | null = null;
  editingRoleId: number | null = null;
  assignUserId: number | null = null;

  formUsername = '';
  formPassword = '';
  formFullName = '';
  formEmail = '';
  formPhone = '';
  formEmployeeCode = '';
  formActive = true;
  formRoleName = '';
  formRoleDescription = '';
  selectedRoleIds = new Set<number>();
  selectedBranchIds = new Set<number>();
  selectedPermissionIds = new Set<number>();

  constructor(
    private readonly api: PharmacyApiService,
    private readonly snackbar: SnackbarService,
  ) {}

  ngOnInit(): void {
    forkJoin({
      roles: this.api.searchRoles({ pageSize: 100 }),
      permissions: this.api.listPermissions(200),
      branches: this.api.listBranches(),
    }).subscribe({
      next: ({ roles, permissions, branches }) => {
        this.roles.set(roles.items);
        this.roleCatalog.set(roles.items);
        this.permissions.set(permissions);
        this.branches.set(branches);
        this.load(1);
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  setTab(tab: 'users' | 'roles'): void {
    this.tab = tab;
    this.search = '';
    this.activeFilter = '';
    this.load(1);
  }

  applyFilters(): void {
    this.load(1);
  }

  clearFilters(): void {
    this.search = '';
    this.activeFilter = '';
    this.load(1);
  }

  load(page = 1): void {
    this.page.set(page);
    this.loading.set(true);
    this.error.set('');
    if (this.tab === 'users') {
      const isActive =
        this.activeFilter === 'true' ? true : this.activeFilter === 'false' ? false : null;
      this.api
        .searchUsers({ page, pageSize: 20, search: this.search, isActive })
        .subscribe({
          next: (result) => {
            this.users.set(result.items);
            this.totalCount.set(result.totalCount);
            this.hasNext.set(result.hasNext);
            this.loading.set(false);
          },
          error: (err: unknown) => this.fail(err),
        });
      return;
    }

    this.api.searchRoles({ page, pageSize: 20, search: this.search }).subscribe({
      next: (result) => {
        this.roles.set(result.items);
        this.totalCount.set(result.totalCount);
        this.hasNext.set(result.hasNext);
        this.loading.set(false);
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  branchLabel(ids: number[]): string {
    if (!ids.length) return '—';
    const map = new Map(this.branches().map((b) => [b.id, b.code]));
    return ids.map((id) => map.get(id) ?? String(id)).join(', ');
  }

  openCreateUser(): void {
    this.modalMode = 'user';
    this.editingUserId = null;
    this.formUsername = '';
    this.formPassword = '';
    this.formFullName = '';
    this.formEmail = '';
    this.formPhone = '';
    this.formEmployeeCode = '';
    this.formActive = true;
    this.modalOpen.set(true);
  }

  openEditUser(user: UserAdminDto): void {
    this.modalMode = 'user';
    this.editingUserId = user.id;
    this.formUsername = user.username;
    this.formPassword = '';
    this.formFullName = user.fullName;
    this.formEmail = user.email ?? '';
    this.formPhone = user.phone ?? '';
    this.formEmployeeCode = user.employeeCode ?? '';
    this.formActive = user.isActive;
    this.modalOpen.set(true);
  }

  openCreateRole(): void {
    this.modalMode = 'role';
    this.editingRoleId = null;
    this.formRoleName = '';
    this.formRoleDescription = '';
    this.modalOpen.set(true);
  }

  openEditRole(role: RoleDto): void {
    this.modalMode = 'role';
    this.editingRoleId = role.id;
    this.formRoleName = role.name;
    this.formRoleDescription = role.description ?? '';
    this.modalOpen.set(true);
  }

  openAssignRoles(user: UserAdminDto): void {
    this.modalMode = 'assign-roles';
    this.assignUserId = user.id;
    this.selectedRoleIds = new Set(user.roleIds);
    this.assignOpen.set(true);
  }

  openAssignBranches(user: UserAdminDto): void {
    this.modalMode = 'assign-branches';
    this.assignUserId = user.id;
    this.selectedBranchIds = new Set(user.branchIds);
    this.assignOpen.set(true);
  }

  openAssignPermissions(role: RoleDto): void {
    this.modalMode = 'assign-perms';
    this.editingRoleId = role.id;
    this.selectedPermissionIds = new Set(role.permissionIds);
    this.assignOpen.set(true);
  }

  toggleRole(id: number, checked: boolean): void {
    if (checked) this.selectedRoleIds.add(id);
    else this.selectedRoleIds.delete(id);
  }

  toggleBranch(id: number, checked: boolean): void {
    if (checked) this.selectedBranchIds.add(id);
    else this.selectedBranchIds.delete(id);
  }

  togglePermission(id: number, checked: boolean): void {
    if (checked) this.selectedPermissionIds.add(id);
    else this.selectedPermissionIds.delete(id);
  }

  closeModal(): void {
    this.modalOpen.set(false);
  }

  closeAssign(): void {
    this.assignOpen.set(false);
  }

  saveModal(): void {
    this.saving.set(true);
    if (this.modalMode === 'user') {
      if (this.editingUserId == null) {
        this.api
          .createUser({
            username: this.formUsername.trim(),
            password: this.formPassword,
            fullName: this.formFullName.trim(),
            email: this.formEmail.trim() || null,
            phone: this.formPhone.trim() || null,
            employeeCode: this.formEmployeeCode.trim() || null,
            isActive: this.formActive,
          })
          .subscribe({
            next: () => this.afterSave('User created'),
            error: (err: unknown) => this.failSave(err),
          });
        return;
      }
      this.api
        .updateUser(this.editingUserId, {
          fullName: this.formFullName.trim(),
          email: this.formEmail.trim() || null,
          phone: this.formPhone.trim() || null,
          employeeCode: this.formEmployeeCode.trim() || null,
          isActive: this.formActive,
          password: this.formPassword.trim() || null,
        })
        .subscribe({
          next: () => this.afterSave('User updated'),
          error: (err: unknown) => this.failSave(err),
        });
      return;
    }

    if (this.editingRoleId == null) {
      this.api
        .createRole({
          name: this.formRoleName.trim(),
          description: this.formRoleDescription.trim() || null,
        })
        .subscribe({
          next: () => this.afterSave('Role created'),
          error: (err: unknown) => this.failSave(err),
        });
      return;
    }
    this.api
      .updateRole(this.editingRoleId, {
        name: this.formRoleName.trim(),
        description: this.formRoleDescription.trim() || null,
      })
      .subscribe({
        next: () => this.afterSave('Role updated'),
        error: (err: unknown) => this.failSave(err),
      });
  }

  saveAssign(): void {
    this.saving.set(true);
    if (this.modalMode === 'assign-roles' && this.assignUserId != null) {
      this.api.assignUserRoles(this.assignUserId, [...this.selectedRoleIds]).subscribe({
        next: () => this.afterAssign('Roles assigned'),
        error: (err: unknown) => this.failSave(err),
      });
      return;
    }
    if (this.modalMode === 'assign-branches' && this.assignUserId != null) {
      this.api.assignUserBranches(this.assignUserId, [...this.selectedBranchIds]).subscribe({
        next: () => this.afterAssign('Branches assigned'),
        error: (err: unknown) => this.failSave(err),
      });
      return;
    }
    if (this.modalMode === 'assign-perms' && this.editingRoleId != null) {
      this.api.assignRolePermissions(this.editingRoleId, [...this.selectedPermissionIds]).subscribe({
        next: () => this.afterAssign('Permissions assigned'),
        error: (err: unknown) => this.failSave(err),
      });
    }
  }

  deactivateUser(user: UserAdminDto): void {
    this.api.deactivateUser(user.id).subscribe({
      next: () => {
        this.snackbar.success('User deactivated');
        this.load(this.page());
      },
      error: (err: unknown) => this.fail(err),
    });
  }

  private afterSave(message: string): void {
    this.saving.set(false);
    this.modalOpen.set(false);
    this.snackbar.success(message);
    this.load(this.page());
    if (this.tab === 'roles' || this.modalMode === 'role') {
      this.api.searchRoles({ pageSize: 100 }).subscribe({
        next: (r) => {
          this.roleCatalog.set(r.items);
          if (this.tab === 'roles') this.roles.set(r.items);
        },
      });
    }
  }

  private afterAssign(message: string): void {
    this.saving.set(false);
    this.assignOpen.set(false);
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
