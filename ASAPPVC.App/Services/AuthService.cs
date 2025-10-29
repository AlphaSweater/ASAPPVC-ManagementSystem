using ASAPPVC.App.Models;
using ASAPPVC.App.Repositories;
using ASAPPVC.App.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;

namespace ASAPPVC.App.Services
{
    #region Interface

    public interface IAuthService
    {
        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task<IdentityResult> RegisterAsync(RegisterViewModel model);

        Task LogoutAsync();
    }

    #endregion Interface

    public class AuthService : IAuthService
    {
        //─────────── Dependencies ───────────\\
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly IUserRepository _userRepository;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IUserRepository userRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userRepository = userRepository;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //gets user from database and signs them in
        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //Adds user to database
        public async Task<IdentityResult> RegisterAsync(RegisterViewModel viewModel)
        {
            var newUser = ApplicationUser.Create(viewModel);

            var result = await _userManager.CreateAsync(newUser, viewModel.Password);
            if (!result.Succeeded)
                return result;

            try
            {
                // Only assign role after creation succeeds
                await _userManager.AddToRoleAsync(newUser, viewModel.Role.ToString());
            }
            catch
            {
                // Rollback user creation if profile creation fails
                await _userManager.DeleteAsync(newUser);
                var profileError = new IdentityError
                {
                    Code = "ProfileCreationFailed",
                    Description = "User account was created, but profile creation failed. The account has been removed."
                };
                return IdentityResult.Failed(profileError);
            }

            return result;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //signs user out
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\