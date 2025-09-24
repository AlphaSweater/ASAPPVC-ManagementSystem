using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult ViewCustomer()
        {
            return View();
        }
    }
}
