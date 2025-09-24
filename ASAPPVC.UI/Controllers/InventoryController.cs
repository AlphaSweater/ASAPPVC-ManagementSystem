using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    public class InventoryController : Controller
    {
        //Part constants
        private const string AddPartPath = "~/Views/Inventory/Parts/AddPart.cshtml";
        private const string ViewPartPath = "~/Views/Inventory/Parts/ViewPart.cshtml";
        private const string PartInventoryPath = "~/Views/Inventory/Parts/ViewPartInventory.cshtml";

        //Product constants
        private const string AddProductPath = "~/Views/Inventory/Products/AddProduct.cshtml";
        private const string ViewProductPath = "~/Views/Inventory/Products/ViewProduct.cshtml";
        private const string ProductInventoryPath = "~/Views/Inventory/Products/ViewProductInventory.cshtml";


        public IActionResult AddPart()
        {
            return View(AddPartPath);
        }
        public IActionResult viewPart()
        {
            return View(ViewPartPath);
        }
        public IActionResult viewPartInventory()
        {
            return View(PartInventoryPath);
        }

        public IActionResult AddProduct()
        {
            return View(AddProductPath);
        }

        public IActionResult viewProduct()
        {
            return View(ViewProductPath);
        }

        public IActionResult viewProductInventory()
        {
            return View(ProductInventoryPath);
        }
    }
}
