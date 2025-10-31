using ASAPPVC.App.Models;
using ASAPPVC.App.Models.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ASAPPVC.App.Services
{
    public interface IStockAlertServices
    {
        Task NotifyOnCreateAsync(Component component, CancellationToken ct = default);
    }

    public sealed class StockAlertServices : IStockAlertServices
    {
        private readonly ILogger<StockAlertServices> _logger;
        private readonly IConfiguration _configuration;

        public StockAlertServices(ILogger<StockAlertServices> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task NotifyOnCreateAsync(Component component, CancellationToken ct = default)
        {
            if (component is null)
                return Task.CompletedTask;

            var status = ReorderStatusPolicy.Evaluate(component.QuantityOnHand, component.ReorderLevel);
            if (status != ReorderStatus.Approaching)
                return Task.CompletedTask;

            var ownerEmail = _configuration["Notifications:OwnerEmail"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(ownerEmail))
            {
                _logger.LogWarning("Stock alert not sent: Notifications:OwnerEmail is not configured.");
                return Task.CompletedTask;
            }

            // Simulate sending email (no SMTP available). Log the outgoing message.
            var subject = $"Stock Approaching: {component.ComponentName} ({component.ComponentCode})";
            var body =
                $"Component: {component.ComponentName} ({component.ComponentCode})\n" +
                $"Quantity On Hand: {component.QuantityOnHand}\n" +
                $"Reorder Level: {component.ReorderLevel}\n" +
                $"Status: {status}\n" +
                "Action: Consider reordering to prevent low stock.";

            _logger.LogInformation("[Email -> {Owner}] {Subject}\n{Body}", ownerEmail, subject, body);
            return Task.CompletedTask;
        }
    }
}
