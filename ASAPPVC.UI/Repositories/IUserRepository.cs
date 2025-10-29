using ASAPPVC.App.Models;

namespace ASAPPVC.App.Repositories
{
    // Inherit common CRUD and Save contract from IBaseRepository<T>
    public interface IUserRepository : IBaseRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetByUserIdAsync(Guid userId);

        Task<ApplicationUser> CreateUserAsync(ApplicationUser newUser);
    }
}