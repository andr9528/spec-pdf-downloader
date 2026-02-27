using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using PdfDownloader.Infrastructure;
using PdfDownloader.Infrastructure.Services.Interfaces;
using Xunit;

namespace PdfDownloader.Test
{
    public class PdfsDownloaderTests
    {
        private PdfsDownloader CreateDownloaderWithMockedHttp(byte[] responseBytes, HttpStatusCode statusCode = HttpStatusCode.OK, string mediaType = "application/pdf")
        {
            // Mock HttpMessageHandler
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = statusCode,
                   Content = new ByteArrayContent(responseBytes)
                   {
                       Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType) }
                   }
               });

            var client = new HttpClient(handlerMock.Object);

            return new PdfsDownloader(client);
        }

        [Fact]
        public async Task DownloadAsync_ReturnsFalse_WhenUrlIsEmpty()
        {
            var downloader = new PdfsDownloader();

            var result = await downloader.DownloadAsync("", "file.pdf");

            Assert.False(result.success);
            Assert.Contains("Url is Empty", result.error);
        }

        [Fact]
        public async Task DownloadAsync_ReturnsFalse_WhenUrlIsInvalid()
        {
            var downloader = new PdfsDownloader();

            var result = await downloader.DownloadAsync("invalid-url", "file.pdf");

            Assert.False(result.success);
            Assert.Equal("Url is not a valid url", result.error);
        }

        [Fact]
        public async Task DownloadAsync_ReturnsFalse_WhenPdfHeaderInvalid()
        {
            var invalidPdfBytes = new byte[] { 0x00, 0x00 }; // Not %PDF
            var downloader = CreateDownloaderWithMockedHttp(invalidPdfBytes);

            var result = await downloader.DownloadAsync("http://fake.com/file.pdf", Path.GetTempFileName());

            Assert.False(result.success);
            Assert.Equal("Invalid PDF header", result.error);
        }

        [Fact]
        public async Task DownloadAsync_ReturnsTrue_WhenPdfValid()
        {
            var validPdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");
            var downloader = CreateDownloaderWithMockedHttp(validPdfBytes);

            var result = await downloader.DownloadAsync("http://fake.com/file.pdf", tempFile);

            Assert.True(result.success);
            Assert.True(File.Exists(tempFile));

            File.Delete(tempFile);
        }

        [Fact]
        public async Task DoDownloads_MarksPdfAsDownloaded_WhenSuccessful()
        {
            var validPdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            var downloader = CreateDownloaderWithMockedHttp(validPdfBytes);

            var pdfs = new List<PdfDownload>
            {
                new PdfDownload { Url = "http://fake.com/file.pdf", BRnum = "123" }
            };

            var result = await downloader.DoDownloads(tempDir, pdfs);

            Assert.Single(result);
            Assert.True(result[0].IsDownloaded);
            Assert.Equal(Path.Combine(tempDir, "123.pdf"), result[0].LocalPath);
            Assert.Null(result[0].ErrorMessage);

            // Cleanup
            File.Delete(result[0].LocalPath);
            Directory.Delete(tempDir);
        }

        [Fact]
        public async Task DoDownloads_SetsErrorMessage_WhenDownloadFails()
        {
            // Mock HttpClient to always return 404
            var downloader = CreateDownloaderWithMockedHttp(new byte[0], HttpStatusCode.NotFound);

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            var pdfs = new List<PdfDownload>
            {
                new PdfDownload { Url = "http://fake.com/file.pdf", BackupUrl = "http://backup.com/file.pdf", BRnum = "123" }
            };

            var result = await downloader.DoDownloads(tempDir, pdfs);

            Assert.Single(result);
            Assert.False(result[0].IsDownloaded);
            Assert.NotNull(result[0].ErrorMessage);
            Assert.Contains("First Attempts", result[0].ErrorMessage);
            Assert.Contains("Second Attempt", result[0].ErrorMessage);

            Directory.Delete(tempDir);
        }
    }
}