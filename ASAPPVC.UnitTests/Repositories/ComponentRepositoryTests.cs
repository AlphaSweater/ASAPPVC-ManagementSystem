using ASAPPVC.App.Data;
using ASAPPVC.App.Models;
using ASAPPVC.App.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UnitTests.Repositories
{
    public class ComponentRepositoryTests
    {
        private static AppDbContext CreateContext(SqliteConnection conn)
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conn)
                .Options;
            return new AppDbContext(opts);
        }

        // ---------------- GetByIdOrCode: by Id returns entity ----------------
        [Fact]
        public async Task GetByIdOrCodeAsync_ById_ReturnsComponentAsync()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();
            using var ctx = CreateContext(conn);
            ctx.Database.EnsureCreated();

            var comp = new Component { Id = Guid.NewGuid(), ComponentCode = "C-001", ComponentName = "Bolt", UnitCost = 1m, QuantityOnHand = 5m, LocationCode = "A-1" };
            ctx.Components.Add(comp);
            await ctx.SaveChangesAsync();

            var repo = new ComponentRepository(ctx);

            var fetched = await repo.GetByIdOrCodeAsync(id: comp.Id);

            fetched.Should().NotBeNull();
            fetched!.Id.Should().Be(comp.Id);
        }

        // ---------------- GetByIdOrCode: by Code returns entity ----------------
        [Fact]
        public async Task GetByIdOrCodeAsync_ByCode_ReturnsComponentAsync()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();
            using var ctx = CreateContext(conn);
            ctx.Database.EnsureCreated();

            var comp = new Component { Id = Guid.NewGuid(), ComponentCode = "C-002", ComponentName = "Nut", UnitCost = 0.5m, QuantityOnHand = 10m, LocationCode = "B-2" };
            ctx.Components.Add(comp);
            await ctx.SaveChangesAsync();

            var repo = new ComponentRepository(ctx);

            var fetched = await repo.GetByIdOrCodeAsync(code: comp.ComponentCode);

            fetched.Should().NotBeNull();
            fetched!.ComponentCode.Should().Be("C-002");
        }

        // ---------------- GetListOrderedByCode: returns ordered list ----------------
        [Fact]
        public async Task GetListOrderedByCodeAsync_ReturnsOrderedAsync()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();
            using var ctx = CreateContext(conn);
            ctx.Database.EnsureCreated();

            var a = new Component { Id = Guid.NewGuid(), ComponentCode = "C-100", ComponentName = "A", UnitCost = 1m, QuantityOnHand = 1m, LocationCode = "X-1" };
            var b = new Component { Id = Guid.NewGuid(), ComponentCode = "C-010", ComponentName = "B", UnitCost = 1m, QuantityOnHand = 1m, LocationCode = "X-2" };
            ctx.Components.AddRange(a, b);
            await ctx.SaveChangesAsync();

            var repo = new ComponentRepository(ctx);

            var list = await repo.GetListOrderedByCodeAsync();

            list.Select(c => c.ComponentCode).Should().ContainInOrder("C-010", "C-100");
        }

        // ---------------- Search: empty term returns all ordered ----------------
        [Fact]
        public async Task SearchAsync_EmptyTerm_ReturnsAllOrderedAsync()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();
            using var ctx = CreateContext(conn);
            ctx.Database.EnsureCreated();

            var x = new Component { Id = Guid.NewGuid(), ComponentCode = "C-200", ComponentName = "Zed", UnitCost = 1m, QuantityOnHand = 1m, LocationCode = "X-1" };
            var y = new Component { Id = Guid.NewGuid(), ComponentCode = "C-100", ComponentName = "Alpha", UnitCost = 1m, QuantityOnHand = 1m, LocationCode = "X-2" };
            ctx.Components.AddRange(x, y);
            await ctx.SaveChangesAsync();

            var repo = new ComponentRepository(ctx);

            var results = await repo.SearchAsync(term: string.Empty);

            results.Select(r => r.ComponentName).Should().ContainInOrder("Alpha", "Zed");
        }
    }
}