namespace API_HtmlToPdfConverter.Controllers
{
    using API_HtmlToPdfConverter.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class HtmlController : Controller
    {
        private readonly IAzureStorageAccountService _storageService;
        public HtmlController(IAzureStorageAccountService storageService)
        {
            _storageService = storageService;
        }
        
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
                var temp = htmlFile.OpenReadStream().ReadByte();
                using (var memoryStream = new MemoryStream())
                {
                    htmlFile.OpenReadStream().CopyTo(memoryStream);
                    await _storageService.UploadHtmlFileToStorage(memoryStream, htmlBlobName);
                }
                
                
                return Ok(new { htmlBlobName });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
