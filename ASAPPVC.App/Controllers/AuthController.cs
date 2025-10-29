// Controllers/AuthController.cs
using ASAPPVC.App.Services;
using ASAPPVC.App.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.App.Controllers
{
    public class AuthController(IAuthService auth) : Controller
    {
        // Base path for auth views - keep view references centralized so they can be changed easily
        private const string ViewRoot = "~/Views/Auth/";

        // View path constants

        private const string LoginViewName = ViewRoot + "Login.cshtml";
        private const string RegisterViewName = ViewRoot + "Register.cshtml";

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Dependency injections

        private readonly IAuthService _auth = auth;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(LoginViewName);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // method that allows users to log in
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(LoginViewName, vm);

            var result = await _auth.LoginAsync(vm);
            if (result.Succeeded)
                return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? Redirect(returnUrl)
                    : RedirectToAction("Index", "Home");

            ModelState.AddModelError(string.Empty, "Incorrect email or password.");
            return View(LoginViewName, vm);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // displays the register view
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View(RegisterViewName);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // method that allows users to register
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(RegisterViewName, vm);

            var result = await _auth.RegisterAsync(vm);
            if (result.Succeeded)
                return RedirectToAction(nameof(Index));

            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(RegisterViewName, vm);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // method that allows users to log out
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _auth.LogoutAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\