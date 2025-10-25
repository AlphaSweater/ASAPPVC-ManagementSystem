using ASAPPVC.UI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
    {
        // Customers Table
        public DbSet<CustomerModel> Customers { get; set; }

        // Components Table
        public DbSet<ComponentModel> Components { get; set; }

        // Products and ProductComponents Bridge Tables
        public DbSet<ProductModel> Products { get; set; }

        public DbSet<ProductComponentModel> ProductComponents { get; set; }

        // Orders and OrderProducts Bridge Tables
        public DbSet<OrderModel> Orders { get; set; }

        public DbSet<OrderProductModel> OrderProducts { get; set; }

        // CodeCounters Table for generating sequential codes
        public DbSet<CodeCounters> CodeCounters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductComponentModel>()
                .HasOne(pp => pp.Product)
                .WithMany(p => p.ProductComponents)
                .HasForeignKey(pp => pp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductComponentModel>()
                .HasOne(pp => pp.Component)
                .WithMany()
                .HasForeignKey(pp => pp.ComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderModel>()
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
        }
    }
}