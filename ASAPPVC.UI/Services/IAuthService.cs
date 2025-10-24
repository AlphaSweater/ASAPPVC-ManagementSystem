using ASAPPVC.UI.Models.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;

namespace ASAPPVC.UI.Services
{
    public interface IAuthService
    {
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task<IdentityResult> RegisterAsync(RegisterViewModel model);
        Task LogoutAsync();
    }
}
