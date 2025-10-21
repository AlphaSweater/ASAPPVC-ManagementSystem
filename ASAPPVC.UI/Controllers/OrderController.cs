using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult AddOrder()
        {
            return View();
        }
        public IActionResult ViewOrders()
        {
            return View();
        }
    }
}
