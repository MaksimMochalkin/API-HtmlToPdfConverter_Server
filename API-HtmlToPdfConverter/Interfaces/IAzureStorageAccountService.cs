namespace API_HtmlToPdfConverter.Interfaces
{
    public interface IAzureStorageAccountService
    {
        Task UploadHtmlFileToStorage(Stream fileStream, string blobName);
        Task<Stream> DownloadFileFromStorage(string blobName);
        Task<Uri> GetPdfDownloadUrl(Stream fileStream, string blobName);
    }
}
