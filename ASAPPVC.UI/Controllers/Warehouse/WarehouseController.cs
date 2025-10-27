using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers.Warehouse
{
    public class WarehouseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
