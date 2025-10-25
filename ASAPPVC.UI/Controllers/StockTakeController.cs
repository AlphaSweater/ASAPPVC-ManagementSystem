using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    public class StockTakeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
