using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UnitTests.Repositories
{
    public class UserRepositoryTests
    {
        private static AppDbContext CreateContext(SqliteConnection conn)
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conn)
                .Options;
            return new AppDbContext(opts);
        }

        // ---------------- CreateUser: persists and can be retrieved ----------------
        [Fact]
        public async System.Threading.Tasks.Task CreateUserAsync_PersistsAndRetrievableAsync()
        {
            using var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();
            using var ctx = CreateContext(conn);
            ctx.Database.EnsureCreated();

            var repo = new UserRepository(ctx);

            var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "jsmith", Email = "jsmith@x.com", FirstName = "John", LastName = "Smith", IsActive = true, PreferencesJson = "{}" };

            var created = await repo.CreateUserAsync(user);

            created.Should().NotBeNull();
            created.Id.Should().Be(user.Id);

            var fetched = await repo.GetByUserIdAsync(user.Id);
            fetched.Should().NotBeNull();
            fetched!.Email.Should().Be("jsmith@x.com");
        }
    }
}