using PdfDownloader.Domain.Entities;

public interface IDownloadRepository
{
    Task<PdfDownload> CreateOrUpdate(PdfDownload download);
    List<PdfDownload> GetDownloads();
    List<PdfDownload> GetDownloads(List<Guid> ids);
    PdfDownload? GetDownload(Guid id);
    byte[] GetFile(string path);

    List<PdfDownload> GetNotDownloaded();
}