using ASAPPVC.App.Models;
using ASAPPVC.App.Services;
using ASAPPVC.App.ViewModels.Reports;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.App.Controllers
{
    public class ReportsController(IOrderService orderService, IOrderMapper orderMapper) : Controller
    {
        private readonly IOrderService _orders = orderService;
        private readonly IOrderMapper _mapper = orderMapper;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // generates and displays a picking slip for the specified order ID
        [HttpGet]
        public async Task<IActionResult> PickingSlip(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty) return BadRequest("Order ID is required.");

            var result = await _orders.GetFullDomainAsync(id, ct);

            if (!result.Ok || result.Value is null)
                return NotFound(result.Error ?? "Order not found.");

            PickingSlipViewModel vm = _mapper.ToPickingSlipVm(result.Value);
            return View("~/Views/Reports/PickingSlip.cshtml", vm);
        }
    }
}