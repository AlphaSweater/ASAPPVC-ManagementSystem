using ASAPPVC.UI.Models.ViewModels.Reports;

using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult PickingSlip() {
            var viewModel = new PickingSlipViewModel();
            return View(viewModel);
        }
    }
}