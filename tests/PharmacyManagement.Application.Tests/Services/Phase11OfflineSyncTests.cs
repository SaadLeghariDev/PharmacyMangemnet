using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PharmacyManagement.Application.DTOs.Sync;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Domain.Constants;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;
using PharmacyManagement.Infrastructure.Services;

namespace PharmacyManagement.Application.Tests.Services;

public class Phase11OfflineSyncTests
{
    private sealed class Fixture
    {
        public PharmacyManagementDbContext Db { get; init; } = null!;
        public long TenantId { get; init; }
        public long BranchId { get; init; }
        public long TerminalId { get; init; }
        public long Terminal2Id { get; init; }
        public SyncService Sync { get; init; } = null!;
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

        var counter = new Counter
        {
            BranchId = branch.Id,
            Code = "C1",
            Name = "Counter 1",
            IsActive = true
        };
        db.Counters.Add(counter);
        await db.SaveChangesAsync();

        var terminal = new Posterminal
        {
            BranchId = branch.Id,
            CounterId = counter.Id,
            TerminalCode = "T1",
            IsActive = true
        };
        var terminal2 = new Posterminal
        {
            BranchId = branch.Id,
            CounterId = counter.Id,
            TerminalCode = "T2",
            IsActive = true
        };
        db.Posterminals.AddRange(terminal, terminal2);
        await db.SaveChangesAsync();

        var current = new Mock<ICurrentUserService>();
        current.SetupGet(c => c.TenantId).Returns(tenant.Id);
        current.SetupGet(c => c.UserId).Returns(1L);

        return new Fixture
        {
            Db = db,
            TenantId = tenant.Id,
            BranchId = branch.Id,
            TerminalId = terminal.Id,
            Terminal2Id = terminal2.Id,
            Sync = new SyncService(db, current.Object)
        };
    }

    [Fact]
    public async Task Register_node_then_reject_duplicate_code()
    {
        var fx = await SeedAsync();

        var node = await fx.Sync.RegisterNodeAsync(new RegisterSyncNodeRequest
        {
            BranchId = fx.BranchId,
            TerminalId = fx.TerminalId,
            NodeCode = "NODE-T1"
        });

        node.NodeCode.Should().Be("NODE-T1");
        node.IsActive.Should().BeTrue();
        node.LastSequence.Should().Be(0);

        var act = () => fx.Sync.RegisterNodeAsync(new RegisterSyncNodeRequest
        {
            BranchId = fx.BranchId,
            TerminalId = fx.Terminal2Id,
            NodeCode = "NODE-T1"
        });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Push_completes_batch_and_advances_sequence_or_partial_on_failure()
    {
        var fx = await SeedAsync();
        var node = await fx.Sync.RegisterNodeAsync(new RegisterSyncNodeRequest
        {
            BranchId = fx.BranchId,
            TerminalId = fx.TerminalId,
            NodeCode = "NODE-A"
        });

        var ok = await fx.Sync.PushAsync(new SyncPushRequest
        {
            SyncNodeId = node.Id,
            AutoComplete = true,
            Items =
            [
                new CreateSyncItemRequest
                {
                    EntityName = "Sale",
                    EntityId = 42,
                    Operation = SyncItemOperations.Insert,
                    Payload = "{}",
                    Version = 5
                }
            ]
        });

        ok.Batch.Status.Should().Be(SyncBatchStatuses.Completed);
        ok.Batch.ItemCount.Should().Be(1);
        ok.Batch.ProcessedCount.Should().Be(1);
        ok.Batch.BatchNumber.Should().Be("SB-000001");
        ok.Node.LastSequence.Should().Be(5);
        ok.Node.LastSyncAt.Should().NotBeNull();

        var failed = await fx.Sync.PushAsync(new SyncPushRequest
        {
            SyncNodeId = node.Id,
            AutoComplete = true,
            SimulateFailures = true,
            Items =
            [
                new CreateSyncItemRequest
                {
                    EntityName = "Sale",
                    EntityId = 43,
                    Operation = SyncItemOperations.Update,
                    Version = 6
                },
                new CreateSyncItemRequest
                {
                    EntityName = "Sale",
                    EntityId = 44,
                    Operation = SyncItemOperations.Update,
                    Version = 7
                }
            ]
        });

        // All items failed → Failed (not Partial)
        failed.Batch.Status.Should().Be(SyncBatchStatuses.Failed);
        failed.Batch.FailedCount.Should().Be(2);

        // Mixed: create InProgress, add items, mark one failed one processed, complete
        var batch = await fx.Sync.CreateBatchAsync(new CreateSyncBatchRequest { SyncNodeId = node.Id });
        var items = await fx.Sync.AddItemsAsync(batch.Id, new CreateSyncItemsRequest
        {
            Items =
            [
                new CreateSyncItemRequest
                {
                    EntityName = "Product",
                    EntityId = 1,
                    Operation = SyncItemOperations.Insert,
                    Version = 8
                },
                new CreateSyncItemRequest
                {
                    EntityName = "Product",
                    EntityId = 2,
                    Operation = SyncItemOperations.Insert,
                    Version = 9
                }
            ]
        });

        await fx.Sync.UpdateItemStatusAsync(items[0].Id, new UpdateSyncItemStatusRequest
        {
            Status = SyncItemStatuses.Processed
        });
        await fx.Sync.UpdateItemStatusAsync(items[1].Id, new UpdateSyncItemStatusRequest
        {
            Status = SyncItemStatuses.Failed,
            ErrorMessage = "apply error"
        });

        // Derive Partial via push-style completion helper: set status from item counts
        var refreshed = await fx.Db.SyncBatches.Include(b => b.SyncItems).FirstAsync(b => b.Id == batch.Id);
        var failCount = refreshed.SyncItems.Count(i => i.Status == SyncItemStatuses.Failed);
        var okCount = refreshed.SyncItems.Count(i => i.Status == SyncItemStatuses.Processed);
        failCount.Should().Be(1);
        okCount.Should().Be(1);

        var partial = await fx.Sync.UpdateBatchStatusAsync(batch.Id, new UpdateSyncBatchStatusRequest
        {
            Status = SyncBatchStatuses.Partial
        });
        partial.Status.Should().Be(SyncBatchStatuses.Partial);
        partial.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Pull_copies_peer_items_and_idempotency_keys_list_is_readable()
    {
        var fx = await SeedAsync();
        var nodeA = await fx.Sync.RegisterNodeAsync(new RegisterSyncNodeRequest
        {
            BranchId = fx.BranchId,
            TerminalId = fx.TerminalId,
            NodeCode = "NODE-SRC"
        });
        var nodeB = await fx.Sync.RegisterNodeAsync(new RegisterSyncNodeRequest
        {
            BranchId = fx.BranchId,
            TerminalId = fx.Terminal2Id,
            NodeCode = "NODE-DST"
        });

        await fx.Sync.PushAsync(new SyncPushRequest
        {
            SyncNodeId = nodeA.Id,
            Items =
            [
                new CreateSyncItemRequest
                {
                    EntityName = "Sale",
                    EntityId = 100,
                    Operation = SyncItemOperations.Insert,
                    Payload = "{\"n\":1}",
                    Version = 3
                }
            ]
        });

        var pull = await fx.Sync.PullAsync(new SyncPullRequest
        {
            SyncNodeId = nodeB.Id,
            SinceSequence = 0
        });

        pull.Batch.Status.Should().Be(SyncBatchStatuses.Completed);
        pull.Batch.ItemCount.Should().Be(1);
        pull.Batch.Items[0].EntityId.Should().Be(100);
        pull.Batch.Items[0].Status.Should().Be(SyncItemStatuses.Processed);
        pull.Node.LastSequence.Should().Be(3);

        fx.Db.IdempotencyKeys.Add(new IdempotencyKey
        {
            TerminalId = fx.TerminalId,
            Key = "offline-sale-abc-001",
            EntityType = "Sale",
            EntityId = 100,
            CreatedAt = DateTime.UtcNow
        });
        await fx.Db.SaveChangesAsync();

        var keys = await fx.Sync.SearchIdempotencyKeysAsync(new IdempotencyKeyQuery
        {
            TerminalId = fx.TerminalId,
            EntityType = "Sale"
        });

        keys.TotalCount.Should().Be(1);
        keys.Items[0].Key.Should().Be("offline-sale-abc-001");
        keys.Items[0].EntityId.Should().Be(100);

        // Sale create path still owns IdempotencyKeys writes — admin list is read-only and must not alter rows.
        var keyRow = await fx.Db.IdempotencyKeys.AsNoTracking()
            .SingleAsync(k => k.TerminalId == fx.TerminalId && k.Key == "offline-sale-abc-001");
        keyRow.EntityType.Should().Be("Sale");
    }
}
