using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    public class UserRepository(AppDbContext context) : BaseRepository<ApplicationUser>(context), IUserRepository
    {
        public async Task<ApplicationUser?> GetByUserIdAsync(Guid userId)
        {
            return await GetByIdAsync(userId);
        }

        public async Task<ApplicationUser> CreateUserAsync(ApplicationUser newUser)
        {
            var completeUser = await AddAsync(newUser);
            await SaveAsync();

            return completeUser;
        }
    }
}