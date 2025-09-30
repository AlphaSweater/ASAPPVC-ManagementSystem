using ASAPPVC.UI.Data;
using ASAPPVC.UI.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories.Implementation
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
