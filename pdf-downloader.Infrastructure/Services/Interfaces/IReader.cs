namespace PdfDownloader.Infrastructure.Services.Interfaces;

public interface IReader
{
    public Task<List<PdfDownload>?> Read(Stream fileStream);
}