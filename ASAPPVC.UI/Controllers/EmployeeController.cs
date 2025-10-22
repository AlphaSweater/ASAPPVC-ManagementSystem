using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        //constructor
        public IActionResult Index()
        {
            return View();
        }
    }
}
