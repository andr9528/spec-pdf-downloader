using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using pdf_downloader.Infrastructure.Services.Interfaces;
using PdfDownloader.Infrastructure.Services.Interfaces;

namespace pdf_downloader.Infrastructure.Services
{
    using PdfDownloader.Infrastructure.Services.Interfaces;

    public class FileService : IFileService
    {
        private readonly IReader _reader;
        private readonly IDownloader _downloader;
        private readonly IDownloadRepository _downloadRepository;
        private readonly ILogger<FileService> _logger;

        public FileService(
            IReader reader,
            IDownloader downloader,
            IDownloadRepository downloadRepository,
            ILogger<FileService> logger)
        {
            _reader = reader;
            _downloader = downloader;
            _downloadRepository = downloadRepository;
            _logger = logger;
        }

        public async Task<List<PdfDownload>> ParseExcelAsync(Stream excelStream)
        {
            if (excelStream == null) throw new ArgumentNullException(nameof(excelStream));

            var pdfs = await _reader.Read(excelStream);

            if (pdfs == null || !pdfs.Any())
                throw new InvalidOperationException("No PDF data found in Excel file.");

            return pdfs;
        }

        public async Task<string> EnsureDirectoryAsync(string folderName)
        {
            string baseDir = Directory.GetCurrentDirectory();
            string fullPath = Path.Combine(baseDir, "storage", folderName);
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        public async Task<byte[]> GetFileAsync(string relativePath)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            if (!File.Exists(path))
            {
                _logger.LogWarning("Requested file not found: {Path}", path);
                throw new FileNotFoundException($"File not found: {path}");
            }

            return await File.ReadAllBytesAsync(path);
        }

        public async Task<List<PdfDownload>> DownloadAndSavePdfsAsync(List<PdfDownload> pdfs, string folderName)
        {
            if (pdfs == null || !pdfs.Any()) return new List<PdfDownload>();

            string dir = await EnsureDirectoryAsync(folderName);
            List<PdfDownload> downloaded = await _downloader.DoDownloads(dir, pdfs);

            var updateTasks = downloaded.Select(d => _downloadRepository.CreateOrUpdate(d));
            await Task.WhenAll(updateTasks);

            _logger.LogInformation("Downloaded {Count} PDFs to {Dir}", downloaded.Count, dir);
            return downloaded;
        }
    }
}