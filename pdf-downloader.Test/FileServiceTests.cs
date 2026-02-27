using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using pdf_downloader.Infrastructure.Services;
using pdf_downloader.Infrastructure.Services.Interfaces;
using PdfDownloader.Infrastructure.Services.Interfaces;

namespace pdf_downloader.Test
{
    public class FileServiceTests
    {
        private readonly Mock<IReader> _readerMock;
        private readonly Mock<IDownloader> _downloaderMock;
        private readonly Mock<IDownloadRepository> _downloadRepoMock;
        private readonly Mock<ILogger<FileService>> _loggerMock;
        private readonly FileService _fileService;

        public FileServiceTests()
        {
            _readerMock = new Mock<IReader>();
            _downloaderMock = new Mock<IDownloader>();
            _downloadRepoMock = new Mock<IDownloadRepository>();
            _loggerMock = new Mock<ILogger<FileService>>();

            _fileService = new FileService(
                _readerMock.Object,
                _downloaderMock.Object,
                _downloadRepoMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task ParseExcelAsync_ReturnsPdfs_WhenExcelHasData()
        {
            // Arrange
            var pdfs = new List<PdfDownload> { new PdfDownload { Url = "http://test.com/file.pdf" } };
            _readerMock.Setup(r => r.Read(It.IsAny<Stream>())).ReturnsAsync(pdfs);

            using var ms = new MemoryStream();

            // Act
            var result = await _fileService.ParseExcelAsync(ms);

            // Assert
            Assert.Equal(pdfs.Count, result.Count);
        }

        [Fact]
        public async Task ParseExcelAsync_Throws_WhenExcelIsEmpty()
        {
            // Arrange
            _readerMock.Setup(r => r.Read(It.IsAny<Stream>())).ReturnsAsync(new List<PdfDownload>());

            using var ms = new MemoryStream();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _fileService.ParseExcelAsync(ms));
        }

        [Fact]
        public async Task EnsureDirectoryAsync_CreatesDirectory()
        {
            // Arrange
            string folderName = "testfolder";

            // Act
            string path = await _fileService.EnsureDirectoryAsync(folderName);

            // Assert
            Assert.True(Directory.Exists(path));

            // Cleanup
            Directory.Delete(path, true);
        }

        [Fact]
        public async Task GetFileAsync_ReturnsBytes_WhenFileExists()
        {
            // Arrange
            string testFile = Path.Combine(Directory.GetCurrentDirectory(), "testfile.txt");
            await File.WriteAllTextAsync(testFile, "hello world");

            // Act
            var bytes = await _fileService.GetFileAsync("testfile.txt");

            // Assert
            Assert.NotEmpty(bytes);

            // Cleanup
            File.Delete(testFile);
        }

        [Fact]
        public async Task GetFileAsync_Throws_WhenFileDoesNotExist()
        {
            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => _fileService.GetFileAsync("nonexistent.txt"));
        }

        [Fact]
        public async Task DownloadAndSavePdfsAsync_DownloadsFilesAndUpdatesRepository()
        {
            // Arrange
            var pdfs = new List<PdfDownload> { new PdfDownload { Url = "http://test.com/file.pdf" } };
            _downloaderMock
                .Setup(d => d.DoDownloads(It.IsAny<string>(), pdfs))
                .ReturnsAsync(pdfs);
            _downloadRepoMock
                .Setup(r => r.CreateOrUpdate(It.IsAny<PdfDownload>()))
                .ReturnsAsync((PdfDownload d) => d);

            string folderName = "downloads";

            // Act
            var result = await _fileService.DownloadAndSavePdfsAsync(pdfs, folderName);

            // Assert
            Assert.Equal(pdfs.Count, result.Count);
            _downloadRepoMock.Verify(r => r.CreateOrUpdate(It.IsAny<PdfDownload>()), Times.Exactly(pdfs.Count));

            // Cleanup
            string path = Path.Combine(Directory.GetCurrentDirectory(), "storage", folderName);
            Directory.Delete(path, true);
        }

        [Fact]
        public async Task DownloadAndSavePdfsAsync_ReturnsEmpty_WhenNoPdfs()
        {
            // Act
            var result = await _fileService.DownloadAndSavePdfsAsync(new List<PdfDownload>(), "anyfolder");

            // Assert
            Assert.Empty(result);
        }
    }
}