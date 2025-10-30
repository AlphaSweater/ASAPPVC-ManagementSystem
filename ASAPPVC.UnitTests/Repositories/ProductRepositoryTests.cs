using ASAPPVC.App.Data;
using ASAPPVC.App.Models;
using ASAPPVC.App.Models.Enums;
using ASAPPVC.App.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UnitTests.Repositories
{
    public class ProductRepositoryTests
    {
        private static AppDbContext CreateContext(SqliteConnection conn)
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conn)
                .Options;
            return new AppDbContext(opts);
        }

        // ---------------- GetByIdOrCodeWithComponents: includes components ----------------
        [Fact]
        public async Task GetByIdOrCodeWithComponentsAsync_IncludesComponentsAsync()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();
            using var ctx = CreateContext(conn);
            ctx.Database.EnsureCreated();

            var comp = new Component { Id = Guid.NewGuid(), ComponentCode = "CMP-1", ComponentName = "Screw", UnitCost = 0.1m, QuantityOnHand = 100m, LocationCode = "L-1" };
            var product = new Product { Id = Guid.NewGuid(), ProductCode = "PRD-1", ProductName = "Panel", SellingPrice = 10m, Description = "Desc" };

            var pc = new ProductComponent { ComponentId = comp.Id, ProductId = product.Id, RequiredQuantity = 2m, UnitOfMeasure = Unit.Piece };

            // set navigation properties so EF will wire them if tracked
            product.ProductComponents = new List<ProductComponent> { pc };
            pc.Component = comp;
            pc.Product = product;

            ctx.Components.Add(comp);
            ctx.Products.Add(product);
            ctx.ProductComponents.Add(pc);
            await ctx.SaveChangesAsync();

            var repo = new ProductRepository(ctx);

            var fetchedById = await repo.GetByIdOrCodeWithComponentsAsync(id: product.Id);

            fetchedById.Should().NotBeNull();
            fetchedById!.ProductComponents.Should().NotBeNull();
            fetchedById.ProductComponents.Should().ContainSingle();
            fetchedById.ProductComponents.First().Component.Should().NotBeNull();
            fetchedById.ProductComponents.First().Component!.ComponentCode.Should().Be("CMP-1");
        }
    }
}