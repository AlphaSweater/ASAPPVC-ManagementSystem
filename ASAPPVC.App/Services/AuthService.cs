using ASAPPVC.App.Models;
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

        // Returns the current authenticated user's Id or null when not available
        Task<Guid?> GetCurrentUserIdAsync(CancellationToken ct = default);
    }

    #endregion Interface

    public class AuthService : IAuthService
    {
        //─────────── Dependencies ───────────\\
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly IHttpContextAccessor? _http;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor? httpContextAccessor = null)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _http = httpContextAccessor;
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

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Returns the current authenticated user's Guid Id, or null if not available
        public Task<Guid?> GetCurrentUserIdAsync(CancellationToken ct = default)
        {
            try
            {
                var ctx = _http?.HttpContext;
                var user = ctx?.User;
                if (user == null)
                    return Task.FromResult<Guid?>(null);

                // Use UserManager helper to get the id string (works with Identity types)
                var idStr = _userManager.GetUserId(user);
                if (string.IsNullOrWhiteSpace(idStr))
                    return Task.FromResult<Guid?>(null);

                if (Guid.TryParse(idStr, out var gid))
                    return Task.FromResult<Guid?>(gid);

                return Task.FromResult<Guid?>(null);
            }
            catch
            {
                return Task.FromResult<Guid?>(null);
            }
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\