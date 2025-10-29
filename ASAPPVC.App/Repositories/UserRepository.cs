using ASAPPVC.App.Data;
using ASAPPVC.App.Models;

namespace ASAPPVC.App.Repositories
{
    #region Interface

    // Inherit common CRUD and Save contract from IBaseRepository<T>
    public interface IUserRepository : IBaseRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetByUserIdAsync(Guid userId);

        Task<ApplicationUser> CreateUserAsync(ApplicationUser newUser);
    }

    #endregion Interface

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