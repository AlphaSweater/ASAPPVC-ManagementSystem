using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers.Warehouse
{
    public class StockTakeController : Controller
    {
        public IActionResult StockTake()
        {
            return View();
        }
    }
}
