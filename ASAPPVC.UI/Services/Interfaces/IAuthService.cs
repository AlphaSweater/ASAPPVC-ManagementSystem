using ASAPPVC.UI.Models.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;

namespace ASAPPVC.UI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<SignInResult> LoginAsync(LoginViewModel model);
    }
}
