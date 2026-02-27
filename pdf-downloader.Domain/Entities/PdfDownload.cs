public class PdfDownload
{
    public Guid Id { get; set; }
    public string BRnum { get; set; } = default!;
    public string Url { get; set; } = default!;
    public string BackupUrl { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? LocalPath { get; set; }
    public bool IsDownloaded { get; set; } = false;
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}