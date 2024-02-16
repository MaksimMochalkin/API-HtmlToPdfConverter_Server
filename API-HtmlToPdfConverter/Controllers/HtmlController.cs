namespace API_HtmlToPdfConverter.Controllers
{
    using Azure.Storage.Blobs;
    using Azure.Storage.Queues;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
    using PuppeteerSharp;

    [ApiController]
    [Route("api/[controller]")]
    public class HtmlController : Controller
    {
        private const string StorageConnectionString = "put_connection_string";
        private const string QueueConnectionString = "put_connection_string";
        private const string ContainerName = "htmlfilescontainer";
        private const string PdfFilesContainerName = "pdffilescontainer";
        private const string QueueName = "htmlfilesqueue";

        private readonly QueueServiceClient _queueServiceClient;
        private readonly BlobServiceClient _blobServiceClient;

        //public PdfController(IHubContext<PdfHub> pdfHubContext)
        //{
        //    _pdfHubContext = pdfHubContext;
        //}
        //public PdfController(QueueServiceClient queueServiceClient,
        //    BlobServiceClient blobServiceClient)
        //{
        //    _blobServiceClient = blobServiceClient;
        //    _queueServiceClient = queueServiceClient;
        //}

        [HttpPost("putHtmlTo")]
        public async Task<IActionResult> PutHtmlToAzureServices(IFormFile htmlFile)
        {
            try
            {
                if (htmlFile == null || htmlFile.Length == 0)
                {
                    return BadRequest("Invalid HTML file");
                }

                var htmlBlobName = $"{Guid.NewGuid()}_input.html";
                //await UploadFileToStorage(htmlFile.OpenReadStream(), htmlBlobName);
                
                return Ok(new { htmlBlobName });
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
            var fileUrl = blobServiceClient.Uri.AbsoluteUri;
            var temp = containerClient.Uri;
            //await htmlBlobClient.UploadAsync(fileStream);
            // Use Azure Storage SDK to upload the HTML file to Azure Storage
            //var queueServiceClient = new QueueServiceClient(QueueConnectionString);
            //var queueClient = queueServiceClient.GetQueueClient(QueueName);

            //await queueClient.SendMessageAsync(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));

            // Upload HTML file to Azure Storage
            
        }
    }
}
