using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        public IActionResult ViewCustomer()
        {
            return View();
        }
    }
}
