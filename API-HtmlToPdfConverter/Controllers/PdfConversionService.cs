namespace API_HtmlToPdfConverter.Controllers
{
    using Azure.Storage.Blobs;
    using Azure.Storage.Queues;
    using Microsoft.AspNetCore.SignalR;
    using PuppeteerSharp;

    public class PdfConversionService : BackgroundService
    {
        private const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=maximmstorageaccount;AccountKey=ZL4D6WagjQhhYaC6t00hMGbF0YnjM4uAevlIjPEdYL08n/Yl2O3ZSvI0y4ucwhWHSXjR2DbKb2al+AStILNsTg==;EndpointSuffix=core.windows.net";
        private const string QueueConnectionString = "DefaultEndpointsProtocol=https;AccountName=maximmstorageaccount;AccountKey=ZL4D6WagjQhhYaC6t00hMGbF0YnjM4uAevlIjPEdYL08n/Yl2O3ZSvI0y4ucwhWHSXjR2DbKb2al+AStILNsTg==;EndpointSuffix=core.windows.net";
        private const string HtmlFilesContainerName = "htmlfilescontainer";
        private const string PdfFilesContainerName = "pdffilescontainer";
        private const string QueueName = "htmlfilesqueue";
        private readonly string connectionString = "your_storage_account_connection_string";
        private readonly string queueName = "pdfconversionqueue";
        private readonly IHubContext<PdfHub> _pdfHubContext;
        private readonly QueueServiceClient _queueServiceClient;
        private readonly BlobServiceClient _blobServiceClient;

        //public PdfConversionService(QueueServiceClient queueServiceClient,
        //    BlobServiceClient blobServiceClient)
        //{
        //    _blobServiceClient = blobServiceClient;
        //    _queueServiceClient = queueServiceClient;
        //}

        public PdfConversionService(IHubContext<PdfHub> pdfHubContext)
        {
            _pdfHubContext = pdfHubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Dequeue a message for processing
                    var queueServiceClient = new QueueServiceClient(QueueConnectionString);
                    var queueClient = queueServiceClient.GetQueueClient(QueueName);
                    var messages = await queueClient.ReceiveMessagesAsync(1, TimeSpan.FromMinutes(1));

                    if (messages?.Value?.Length > 0)
                    {
                        var blobName = messages.Value[0].MessageText;

                        await _pdfHubContext.Clients.All.SendAsync("ConversionInProgress");

                        // Download HTML file from Azure Storage
                        var htmlStream = await DownloadFileFromStorage(blobName);

                        // Perform HTML to PDF conversion using Puppeteer Sharp
                        var pdfBytes = await ConvertHtmlToPdfAsync(htmlStream);

                        // Upload PDF file to Azure Storage
                        var pdfBlobName = $"{Guid.NewGuid()}_output.pdf";
                        await UploadFileToStorage(pdfBytes, pdfBlobName);

                        // Delete the message from the queue
                        await queueClient.DeleteMessageAsync(messages.Value[0].MessageId, messages.Value[0].PopReceipt);

                        await _pdfHubContext.Clients.All.SendAsync("ConversionComplete");

                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions and log
                }

                // Add a delay to avoid constant polling
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task<Stream> DownloadFileFromStorage(string blobName)
        {
            // Use Azure Storage SDK to download the HTML file from Azure Storage
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

        private async Task UploadFileToStorage(byte[] fileBytes, string blobName)
        {
            // Use Azure Storage SDK to upload the PDF file to Azure Storage
            var blobServiceClient = new BlobServiceClient(StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(PdfFilesContainerName);

            using (var stream = new MemoryStream(fileBytes))
            {
                await containerClient.UploadBlobAsync(blobName, stream);
            }
        }
    }
}
