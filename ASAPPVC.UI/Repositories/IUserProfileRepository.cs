using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    // Inherit common CRUD and Save contract from IBaseRepository<T>
    public interface IUserProfileRepository : IBaseRepository<UserProfileModel>
    {
    }
}