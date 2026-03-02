using PdfDownloader.Domain.Entities;

public interface IDownloadRepository
{
    Task<PdfDownload> CreateOrUpdate(PdfDownload download);
    List<PdfDownload> GetDownloads();
    List<PdfDownload> GetDownloads(List<Guid> ids);
    PdfDownload? GetDownload(Guid id);

    /// <summary>
    /// Remove me - im unused
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    byte[] GetFile(string path);

    List<PdfDownload> GetNotDownloaded();
}