using ASAPPVC.UI.Data;
using Microsoft.AspNetCore.Identity;
using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories.Interfaces
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly AppDbContext _context;
        public UserProfileRepository(AppDbContext context)
        {
            _context = context;
        }
    }
}
