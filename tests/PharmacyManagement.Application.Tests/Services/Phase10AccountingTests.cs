using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PharmacyManagement.Application.DTOs.Finance;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class Phase10AccountingTests
{
    private sealed class Fixture
    {
        public PharmacyManagementDbContext Db { get; init; } = null!;
        public long TenantId { get; init; }
        public long BranchId { get; init; }
        public long AssetTypeId { get; init; }
        public long ExpenseTypeId { get; init; }
        public long CashAccountId { get; init; }
        public long ExpenseAccountId { get; init; }
        public ChartOfAccountService Coa { get; init; } = null!;
        public AccountTypeService AccountTypes { get; init; } = null!;
        public JournalEntryService Journals { get; init; } = null!;
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

        var asset = new AccountType { Name = "Asset" };
        var liability = new AccountType { Name = "Liability" };
        var equity = new AccountType { Name = "Equity" };
        var revenue = new AccountType { Name = "Revenue" };
        var expense = new AccountType { Name = "Expense" };
        db.AccountTypes.AddRange(asset, liability, equity, revenue, expense);
        await db.SaveChangesAsync();

        var cash = new ChartOfAccount
        {
            TenantId = tenant.Id,
            Code = "1000",
            Name = "Cash",
            AccountTypeId = asset.Id,
            IsSystemAccount = true,
            IsActive = true
        };
        var expAcct = new ChartOfAccount
        {
            TenantId = tenant.Id,
            Code = "5000",
            Name = "Office Expense",
            AccountTypeId = expense.Id,
            IsSystemAccount = false,
            IsActive = true
        };
        db.ChartOfAccounts.AddRange(cash, expAcct);

        db.NumberSequences.Add(new NumberSequence
        {
            TenantId = tenant.Id,
            BranchId = branch.Id,
            TerminalId = null,
            DocumentType = DocumentTypes.Journal,
            Prefix = "JE-",
            CurrentNumber = 0,
            NumberLength = 6
        });
        await db.SaveChangesAsync();

        var current = new Mock<ICurrentUserService>();
        current.SetupGet(c => c.TenantId).Returns(tenant.Id);
        current.SetupGet(c => c.UserId).Returns(1L);
        var sequences = new NumberSequenceService(db);

        return new Fixture
        {
            Db = db,
            TenantId = tenant.Id,
            BranchId = branch.Id,
            AssetTypeId = asset.Id,
            ExpenseTypeId = expense.Id,
            CashAccountId = cash.Id,
            ExpenseAccountId = expAcct.Id,
            Coa = new ChartOfAccountService(db, current.Object),
            AccountTypes = new AccountTypeService(db),
            Journals = new JournalEntryService(db, current.Object, sequences)
        };
    }

    [Fact]
    public async Task AccountTypes_list_and_coa_create_deactivate()
    {
        var fx = await SeedAsync();

        var types = await fx.AccountTypes.SearchAsync(new AccountTypeQuery { PageSize = 50 });
        types.TotalCount.Should().Be(5);
        types.Items.Select(t => t.Name).Should().Contain(["Asset", "Expense"]);

        var created = await fx.Coa.CreateAsync(new CreateChartOfAccountRequest
        {
            Code = "1100",
            Name = "Bank",
            AccountTypeId = fx.AssetTypeId,
            ParentAccountId = fx.CashAccountId,
            IsActive = true
        });
        created.Code.Should().Be("1100");
        created.ParentAccountId.Should().Be(fx.CashAccountId);
        created.AccountTypeName.Should().Be("Asset");

        var deactivated = await fx.Coa.DeactivateAsync(created.Id);
        deactivated.IsActive.Should().BeFalse();

        var list = await fx.Coa.SearchAsync(new ChartOfAccountQuery { IsActive = false });
        list.Items.Should().Contain(a => a.Id == created.Id);
    }

    [Fact]
    public async Task Balanced_journal_create_posts_and_reverse()
    {
        var fx = await SeedAsync();

        var draft = await fx.Journals.CreateDraftAsync(new CreateJournalEntryRequest
        {
            BranchId = fx.BranchId,
            EntryDate = DateTime.UtcNow.Date,
            Description = "Pay office expense",
            Lines =
            [
                new CreateJournalLineRequest { AccountId = fx.ExpenseAccountId, Debit = 100m, Credit = 0m },
                new CreateJournalLineRequest { AccountId = fx.CashAccountId, Debit = 0m, Credit = 100m }
            ]
        });

        draft.Status.Should().Be(JournalEntryStatuses.Draft);
        draft.EntryNumber.Should().StartWith("JE-");
        draft.Lines.Should().HaveCount(2);
        draft.TotalDebit.Should().Be(100m);
        draft.TotalCredit.Should().Be(100m);

        var posted = await fx.Journals.PostAsync(draft.Id);
        posted.Status.Should().Be(JournalEntryStatuses.Posted);
        posted.PostedBy.Should().Be(1L);
        posted.PostedAt.Should().NotBeNull();

        var reversal = await fx.Journals.ReverseAsync(draft.Id);
        reversal.Status.Should().Be(JournalEntryStatuses.Posted);
        reversal.ReversalOfEntryId.Should().Be(draft.Id);
        reversal.Lines.Should().HaveCount(2);
        reversal.Lines.Should().Contain(l => l.AccountId == fx.ExpenseAccountId && l.Credit == 100m && l.Debit == 0m);
        reversal.Lines.Should().Contain(l => l.AccountId == fx.CashAccountId && l.Debit == 100m && l.Credit == 0m);

        var original = await fx.Journals.GetByIdAsync(draft.Id);
        original!.Status.Should().Be(JournalEntryStatuses.Reversed);
    }

    [Fact]
    public async Task Unbalanced_journal_is_rejected()
    {
        var fx = await SeedAsync();

        var act = () => fx.Journals.CreateDraftAsync(new CreateJournalEntryRequest
        {
            BranchId = fx.BranchId,
            EntryDate = DateTime.UtcNow.Date,
            Lines =
            [
                new CreateJournalLineRequest { AccountId = fx.ExpenseAccountId, Debit = 100m, Credit = 0m },
                new CreateJournalLineRequest { AccountId = fx.CashAccountId, Debit = 0m, Credit = 50m }
            ]
        });

        var ex = await act.Should().ThrowAsync<ValidationAppException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("not balanced", StringComparison.OrdinalIgnoreCase));
    }
}
