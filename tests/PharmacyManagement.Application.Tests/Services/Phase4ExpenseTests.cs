using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PharmacyManagement.Application.DTOs.Cash;
using PharmacyManagement.Application.DTOs.Expenses;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class Phase4ExpenseTests
{
    private sealed class Fixture
    {
        public PharmacyManagementDbContext Db { get; init; } = null!;
        public long TenantId { get; init; }
        public long BranchId { get; init; }
        public long CounterId { get; init; }
        public long TerminalId { get; init; }
        public long UserId { get; init; }
        public long CashMethodId { get; init; }
        public long CardMethodId { get; init; }
        public ExpenseCategoryService Categories { get; init; } = null!;
        public ExpenseService Expenses { get; init; } = null!;
        public CashShiftService CashShifts { get; init; } = null!;
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
        await db.SaveChangesAsync();

        var user = new User
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
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var counter = new Counter { BranchId = branch.Id, Code = "C1", Name = "Counter 1", IsActive = true };
        db.Counters.Add(counter);
        await db.SaveChangesAsync();

        var terminal = new Posterminal
        {
            BranchId = branch.Id,
            CounterId = counter.Id,
            TerminalCode = "T1",
            IsPrimary = true,
            IsOnline = true,
            IsActive = true
        };
        db.Posterminals.Add(terminal);

        var cash = new PaymentMethod { Name = "Cash", Code = "CASH", Type = "Cash", IsActive = true };
        var card = new PaymentMethod { Name = "Card", Code = "CARD", Type = "Card", IsActive = true };
        db.PaymentMethods.AddRange(cash, card);

        db.NumberSequences.Add(new NumberSequence
        {
            TenantId = tenant.Id,
            BranchId = branch.Id,
            DocumentType = DocumentTypes.Expense,
            Prefix = "EXP-",
            CurrentNumber = 0,
            NumberLength = 6
        });
        await db.SaveChangesAsync();

        var current = new Mock<ICurrentUserService>();
        current.SetupGet(c => c.TenantId).Returns(tenant.Id);
        current.SetupGet(c => c.UserId).Returns(user.Id);

        var sequences = new NumberSequenceService(db);
        return new Fixture
        {
            Db = db,
            TenantId = tenant.Id,
            BranchId = branch.Id,
            CounterId = counter.Id,
            TerminalId = terminal.Id,
            UserId = user.Id,
            CashMethodId = cash.Id,
            CardMethodId = card.Id,
            Categories = new ExpenseCategoryService(db),
            Expenses = new ExpenseService(db, current.Object, sequences),
            CashShifts = new CashShiftService(db, current.Object)
        };
    }

    [Fact]
    public async Task Category_create_and_update_works()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            var created = await fx.Categories.CreateAsync(new CreateExpenseCategoryRequest
            {
                Name = "Utilities",
                Code = "util"
            });
            created.Code.Should().Be("UTIL");
            created.Name.Should().Be("Utilities");

            var updated = await fx.Categories.UpdateAsync(created.Id, new UpdateExpenseCategoryRequest
            {
                Name = "Shop Utilities",
                Code = "UTIL"
            });
            updated.Name.Should().Be("Shop Utilities");

            var list = await fx.Categories.SearchAsync(new ExpenseCategoryQuery { Search = "Shop" });
            list.TotalCount.Should().Be(1);
            list.Items[0].Id.Should().Be(created.Id);
        }
    }

    [Fact]
    public async Task Category_rejects_duplicate_code()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            await fx.Categories.CreateAsync(new CreateExpenseCategoryRequest { Name = "Rent", Code = "RENT" });
            var act = async () => await fx.Categories.CreateAsync(new CreateExpenseCategoryRequest
            {
                Name = "Other Rent",
                Code = "rent"
            });
            await act.Should().ThrowAsync<ConflictException>();
        }
    }

    [Fact]
    public async Task Expense_allocates_EXP_number()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            var cat = await fx.Categories.CreateAsync(new CreateExpenseCategoryRequest
            {
                Name = "Supplies",
                Code = "SUP"
            });

            var expense = await fx.Expenses.CreateAsync(new CreateExpenseRequest
            {
                BranchId = fx.BranchId,
                CategoryId = cat.Id,
                ExpenseDate = DateTime.UtcNow.Date,
                Amount = 25.5m,
                PaymentMethodId = fx.CardMethodId,
                Description = "Stationery"
            });

            expense.ExpenseNumber.Should().StartWith("EXP-");
            expense.Amount.Should().Be(25.5m);
            expense.CategoryId.Should().Be(cat.Id);
            expense.CreatedBy.Should().Be(fx.UserId);
            expense.ApprovedBy.Should().Be(fx.UserId);

            var cashLines = await fx.Db.CashTransactions.CountAsync();
            cashLines.Should().Be(0);
        }
    }

    [Fact]
    public async Task Cash_expense_posts_positive_Expense_cash_transaction_on_open_shift()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            var cat = await fx.Categories.CreateAsync(new CreateExpenseCategoryRequest
            {
                Name = "Courier",
                Code = "COURIER"
            });

            var shift = await fx.CashShifts.OpenAsync(new OpenCashShiftRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                OpeningAmount = 500
            });

            var expense = await fx.Expenses.CreateAsync(new CreateExpenseRequest
            {
                BranchId = fx.BranchId,
                CategoryId = cat.Id,
                ExpenseDate = DateTime.UtcNow.Date,
                Amount = 40m,
                PaymentMethodId = fx.CashMethodId,
                TerminalId = fx.TerminalId,
                Description = "Bike courier"
            });

            var txn = await fx.Db.CashTransactions
                .SingleAsync(t => t.TransactionType == "Expense" && t.ReferenceId == expense.Id);
            txn.CashShiftId.Should().Be(shift.Id);
            txn.ReferenceType.Should().Be("Expense");
            txn.Amount.Should().Be(40m);
            txn.Amount.Should().BeGreaterThan(0);

            var current = await fx.CashShifts.GetByIdAsync(shift.Id);
            current!.RunningTotal.Should().Be(460m); // 500 opening - 40 expense
        }
    }

    [Fact]
    public async Task Cash_expense_requires_open_shift_and_terminal()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            var cat = await fx.Categories.CreateAsync(new CreateExpenseCategoryRequest
            {
                Name = "Misc",
                Code = "MISC"
            });

            var missingTerminal = async () => await fx.Expenses.CreateAsync(new CreateExpenseRequest
            {
                BranchId = fx.BranchId,
                CategoryId = cat.Id,
                ExpenseDate = DateTime.UtcNow.Date,
                Amount = 10m,
                PaymentMethodId = fx.CashMethodId
            });
            await missingTerminal.Should().ThrowAsync<ValidationAppException>();

            var noShift = async () => await fx.Expenses.CreateAsync(new CreateExpenseRequest
            {
                BranchId = fx.BranchId,
                CategoryId = cat.Id,
                ExpenseDate = DateTime.UtcNow.Date,
                Amount = 10m,
                PaymentMethodId = fx.CashMethodId,
                TerminalId = fx.TerminalId
            });
            await noShift.Should().ThrowAsync<ValidationAppException>();
        }
    }

    [Fact]
    public async Task Non_cash_expense_does_not_create_cash_transaction()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            var cat = await fx.Categories.CreateAsync(new CreateExpenseCategoryRequest
            {
                Name = "Bank fees",
                Code = "BANKFEE"
            });

            await fx.CashShifts.OpenAsync(new OpenCashShiftRequest
            {
                BranchId = fx.BranchId,
                CounterId = fx.CounterId,
                TerminalId = fx.TerminalId,
                OpeningAmount = 100
            });

            var expense = await fx.Expenses.CreateAsync(new CreateExpenseRequest
            {
                BranchId = fx.BranchId,
                CategoryId = cat.Id,
                ExpenseDate = DateTime.UtcNow.Date,
                Amount = 15m,
                PaymentMethodId = fx.CardMethodId,
                TerminalId = fx.TerminalId
            });

            (await fx.Db.CashTransactions.CountAsync(t =>
                t.TransactionType == "Expense" && t.ReferenceId == expense.Id)).Should().Be(0);
        }
    }

    [Fact]
    public async Task Expense_update_allows_description_only()
    {
        var fx = await SeedAsync();
        await using (fx.Db)
        {
            var cat = await fx.Categories.CreateAsync(new CreateExpenseCategoryRequest
            {
                Name = "Cleaning",
                Code = "CLEAN"
            });
            var created = await fx.Expenses.CreateAsync(new CreateExpenseRequest
            {
                BranchId = fx.BranchId,
                CategoryId = cat.Id,
                ExpenseDate = DateTime.UtcNow.Date,
                Amount = 12m,
                PaymentMethodId = fx.CardMethodId,
                Description = "Original"
            });

            var updated = await fx.Expenses.UpdateAsync(created.Id, new UpdateExpenseRequest
            {
                Description = "Updated note"
            });
            updated.Description.Should().Be("Updated note");
            updated.Amount.Should().Be(12m);
            updated.ExpenseNumber.Should().Be(created.ExpenseNumber);
        }
    }
}
