using ASAPPVC.App.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;

namespace ASAPPVC.App.Services
{
    public interface IAuthService
    {
        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task<IdentityResult> RegisterAsync(RegisterViewModel model);

        Task LogoutAsync();
    }
}