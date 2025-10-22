// Controllers/AuthController.cs
using ASAPPVC.UI.Models.ViewModels.Auth;
using ASAPPVC.UI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    public class AuthController : Controller
    {
        //─────────── Dependencies ───────────\\
        private readonly IAuthService _auth;

        //constructor
        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays the login view
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            ViewData["HideNavbar"] = true;
            return View(); 
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //method that allows users to log in
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["HideNavbar"] = true;
                return View(vm);
            }

            var result = await _auth.LoginAsync(vm);
            if (result.Succeeded)
                return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? Redirect(returnUrl)
                    : RedirectToAction("Index", "Home");

            ModelState.AddModelError(string.Empty, "Incorrect email or password.");
            ViewData["HideNavbar"] = true;
            return View(vm);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays the register view
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            ViewData["HideNavbar"] = true;
            return View(); 
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //method that allows users to register
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewData["HideNavbar"] = true;
                return View(vm);
            }

            var result = await _auth.RegisterAsync(vm);
            if (result.Succeeded) return RedirectToAction(nameof(Login));

            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            ViewData["HideNavbar"] = true;
            return View(vm);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //method that allows users to log out
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _auth.LogoutAsync();
            return RedirectToAction(nameof(Login));
        }
    }
}
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\