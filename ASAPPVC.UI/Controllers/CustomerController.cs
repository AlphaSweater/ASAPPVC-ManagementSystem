using ASAPPVC.UI.Services;
using ASAPPVC.UI.ViewModels.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        //─────────── Dependencies ───────────\\
        private readonly ICustomerService _customers;

        //constructor
        public CustomerController(ICustomerService customers)
        {
            _customers = customers;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays the create customer view
        [HttpGet]
        public IActionResult CreateCustomer()
        {
            return View();
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //handles the submission of the create customer form
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
            return RedirectToAction(nameof(ViewCustomer), new { id = customer.Id });
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays the list of customers
        [HttpGet]
        public async Task<IActionResult> ViewCustomer(CancellationToken ct)
        {
            var list = await _customers.ListAsync(ct);
            return View(list);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\