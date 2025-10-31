using ASAPPVC.App.Models;
using ASAPPVC.App.Models.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace ASAPPVC.App.Services
{
    public interface IStockAlertServices
    {
        Task NotifyOnCreateAsync(Component component, CancellationToken ct = default);
        Task NotifyOnUpdateAsync(Component component, ReorderStatus previousStatus, ReorderStatus newStatus, CancellationToken ct = default);
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
            if (!ShouldNotify(status))
                return Task.CompletedTask;

            var subject = $"Stock {status}: {component.ComponentName} ({component.ComponentCode})";
            var body = BuildBody(component, status, extra: null);
            return SendOrLog(subject, body);
        }

        public Task NotifyOnUpdateAsync(Component component, ReorderStatus previousStatus, ReorderStatus newStatus, CancellationToken ct = default)
        {
            if (component is null)
                return Task.CompletedTask;

            _logger.LogDebug("Checking stock status change - Component: {Component}, Previous: {PreviousStatus}, New: {NewStatus}", 
                $"{component.ComponentName} ({component.ComponentCode})", previousStatus, newStatus);

            // Only notify when status worsens and target status is one we notify for
            if (newStatus <= previousStatus || !ShouldNotify(newStatus))
            {
                _logger.LogDebug("Skipping notification - status did not worsen or is not tracked. Previous: {PreviousStatus}, New: {NewStatus}", 
                    previousStatus, newStatus);
                return Task.CompletedTask;
            }

            var subject = $"Stock {newStatus}: {component.ComponentName} ({component.ComponentCode})";
            var extra = $"Previous Status: {previousStatus}";
            var body = BuildBody(component, newStatus, extra);
            return SendOrLog(subject, body);
        }

        private static bool ShouldNotify(ReorderStatus status)
        {
            return status == ReorderStatus.Approaching
                || status == ReorderStatus.Low
                || status == ReorderStatus.Critical
                || status == ReorderStatus.OutOfStock;
        }

        private static string BuildBody(Component c, ReorderStatus status, string? extra)
        {
            var lines = new List<string>
            {
                $"Component: {c.ComponentName} ({c.ComponentCode})",
                $"Quantity On Hand: {c.QuantityOnHand}",
                $"Reorder Level: {c.ReorderLevel}",
                $"Status: {status}"
            };
            if (!string.IsNullOrWhiteSpace(extra)) lines.Add(extra);
            if (status == ReorderStatus.Approaching)
                lines.Add("Action: Consider reordering to prevent low stock.");
            if (status == ReorderStatus.Low || status == ReorderStatus.Critical || status == ReorderStatus.OutOfStock)
                lines.Add("Action: Reorder immediately.");
            return string.Join('\n', lines);
        }

        private Task SendOrLog(string subject, string body)
        {
            var ownerEmail = _configuration["Notifications:OwnerEmail"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(ownerEmail))
            {
                _logger.LogWarning("Stock alert not sent: Notifications:OwnerEmail is not configured.");
                return Task.CompletedTask;
            }

            var host = _configuration["Notifications:Smtp:Host"];
            var portStr = _configuration["Notifications:Smtp:Port"];
            var user = _configuration["Notifications:Smtp:User"];
            var pass = _configuration["Notifications:Smtp:Password"];
            var enableSslStr = _configuration["Notifications:Smtp:EnableSsl"];
            var from = _configuration["Notifications:FromEmail"] ?? "noreply@localhost";

            if (string.IsNullOrWhiteSpace(host))
            {
                // No SMTP configured: log the email for visibility
                _logger.LogInformation("[Email -> {Owner}] {Subject}\n{Body}", ownerEmail, subject, body);
                return Task.CompletedTask;
            }

            try
            {
                var port = int.TryParse(portStr, out var p) ? p : 25;
                var enableSsl = bool.TryParse(enableSslStr, out var ssl) && ssl;

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl,
                    Credentials = !string.IsNullOrWhiteSpace(user) ? new NetworkCredential(user, pass) : CredentialCache.DefaultNetworkCredentials
                };

                var mail = new MailMessage(from, ownerEmail)
                {
                    Subject = subject,
                    Body = body
                };

                client.Send(mail);
                _logger.LogInformation("Stock alert sent via SMTP to {Owner}: {Subject}", ownerEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send stock alert email. Falling back to logging. Subject: {Subject}", subject);
                _logger.LogInformation("[Email -> {Owner}] {Subject}\n{Body}", ownerEmail, subject, body);
            }

            return Task.CompletedTask;
        }
    }
}
