using ASAPPVC.App.ViewModels.Reports;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.App.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult PickingSlip()
        {
            var viewModel = new PickingSlipViewModel();
            return View(viewModel);
        }
    }
}