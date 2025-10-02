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
    }
}
