using pdf_downloader.Domain.Exceptions;
using PdfDownloader.Infrastructure.Persistence;
using PdfDownloader.Infrastructure.Services.Interfaces;

namespace PdfDownloader.Infrastructure;
public class PdfsDownloader : IDownloader
{
    private readonly HttpClient _client;

    public PdfsDownloader() : this(new HttpClient()) { }

    public PdfsDownloader(HttpClient client)
    {
        _client = client;
    }

    public async Task<List<PdfDownload>> DoDownloads(string directoryDestination, List<PdfDownload> notDownloadedPdfs)
    {
        var tasks = notDownloadedPdfs.Select(async notDownloaded =>
        {
            string ErrorMessage = "";
            bool isSuccess = false;
            bool secondIsSuccess = false;
            var targetPathpdf = Path.Combine(directoryDestination, notDownloaded.BRnum) + ".pdf";

            try
            {
                isSuccess = await DownloadAsync(notDownloaded.Url, targetPathpdf);          
            }
            catch (DownloadFailedException exception)
            {
                ErrorMessage += $"First Attempt: {exception.Message}";
            }
            catch (Exception exception)
            {
                ErrorMessage += $"First Attempt: {exception.Message}";
            }

            // Could skip this if, if the backup url is empty.
            // i.e `... && !string.IsNullOrEmpty(notDownloaded.BackupUrl)`.
            // Code does the same at the moment, but would improve readability,
            // and eliminate a thrown `DownloadFailedException`.
            if (!isSuccess)
            {
                try
                {
                    secondIsSuccess = await DownloadAsync(notDownloaded.BackupUrl, targetPathpdf);
                }
                catch (DownloadFailedException exception)
                {
                    ErrorMessage += $"{Environment.NewLine}Second Attempt: {exception.Message}";
                }
                catch (Exception exception)
                {
                    ErrorMessage += $"{Environment.NewLine}Second Attempt: {exception.Message}";
                }
            }

            if (isSuccess || secondIsSuccess)
            {
                notDownloaded.LocalPath = targetPathpdf;
                notDownloaded.IsDownloaded = true;
            }
            else
            {
                notDownloaded.ErrorMessage = ErrorMessage;
            }
        });

        await Task.WhenAll(tasks);

        return notDownloadedPdfs;
    }

    public async Task<bool> DownloadAsync(string url, string targetPath)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
                throw new DownloadFailedException($"Url is Empty. Please provide a Url.");

            if (!IsValidUrl(url))
                throw new DownloadFailedException($"Url is not a valid url");

            var response = await _client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new DownloadFailedException($"Address was not found.");

            if (!response.IsSuccessStatusCode)
                throw new DownloadFailedException($"Request failed with statuscode: {response.StatusCode}");

            if (response.Content.Headers.ContentType?.MediaType != "application/pdf")
                throw new DownloadFailedException($"Not a PDF");

            var bytes = await response.Content.ReadAsByteArrayAsync();

            // Can end up throwing an `IndexOutOfRangeException` if response content is less than 2 bytes.
            if (bytes[0] != 0x25 || bytes[1] != 0x50) // %P
                throw new DownloadFailedException($"Invalid PDF header");

            await File.WriteAllBytesAsync(targetPath, bytes);

            return true;
        }
        catch (Exception exception)
        {
            throw;
        }
    }

    static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        try
        {
            var uri = new Uri(url, UriKind.Absolute);
            return (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
        catch
        {
            return false;
        }
    }
}