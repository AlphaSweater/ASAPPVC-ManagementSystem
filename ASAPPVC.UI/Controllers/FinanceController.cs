using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    public class FinanceController : Controller
    {
        public IActionResult Transactions()
        {
            return View();
        }
    }
}
