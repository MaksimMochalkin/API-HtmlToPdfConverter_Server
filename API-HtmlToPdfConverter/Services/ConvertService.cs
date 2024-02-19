namespace API_HtmlToPdfConverter.Services
{
    using API_HtmlToPdfConverter.Interfaces;
    using PuppeteerSharp;

    public class ConvertService : IConvertService
    {
        public async Task<byte[]> ConvertHtmlToPdfAsync(Stream htmlStream)
        {
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = "path to chrome.exe",
            };

            using (var browser = await Puppeteer.LaunchAsync(launchOptions))
            using (var page = await browser.NewPageAsync())
            {
                var htmlContent = await new StreamReader(htmlStream).ReadToEndAsync();
                await page.SetContentAsync(htmlContent);

                return await page.PdfDataAsync();
            }
        }
    }
}
