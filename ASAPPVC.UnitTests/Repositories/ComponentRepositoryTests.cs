using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories;
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
        public async System.Threading.Tasks.Task GetByIdOrCodeAsync_ById_ReturnsComponentAsync()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();
            using var ctx = CreateContext(conn);
            ctx.Database.EnsureCreated();

            var comp = new Component { Id = Guid.NewGuid(), ComponentCode = "C-001", Name = "Bolt", UnitCost = 1m, CurrentAmount = 5m, StorageLocation = "A1" };
            ctx.Components.Add(comp);
            await ctx.SaveChangesAsync();

            var repo = new ComponentRepository(ctx);

            var fetched = await repo.GetByIdOrCodeAsync(id: comp.Id);

            fetched.Should().NotBeNull();
            fetched!.Id.Should().Be(comp.Id);
        }

        // ---------------- GetByIdOrCode: by Code returns entity ----------------
        [Fact]
        public async System.Threading.Tasks.Task GetByIdOrCodeAsync_ByCode_ReturnsComponentAsync()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();
            using var ctx = CreateContext(conn);
            ctx.Database.EnsureCreated();

            var comp = new Component { Id = Guid.NewGuid(), ComponentCode = "C-002", Name = "Nut", UnitCost = 0.5m, CurrentAmount = 10m, StorageLocation = "B2" };
            ctx.Components.Add(comp);
            await ctx.SaveChangesAsync();

            var repo = new ComponentRepository(ctx);

            var fetched = await repo.GetByIdOrCodeAsync(code: comp.ComponentCode);

            fetched.Should().NotBeNull();
            fetched!.ComponentCode.Should().Be("C-002");
        }

        // ---------------- GetListOrderedByCode: returns ordered list ----------------
        [Fact]
        public async System.Threading.Tasks.Task GetListOrderedByCodeAsync_ReturnsOrderedAsync()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();
            using var ctx = CreateContext(conn);
            ctx.Database.EnsureCreated();

            var a = new Component { Id = Guid.NewGuid(), ComponentCode = "C-100", Name = "A", UnitCost = 1m, CurrentAmount = 1m, StorageLocation = "X" };
            var b = new Component { Id = Guid.NewGuid(), ComponentCode = "C-010", Name = "B", UnitCost = 1m, CurrentAmount = 1m, StorageLocation = "X" };
            ctx.Components.AddRange(a, b);
            await ctx.SaveChangesAsync();

            var repo = new ComponentRepository(ctx);

            var list = await repo.GetListOrderedByCodeAsync();

            list.Select(c => c.ComponentCode).Should().ContainInOrder("C-010", "C-100");
        }

        // ---------------- Search: empty term returns all ordered ----------------
        [Fact]
        public async System.Threading.Tasks.Task SearchAsync_EmptyTerm_ReturnsAllOrderedAsync()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();
            using var ctx = CreateContext(conn);
            ctx.Database.EnsureCreated();

            var x = new Component { Id = Guid.NewGuid(), ComponentCode = "C-200", Name = "Zed", UnitCost = 1m, CurrentAmount = 1m, StorageLocation = "X" };
            var y = new Component { Id = Guid.NewGuid(), ComponentCode = "C-100", Name = "Alpha", UnitCost = 1m, CurrentAmount = 1m, StorageLocation = "X" };
            ctx.Components.AddRange(x, y);
            await ctx.SaveChangesAsync();

            var repo = new ComponentRepository(ctx);

            var results = await repo.SearchAsync(term: string.Empty);

            results.Select(r => r.Name).Should().ContainInOrder("Alpha", "Zed");
        }
    }
}