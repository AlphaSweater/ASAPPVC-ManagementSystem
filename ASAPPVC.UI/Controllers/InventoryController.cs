using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    public class InventoryController : Controller
    {
        private const string AddPartPath = "~/Views/Inventory/Parts/AddPart.cshtml";
        private const string ViewPartPath = "~/Views/Inventory/Parts/ViewPart.cshtml";
        private const string PartInventoryPath = "~/Views/Inventory/Parts/ViewPartInventory.cshtml";
        public IActionResult AddPart()
        {
            return View(AddPartPath);
        }
        public IActionResult viewPart()
        {
            return View(ViewPartPath);
        }
        public IActionResult viewInventory()
        {
            return View(PartInventoryPath);
        }
    }
}
