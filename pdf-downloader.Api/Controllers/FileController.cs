using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pdf_downloader.Infrastructure.Services.Interfaces;
using PdfDownloader.Domain.Enums;
using PdfDownloader.Infrastructure.Services.Interfaces;

[ApiController]
[Route("api/file")]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly IDownloadRepository _downloadRepository;
    private readonly ILogger<FileController> _logger;

    public FileController(
        IFileService fileService,
        IDownloadRepository downloadRepository,
        ILogger<FileController> logger)
    {
        _fileService = fileService;
        _downloadRepository = downloadRepository;
        _logger = logger;
    }

    [Authorize]
    [HttpGet("downloads")]
    public IActionResult GetAllDownloads()
    {
        var downloads = _downloadRepository.GetDownloads();
        return Ok(downloads);
    }

    [Authorize]
    [HttpGet("downloads/{id:guid}")]
    public async Task<IActionResult> GetDownload(Guid id)
    {
        var download = _downloadRepository.GetDownload(id);
        if (download == null)
            return NotFound("Requested PDF not found.");

        try
        {
            var fileBytes = await _fileService.GetFileAsync(download.LocalPath);
            return File(fileBytes, "application/pdf", download.Title);
        }
        catch (FileNotFoundException)
        {
            return NotFound("File missing on server.");
        }
    }

    [Authorize(Roles = nameof(Role.ADMIN))]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0 || !file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Invalid or empty file.");

        List<PdfDownload> pdfs;
        await using (var stream = file.OpenReadStream())
        {
            pdfs = await _fileService.ParseExcelAsync(stream);
        }

        var tasks = pdfs.Select(d => _downloadRepository.CreateOrUpdate(d));
        await Task.WhenAll(tasks);

        _logger.LogInformation("Uploaded {Count} PDFs", pdfs.Count);
        return Ok(pdfs);
    }

    [Authorize(Roles = nameof(Role.ADMIN))]
    [HttpPost("downloads/all")]
    public async Task<IActionResult> DownloadAll()
    {
        var notDownloaded = _downloadRepository.GetNotDownloaded();
        var downloaded = await _fileService.DownloadAndSavePdfsAsync(notDownloaded, "pdf");
        return Ok(downloaded);
    }

    [Authorize(Roles = nameof(Role.ADMIN))]
    [HttpPost("downloads")]
    public async Task<IActionResult> DownloadSelected([FromBody] List<Guid> ids)
    {
        var toDownload = _downloadRepository.GetDownloads(ids);
        if (toDownload == null || !toDownload.Any())
            return NotFound("No valid PDFs selected for download.");

        var downloaded = await _fileService.DownloadAndSavePdfsAsync(toDownload, "pdf");
        return Ok(downloaded);
    }
}