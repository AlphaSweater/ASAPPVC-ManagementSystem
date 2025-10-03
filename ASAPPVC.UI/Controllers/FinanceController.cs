using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    [Authorize]
    public class FinanceController : Controller
    {
        public IActionResult Transactions()
        {
            return View();
        }
    }
}
