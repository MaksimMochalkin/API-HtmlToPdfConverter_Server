namespace API_HtmlToPdfConverter.Controllers
{
    using Azure.Storage.Blobs;
    using Azure.Storage.Queues;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
    using PuppeteerSharp;

    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : Controller
    {
        private const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=maximmstorageaccount;AccountKey=ZL4D6WagjQhhYaC6t00hMGbF0YnjM4uAevlIjPEdYL08n/Yl2O3ZSvI0y4ucwhWHSXjR2DbKb2al+AStILNsTg==;EndpointSuffix=core.windows.net";
        private const string QueueConnectionString = "DefaultEndpointsProtocol=https;AccountName=maximmstorageaccount;AccountKey=ZL4D6WagjQhhYaC6t00hMGbF0YnjM4uAevlIjPEdYL08n/Yl2O3ZSvI0y4ucwhWHSXjR2DbKb2al+AStILNsTg==;EndpointSuffix=core.windows.net";
        private const string ContainerName = "htmlfilescontainer";
        private const string QueueName = "htmlfilesqueue";
        private readonly IHubContext<PdfHub> _pdfHubContext;

        private readonly QueueServiceClient _queueServiceClient;
        private readonly BlobServiceClient _blobServiceClient;

        public PdfController(IHubContext<PdfHub> pdfHubContext)
        {
            _pdfHubContext = pdfHubContext;
        }
        //public PdfController(QueueServiceClient queueServiceClient,
        //    BlobServiceClient blobServiceClient)
        //{
        //    _blobServiceClient = blobServiceClient;
        //    _queueServiceClient = queueServiceClient;
        //}

        [HttpPost]
        public async Task<IActionResult> ConvertHtmlToPdf(IFormFile htmlFile)
        {
            try
            {
                if (htmlFile == null || htmlFile.Length == 0)
                {
                    return BadRequest("Invalid HTML file");
                }

                // Upload HTML file to Azure Storage
                var htmlBlobName = $"{Guid.NewGuid()}_input.html";
                var pdfBlobName = $"{Guid.NewGuid()}_output.pdf";
                await UploadFileToStorage(htmlFile.OpenReadStream(), htmlBlobName);

                // Enqueue a message for background processing
                await EnqueueMessage(htmlBlobName, pdfBlobName);


                await _pdfHubContext.Clients.All.SendAsync("ConversionStarted");
                var pdfBytes = await ConvertHtmlToPdfAsync(htmlFile.OpenReadStream());
                return File(pdfBytes, "application/pdf", "output.pdf");

                // Return immediate response to the client
                //return Ok("Conversion request received. PDF will be generated.");
            }
            catch (Exception ex)
            {
                // Handle exceptions and return appropriate response
                return BadRequest($"Error: {ex.Message}");
            }
        }

        private async Task UploadFileToStorage(Stream fileStream, string blobName)
        {
            var blobServiceClient = new BlobServiceClient(StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
            //var htmlBlobClient = containerClient.GetBlobClient(blobName);
            await containerClient.UploadBlobAsync(blobName, fileStream).ConfigureAwait(false);
            //await htmlBlobClient.UploadAsync(fileStream);
            // Use Azure Storage SDK to upload the HTML file to Azure Storage
            //var queueServiceClient = new QueueServiceClient(QueueConnectionString);
            //var queueClient = queueServiceClient.GetQueueClient(QueueName);

            //await queueClient.SendMessageAsync(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));

            // Upload HTML file to Azure Storage
            
        }

        private async Task EnqueueMessage(string messageContent, string pdfBlobName)
        {
            // Use Azure Storage SDK to enqueue a message for background processing
            var queueClient = new QueueClient(QueueConnectionString, QueueName);
            await queueClient.SendMessageAsync(messageContent);
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
    }
}
