using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using PharmacyManagement.Application.DTOs.Admin;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Identity;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class Phase8AdminTests
{
    private sealed class Fixture
    {
        public PharmacyManagementDbContext Db { get; init; } = null!;
        public long TenantId { get; init; }
        public long BranchId { get; init; }
        public long UserId { get; init; }
        public UserAdminService Users { get; init; } = null!;
        public RoleAdminService Roles { get; init; } = null!;
        public SettingsService Settings { get; init; } = null!;
        public ReasonCodeService ReasonCodes { get; init; } = null!;
        public AuditLogService AuditLogs { get; init; } = null!;
    }

    private static async Task<Fixture> SeedAsync()
    {
        var options = new DbContextOptionsBuilder<PharmacyManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new PharmacyManagementDbContext(options);

        var tenant = new Tenant
        {
            Name = "Demo",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var branch = new Branch
        {
            TenantId = tenant.Id,
            Code = "MAIN",
            Name = "Main",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }
        };
        db.Branches.Add(branch);

        var admin = new User
        {
            TenantId = tenant.Id,
            Username = "admin",
            Email = "a@test.local",
            PasswordHash = "x",
            FullName = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }
        };
        db.Users.Add(admin);

        var view = new Permission { Code = PermissionCodes.OrgView, Name = "View Org", Module = "Organization" };
        var edit = new Permission { Code = PermissionCodes.OrgEdit, Name = "Edit Org", Module = "Organization" };
        var secUsers = new Permission { Code = PermissionCodes.SecUsers, Name = "Users", Module = "Security" };
        var secRoles = new Permission { Code = PermissionCodes.SecRoles, Name = "Roles", Module = "Security" };
        db.Permissions.AddRange(view, edit, secUsers, secRoles);
        await db.SaveChangesAsync();

        var role = new Role
        {
            TenantId = tenant.Id,
            Name = "Administrator",
            Description = "System admin",
            IsSystemRole = true
        };
        role.Permissions.Add(view);
        role.Permissions.Add(edit);
        role.Permissions.Add(secUsers);
        role.Permissions.Add(secRoles);
        db.Roles.Add(role);
        admin.Roles.Add(role);
        admin.Branches.Add(branch);
        await db.SaveChangesAsync();

        db.AuditLogs.Add(new AuditLog
        {
            TenantId = tenant.Id,
            BranchId = branch.Id,
            UserId = admin.Id,
            EntityName = "Users",
            EntityId = admin.Id,
            Action = "Login",
            NewValues = "{\"username\":\"admin\"}",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var current = new Mock<ICurrentUserService>();
        current.SetupGet(c => c.TenantId).Returns(tenant.Id);
        current.SetupGet(c => c.UserId).Returns(admin.Id);
        current.SetupGet(c => c.IsAuthenticated).Returns(true);

        var hasher = new PasswordHasher<PasswordIdentityUser>();
        return new Fixture
        {
            Db = db,
            TenantId = tenant.Id,
            BranchId = branch.Id,
            UserId = admin.Id,
            Users = new UserAdminService(db, current.Object, hasher),
            Roles = new RoleAdminService(db, current.Object),
            Settings = new SettingsService(db, current.Object),
            ReasonCodes = new ReasonCodeService(db, current.Object),
            AuditLogs = new AuditLogService(db, current.Object)
        };
    }

    [Fact]
    public async Task Users_Create_Update_AssignRolesBranches_Deactivate()
    {
        var fx = await SeedAsync();
        var cashierRole = await fx.Roles.CreateAsync(new CreateRoleRequest
        {
            Name = "Cashier",
            Description = "POS cashier"
        });

        var created = await fx.Users.CreateAsync(new CreateUserRequest
        {
            Username = "cashier1",
            Password = "Cashier@123",
            FullName = "Cashier One",
            Email = "c1@test.local",
            RoleIds = [cashierRole.Id],
            BranchIds = [fx.BranchId]
        });

        created.Username.Should().Be("cashier1");
        created.RoleIds.Should().Contain(cashierRole.Id);
        created.BranchIds.Should().Contain(fx.BranchId);
        created.IsActive.Should().BeTrue();

        var stored = await fx.Db.Users.FirstAsync(u => u.Id == created.Id);
        stored.PasswordHash.Should().NotBe(AuthService.SeedPlaceholderHash);
        stored.PasswordHash.Should().NotBe("Cashier@123");

        var updated = await fx.Users.UpdateAsync(created.Id, new UpdateUserRequest
        {
            FullName = "Cashier Updated",
            Email = "c1b@test.local",
            IsActive = true
        });
        updated.FullName.Should().Be("Cashier Updated");

        var adminRole = (await fx.Roles.SearchAsync(new RoleQuery { Search = "Administrator", PageSize = 10 }))
            .Items.Single();
        var withRoles = await fx.Users.AssignRolesAsync(created.Id, new AssignUserRolesRequest
        {
            RoleIds = [adminRole.Id, cashierRole.Id]
        });
        withRoles.RoleIds.Should().BeEquivalentTo([adminRole.Id, cashierRole.Id]);

        var withBranches = await fx.Users.AssignBranchesAsync(created.Id, new AssignUserBranchesRequest
        {
            BranchIds = []
        });
        withBranches.BranchIds.Should().BeEmpty();

        await fx.Users.DeactivateAsync(created.Id);
        (await fx.Users.GetByIdAsync(created.Id))!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Roles_Create_Update_AssignPermissions()
    {
        var fx = await SeedAsync();
        var perms = await fx.Roles.SearchPermissionsAsync(new PermissionQuery { PageSize = 50 });
        perms.Items.Should().Contain(p => p.Code == PermissionCodes.SecUsers);
        perms.Items.Should().Contain(p => p.Code == PermissionCodes.SecRoles);

        var role = await fx.Roles.CreateAsync(new CreateRoleRequest
        {
            Name = "Inventory Clerk",
            PermissionIds = perms.Items.Where(p => p.Code == PermissionCodes.OrgView).Select(p => p.Id).ToList()
        });
        role.PermissionCodes.Should().Contain(PermissionCodes.OrgView);

        var updated = await fx.Roles.UpdateAsync(role.Id, new UpdateRoleRequest
        {
            Name = "Inventory Lead",
            Description = "Lead clerk"
        });
        updated.Name.Should().Be("Inventory Lead");

        var allIds = perms.Items.Select(p => p.Id).ToList();
        var assigned = await fx.Roles.AssignPermissionsAsync(role.Id, new AssignRolePermissionsRequest
        {
            PermissionIds = allIds
        });
        assigned.PermissionIds.Should().HaveCount(allIds.Count);
    }

    [Fact]
    public async Task Settings_ReasonCodes_AuditLogs()
    {
        var fx = await SeedAsync();

        var tenantSetting = await fx.Settings.UpsertTenantSettingAsync(new UpsertSettingRequest
        {
            SettingKey = "POS.ReceiptFooter",
            SettingValue = "Thank you",
            IsEncrypted = false
        });
        tenantSetting.SettingValue.Should().Be("Thank you");

        var again = await fx.Settings.UpsertTenantSettingAsync(new UpsertSettingRequest
        {
            SettingKey = "POS.ReceiptFooter",
            SettingValue = "Come again",
            IsEncrypted = false
        });
        again.Id.Should().Be(tenantSetting.Id);
        again.SettingValue.Should().Be("Come again");

        var branchSetting = await fx.Settings.UpsertBranchSettingAsync(fx.BranchId, new UpsertSettingRequest
        {
            SettingKey = "POS.DefaultCounter",
            SettingValue = "1"
        });
        branchSetting.BranchId.Should().Be(fx.BranchId);

        var reason = await fx.ReasonCodes.CreateAsync(new CreateReasonCodeRequest
        {
            ReasonType = ReasonTypes.Void,
            Code = "MGR",
            Name = "Manager void",
            IsActive = true
        });
        reason.ReasonType.Should().Be(ReasonTypes.Void);

        var updatedReason = await fx.ReasonCodes.UpdateAsync(reason.Id, new UpdateReasonCodeRequest
        {
            Name = "Manager override",
            IsActive = false
        });
        updatedReason.IsActive.Should().BeFalse();

        var audits = await fx.AuditLogs.SearchAsync(new AuditLogQuery
        {
            EntityName = "Users",
            PageSize = 10
        });
        audits.Items.Should().NotBeEmpty();
        audits.Items.First().Action.Should().Be("Login");
    }

    [Fact]
    public void PermissionCodes_SecAndOrg_Present()
    {
        PermissionCodes.SecUsers.Should().Be("SEC.USERS");
        PermissionCodes.SecRoles.Should().Be("SEC.ROLES");
        PermissionCodes.OrgView.Should().Be("ORG.VIEW");
        PermissionCodes.OrgEdit.Should().Be("ORG.EDIT");
        ReasonTypes.IsKnown(ReasonTypes.Void).Should().BeTrue();
        ReasonTypes.IsKnown("Nope").Should().BeFalse();
    }
}
