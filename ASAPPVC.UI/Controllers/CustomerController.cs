using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        public IActionResult CreateCustomer()
        {
            return View();
        }

        public IActionResult ViewCustomer()
        {
            return View();
        }
    }
}