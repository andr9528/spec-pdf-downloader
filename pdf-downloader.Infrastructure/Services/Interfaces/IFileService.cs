using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pdf_downloader.Infrastructure.Services.Interfaces
{
    public interface IFileService
    {
        Task<List<PdfDownload>> ParseExcelAsync(Stream excelStream);
        Task<string> EnsureDirectoryAsync(string folderName);
        Task<byte[]> GetFileAsync(string relativePath);
        Task<List<PdfDownload>> DownloadAndSavePdfsAsync(List<PdfDownload> pdfs, string folderName);
    }
}