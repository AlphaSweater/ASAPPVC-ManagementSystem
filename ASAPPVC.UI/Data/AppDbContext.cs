using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserProfileModel> User { get; set; }
        public DbSet<PartModel> Part { get; set; }
        public DbSet<ProductModel> Product { get; set; }
    }
}
