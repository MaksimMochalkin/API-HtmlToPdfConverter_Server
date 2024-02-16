using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;

namespace API_HtmlToPdfConverter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : Controller
    {
        private const string StorageConnectionString = "put_connection_string";
        private const string QueueConnectionString = "put_connection_string";
        private const string HtmlFilesContainerName = "htmlfilescontainer";
        private const string PdfFilesContainerName = "pdffilescontainer";
        private const string QueueName = "htmlfilesqueue";

        [HttpGet("getPdfLink")]
        public async Task<IActionResult> GetPdfLinkFromAzureService(string htmlBlobName)
        {
            var pdfDownloadLink = "https://www.digitalocean.com/community/tutorials/react-axios-react";
            return Ok(new { pdfDownloadLink });
            var htmlStream = await DownloadFileFromStorage(htmlBlobName).ConfigureAwait(false);

            var pdfBytes = await ConvertHtmlToPdfAsync(htmlStream).ConfigureAwait(false);

            var pdfBlobName = $"{Guid.NewGuid()}_output.pdf";
            var downloadUrl = await UploadFileToStorage(pdfBytes, pdfBlobName).ConfigureAwait(false);
            return Ok(new { downloadUrl });
        }

        private async Task<Stream> DownloadFileFromStorage(string blobName)
        {
            var blobServiceClient = new BlobServiceClient(StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(HtmlFilesContainerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.OpenReadAsync();

            return response;
        }

        private async Task<byte[]> ConvertHtmlToPdfAsync(Stream htmlStream)
        {
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe", // Specify the correct path to chrome.exe
            };

            using (var browser = await Puppeteer.LaunchAsync(launchOptions))
            using (var page = await browser.NewPageAsync())
            {
                var htmlContent = await new StreamReader(htmlStream).ReadToEndAsync();
                await page.SetContentAsync(htmlContent);

                return await page.PdfDataAsync();
            }
        }

        private async Task<Uri> UploadFileToStorage(byte[] fileBytes, string blobName)
        {
            // Use Azure Storage SDK to upload the PDF file to Azure Storage
            var blobServiceClient = new BlobServiceClient(StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(PdfFilesContainerName);
            
            using (var stream = new MemoryStream(fileBytes))
            {
                await containerClient.UploadBlobAsync(blobName, stream).ConfigureAwait(false);
            }

            var downloadUrl = containerClient.GenerateSasUri(Azure.Storage.Sas.BlobContainerSasPermissions.Read,
                DateTimeOffset.UtcNow.AddMinutes(5));

            return downloadUrl;
        }
    }
}
