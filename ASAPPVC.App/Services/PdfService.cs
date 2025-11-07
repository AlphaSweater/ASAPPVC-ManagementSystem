using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace ASAPPVC.App.Services
{
    #region Interface

    public interface IPdfService
    {
        /// <summary>
        /// Converts HTML content to a PDF byte array.
        /// </summary>
        Task<byte[]> HtmlToPdfAsync(string html, CancellationToken ct = default);
    }

    #endregion Interface

    /// <summary>
    /// Service for generating PDFs from HTML using PuppeteerSharp.
    /// </summary>
    public class PdfService : IPdfService
    {
        private static readonly SemaphoreSlim _browserLock = new(1, 1);
        private static IBrowser? _browser;

        public async Task<byte[]> HtmlToPdfAsync(string html, CancellationToken ct = default)
        {
            var browser = await GetBrowserAsync(ct);
            await using var page = await browser.NewPageAsync();

            await page.SetContentAsync(html);

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "12mm",
                    Right = "12mm",
                    Bottom = "12mm",
                    Left = "12mm"
                }
            };

            var pdfBytes = await page.PdfDataAsync(pdfOptions);
            return pdfBytes;
        }

        private static async Task<IBrowser> GetBrowserAsync(CancellationToken ct)
        {
            await _browserLock.WaitAsync(ct);
            try
            {
                if (_browser == null || !_browser.IsConnected)
                {
                    var browserFetcher = new BrowserFetcher();
                    await browserFetcher.DownloadAsync();

                    _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                        Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                    });
                }
                return _browser;
            }
            finally
            {
                _browserLock.Release();
            }
        }
    }
}
