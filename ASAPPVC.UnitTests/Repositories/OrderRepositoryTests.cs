using ASAPPVC.App.Data;
using ASAPPVC.App.Models;
using ASAPPVC.App.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UnitTests.Repositories
{
    public class OrderRepositoryTests
    {
        private static AppDbContext CreateContext(SqliteConnection conn)
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conn)
                .Options;
            return new AppDbContext(opts);
        }

        // ---------------- GetByIdOrCodeWithDetails: includes customer and products ----------------
        [Fact]
        public async System.Threading.Tasks.Task GetByIdOrCodeWithDetailsAsync_IncludesDetailsAsync()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();
            using var ctx = CreateContext(conn);
            ctx.Database.EnsureCreated();

            var customer = new Customer { Id = Guid.NewGuid(), Name = "John", Surname = "Doe", PhoneNumber = "123", Email = "a@b.com" };
            var product = new Product { Id = Guid.NewGuid(), ProductCode = "PRD-1", Name = "Widget", Price = 5m, Description = "D" };

            var order = new Order { Id = Guid.NewGuid(), OrderCode = "ORD-001", CustomerId = customer.Id, OrderDate = DateTime.UtcNow, OrderStatus = default };
            var op = new OrderProduct { OrderId = order.Id, ProductId = product.Id, Quantity = 2 };

            // set navigation properties so EF will wire them if tracked
            order.OrderProducts = new List<OrderProduct> { op };
            op.Product = product;

            ctx.Customers.Add(customer);
            ctx.Products.Add(product);
            ctx.Orders.Add(order);
            ctx.OrderProducts.Add(op);
            await ctx.SaveChangesAsync();

            var repo = new OrderRepository(ctx);

            var fetched = await repo.GetByIdOrCodeWithDetailsAsync(id: order.Id);

            fetched.Should().NotBeNull();
            fetched!.Customer.Should().NotBeNull();
            fetched.OrderProducts.Should().ContainSingle();
            fetched.OrderProducts.First().Product.Should().NotBeNull();
        }
    }
}