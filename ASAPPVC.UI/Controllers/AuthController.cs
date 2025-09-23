using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
