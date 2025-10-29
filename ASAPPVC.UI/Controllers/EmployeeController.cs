using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.App.Controllers
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