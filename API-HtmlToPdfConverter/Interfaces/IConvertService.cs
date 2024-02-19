namespace API_HtmlToPdfConverter.Interfaces
{
    public interface IConvertService
    {
        Task<byte[]> ConvertHtmlToPdfAsync(Stream htmlStream);
    }
}
