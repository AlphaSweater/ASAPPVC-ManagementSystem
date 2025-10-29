using ASAPPVC.App.Models;
using ASAPPVC.App.Models.General;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.App.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
    {
        // -----------------------------
        // DbSets
        // -----------------------------

        // Customers

        public DbSet<Customer> Customers { get; set; }

        // Warehouse items

        public DbSet<Component> Components { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductComponent> ProductComponents { get; set; }

        // Orders

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }

        // Utilities

        public DbSet<CodeCounters> CodeCounters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure domain entities
            ConfigureProductComponent(modelBuilder);
            ConfigureOrderProduct(modelBuilder);
            ConfigureOrder(modelBuilder);
            ConfigureOwnedTypes(modelBuilder);
        }

        // -----------------------------
        // Entity configuration helpers
        // -----------------------------

        private static void ConfigureProductComponent(ModelBuilder modelBuilder)
        {
            var b = modelBuilder.Entity<ProductComponent>();

            // Composite PK (ProductId, ComponentId)
            b.HasKey(pc => new { pc.ProductId, pc.ComponentId });

            // Relationships
            b.HasOne(pc => pc.Product)
                .WithMany(p => p.ProductComponents)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(pc => pc.Component)
                .WithMany(c => c.ProductComponents)
                .HasForeignKey(pc => pc.ComponentId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureOrderProduct(ModelBuilder modelBuilder)
        {
            var b = modelBuilder.Entity<OrderProduct>();

            // Composite PK (OrderId, ProductId)
            b.HasKey(op => new { op.OrderId, op.ProductId });

            // Relationships
            b.HasOne(op => op.Order)
                .WithMany(o => o.OrderProducts)
                .HasForeignKey(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(op => op.Product)
                .WithMany()
                .HasForeignKey(op => op.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureOrder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureOwnedTypes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().OwnsOne(p => p.Image, b =>
            {
                b.Property(i => i.ContentType).HasMaxLength(64);
                b.Property(i => i.Sha256).HasMaxLength(64);
            });

            modelBuilder.Entity<Component>().OwnsOne(c => c.Image, b =>
            {
                b.Property(i => i.ContentType).HasMaxLength(64);
                b.Property(i => i.Sha256).HasMaxLength(64);
            });
        }
    }
}