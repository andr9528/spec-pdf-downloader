namespace PdfDownloader.Infrastructure.Services.Interfaces;

public interface IDownloader
{
    Task<List<PdfDownload>> DoDownloads(string directoryDestination, List<PdfDownload> notDownloadedPdfs);
}