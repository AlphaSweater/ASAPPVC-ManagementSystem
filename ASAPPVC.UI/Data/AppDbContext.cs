using ASAPPVC.UI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace ASAPPVC.UI.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
    {
        // Customers Table
        public DbSet<CustomerModel> Customers { get; set; }

        // Components Table
        public DbSet<Component> Components { get; set; }

        // Products and ProductComponents Bridge Tables
        public DbSet<Product> Products { get; set; }

        public DbSet<ProductComponent> ProductComponents { get; set; }

        // Orders and OrderProducts Bridge Tables
        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderProductModel> OrderProducts { get; set; }

        // CodeCounters Table for generating sequential codes
        public DbSet<CodeCounters> CodeCounters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductComponent>()
                .HasOne(pp => pp.Product)
                .WithMany(p => p.ProductComponents)
                .HasForeignKey(pp => pp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductComponent>()
                .HasOne(pp => pp.Component)
                .WithMany()
                .HasForeignKey(pp => pp.ComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderProductModel>()
                .HasOne(op => op.Order)
                .WithMany(o => o.OrderProducts)
                .HasForeignKey(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderProductModel>()
                .HasOne(op => op.Product)
                .WithMany()
                .HasForeignKey(op => op.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure JSON storage for Modifiers on Product using helper methods to avoid expression tree optional-arg issues
            var modifiersConverter = new ValueConverter<ProductModifiers, string>(
                v => SerializeModifiers(v),
                v => DeserializeModifiers(v));

            modelBuilder.Entity<Product>()
                .Property(p => p.Modifiers)
                .HasConversion(modifiersConverter)
                .HasColumnType("TEXT")
                .IsRequired(false);
        }

        // Helpers used by the ValueConverter (must be static to be usable in expression trees)
        private static string SerializeModifiers(ProductModifiers? mods)
        {
            return JsonSerializer.Serialize(mods ?? new ProductModifiers());
        }

        private static ProductModifiers DeserializeModifiers(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new ProductModifiers();

            return JsonSerializer.Deserialize<ProductModifiers>(json) ?? new ProductModifiers();
        }
    }
}