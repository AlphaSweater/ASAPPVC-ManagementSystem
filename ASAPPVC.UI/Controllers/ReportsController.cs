using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult PickingSlip() {
            return View();
        }
    }
}