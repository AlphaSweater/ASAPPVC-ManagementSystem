using ASAPPVC.UI.Models.ViewModels.Customer;
using ASAPPVC.UI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customers;
        public CustomerController(ICustomerService customers) => _customers = customers;

        [HttpGet]
        public IActionResult CreateCustomer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomer(CreateCustomerViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var (ok, error, customer) = await _customers.CreateAsync(vm, ct);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, error ?? "Unable to create customer.");
                return View(vm);
            }

            TempData["AlertMessage"] = $"Customer '{customer!.Name} {customer.Name}' created.";
            return RedirectToAction(nameof(ViewCustomer), new { id = customer.CustomerID });
        }

        [HttpGet]
        public IActionResult ViewCustomer()
        {
            return View();
        }
    }
}