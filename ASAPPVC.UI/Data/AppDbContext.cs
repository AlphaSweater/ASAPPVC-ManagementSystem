using ASAPPVC.UI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserProfileModel> UserProfiles { get; set; }
        public DbSet<PartModel> Part { get; set; }
        public DbSet<ProductModel> Product { get; set; }
        public DbSet<CustomerModel> Customer { get; set; }
        public DbSet<OrderModel> Order { get; set; }
        public DbSet<OrderProductModel> OrderProduct { get; set; }
        public DbSet<ProductPartModel> ProductPart { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductPartModel>()
                .HasOne(pp => pp.Product)
                .WithMany(p => p.ProductParts)
                .HasForeignKey(pp => pp.ProductID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductPartModel>()
                .HasOne(pp => pp.Part)
                .WithMany()
                .HasForeignKey(pp => pp.PartID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.Customer)
                .WithMany() 
                .HasForeignKey(o => o.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderProductModel>()
                .HasOne(op => op.Order)
                .WithMany(o => o.OrderProducts)
                .HasForeignKey(op => op.OrderID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderProductModel>()
                .HasOne(op => op.Product)
                .WithMany()
                .HasForeignKey(op => op.ProductID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}