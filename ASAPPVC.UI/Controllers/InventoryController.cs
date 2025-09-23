using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    public class InventoryController : Controller
    {
        public IActionResult AddPart()
        {
            return View();
        }
    }
}
