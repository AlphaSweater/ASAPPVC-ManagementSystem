using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    public class UserProfileRepository(AppDbContext context) : BaseRepository<UserProfileModel>(context), IUserProfileRepository
    {
    }
}