using ASAPPVC.UI.Models.ViewModels.Auth;
using ASAPPVC.UI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ASAPPVC.UI.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthService(UserManager<IdentityUser> userManager,
                           SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
            => await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return result;

            return result;
        }

        public async Task LogoutAsync() => await _signInManager.SignOutAsync();

    }
}
