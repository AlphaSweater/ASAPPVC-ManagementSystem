using ASAPPVC.App.Models;
using ASAPPVC.App.Services;
using ASAPPVC.App.ViewModels.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ASAPPVC.App.Controllers
{
    public class ReportsController(
        IOrderService orderService,
        IOrderMapper orderMapper,
        IPdfService pdfService,
        ICompositeViewEngine viewEngine,
        IWebHostEnvironment env) : Controller
    {
        private readonly IOrderService _orders = orderService;
        private readonly IOrderMapper _mapper = orderMapper;
        private readonly IPdfService _pdfService = pdfService;
        private readonly ICompositeViewEngine _viewEngine = viewEngine;
        private readonly IWebHostEnvironment _env = env;

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // generates and downloads a picking slip PDF for the specified order ID
        [HttpGet]
        public async Task<IActionResult> PickingSlip(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty)
                return BadRequest("Order ID is required.");

            var result = await _orders.GetFullDomainAsync(id, ct);

            if (!result.Ok || result.Value is null)
                return NotFound(result.Error ?? "Order not found.");

            PickingSlipViewModel vm = _mapper.ToPickingSlipVm(result.Value);

            // Render view to HTML string
            string html = await RenderViewToStringAsync("PickingSlip", vm);

            // Convert image paths to absolute file paths for PuppeteerSharp
            html = ConvertImagePathsToAbsolute(html);

            // Convert HTML to PDF
            byte[] pdfBytes = await _pdfService.HtmlToPdfAsync(html, ct);

            // Return PDF as download
            string fileName = $"PickingSlip-{vm.PickingSlipNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            await using var sw = new StringWriter();
            var viewResult = _viewEngine.FindView(ControllerContext, viewName, isMainPage: true);

            if (!viewResult.Success)
            {
                throw new InvalidOperationException($"View '{viewName}' not found.");
            }

            var viewContext = new ViewContext(
                     ControllerContext,
               viewResult.View,
              ViewData,
                          TempData,
              sw,
               new HtmlHelperOptions()
                      );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }

        private string ConvertImagePathsToAbsolute(string html)
        {
            // Convert image paths to absolute file paths for PuppeteerSharp
            var wwwrootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
            return System.Text.RegularExpressions.Regex.Replace(
                html,
                @"src=""~/([^""]+)""",
                match =>
                {
                    var relativePath = match.Groups[1].Value.Replace("/", "\\");
                    var absolutePath = Path.Combine(wwwrootPath, relativePath);
                    return $"src=\"file:///{absolutePath.Replace("\\", "/")}\"";
                }
            );
        }
    }
}