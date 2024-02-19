namespace API_HtmlToPdfConverter.Controllers
{
    using API_HtmlToPdfConverter.Interfaces;
    using Azure.Storage.Blobs;
    using Microsoft.AspNetCore.Mvc;
    using PuppeteerSharp;

    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : Controller
    {
        private readonly IAzureStorageAccountService _storageService;
        private readonly IConvertService _convertService;

        public PdfController(IAzureStorageAccountService storageService,
            IConvertService convertService)
        {
            _storageService = storageService;
            _convertService = convertService;
        }

        [HttpGet("getPdfLink")]
        public async Task<IActionResult> GetPdfLinkFromAzureService(string htmlBlobName)
        {
            if (string.IsNullOrWhiteSpace(htmlBlobName))
            {
                return BadRequest("Missed blob name");
            }

            var htmlStream = await _storageService.DownloadFileFromStorage(htmlBlobName).ConfigureAwait(false);

            var pdfBytes = await _convertService.ConvertHtmlToPdfAsync(htmlStream).ConfigureAwait(false);

            var pdfBlobName = $"{Guid.NewGuid()}_output.pdf";
            Uri? pdfDownloadLink = default;
            using (var memoryStream = new MemoryStream(pdfBytes))
            {
                pdfDownloadLink = await _storageService.GetPdfDownloadUrl(memoryStream, pdfBlobName);
            }
            return Ok(new { pdfDownloadLink });
        }
    }
}
