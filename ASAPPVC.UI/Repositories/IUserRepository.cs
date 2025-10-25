using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    // Inherit common CRUD and Save contract from IBaseRepository<T>
    public interface IUserRepository : IBaseRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetByUserIdAsync(Guid userId);

        Task<ApplicationUser> CreateUserAsync(ApplicationUser newUser);
    }
}