using ASAPPVC.UI.Models.ViewModels.Auth;
using ASAPPVC.UI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private const string AuthIndexViewPath = "~/Views/Auth/Login.cshtml";

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToList();

                return RedirectToAction(nameof(Index), new
                {
                    showForm = "login",
                    email = viewModel.Email,
                    loginError = string.Join("|", errors)
                });
            }

            var result = await _authService.LoginAsync(viewModel);

            if (result.Succeeded)
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction(nameof(Index), new
            {
                showForm = "login",
                email = viewModel.Email,
                loginError = "Incorrect email or password used."
            });
        }
    }
}
