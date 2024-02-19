namespace API_HtmlToPdfConverter.Services
{
    using API_HtmlToPdfConverter.Interfaces;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.Configuration;

    public class AzureStorageAccountService : IAzureStorageAccountService
    {
        private const string HtmlFilesContainerName = "Htmlfilescontainer";
        private const string PdfFilesContainerName = "Pdffilescontainer";
        private const string AzureContainerNames = "AzureContainerNames";
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;

        public AzureStorageAccountService(IConfiguration configuration,
            BlobServiceClient blobServiceClient)
        {
            _configuration = configuration;
            _blobServiceClient = blobServiceClient;
        }

        public async Task<Stream> DownloadFileFromStorage(string blobName)
        {
            try
            {
                var containerName = _configuration.GetValue<string>(HtmlFilesContainerName);
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.OpenReadAsync();
                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UploadHtmlFileToStorage(Stream fileStream, string blobName)
        {
            try
            {
                var containerName = _configuration.GetSection(AzureContainerNames).GetValue<string>(HtmlFilesContainerName);
                await UploadFileToStorage(fileStream, blobName, containerName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Uri> GetPdfDownloadUrl(Stream fileStream, string blobName)
        {
            try
            {
                var containerName = _configuration.GetSection(AzureContainerNames).GetValue<string>(PdfFilesContainerName);
                var containerClient = await UploadFileToStorage(fileStream, blobName, containerName).ConfigureAwait(false);

                var downloadUrl = containerClient.GenerateSasUri(Azure.Storage.Sas.BlobContainerSasPermissions.Read,
                    DateTimeOffset.UtcNow.AddMinutes(5));

                return downloadUrl;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<BlobContainerClient> UploadFileToStorage(Stream fileStream,
            string blobName, string containerName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            fileStream.Position = 0;
            await containerClient.UploadBlobAsync(blobName, fileStream).ConfigureAwait(false);

            return containerClient;
        }
    }
}
