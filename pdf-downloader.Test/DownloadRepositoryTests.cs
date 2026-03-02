using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PdfDownloader.Infrastructure.Persistence;

namespace pdf_downloader.Test
{
    public class DownloadRepositoryTests : IAsyncLifetime
    {
        private SqliteConnection _connection = default!;
        private AppDbContext _db = default!;
        private DownloadRepository _sut = default!;

        public async Task InitializeAsync()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            await _connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .EnableSensitiveDataLogging()
                .Options;

            _db = new AppDbContext(options);
            await _db.Database.EnsureCreatedAsync();

            _sut = new DownloadRepository(_db);
        }

        public async Task DisposeAsync()
        {
            await _db.DisposeAsync();
            await _connection.DisposeAsync();
        }

        [Fact]
        public async Task CreateOrUpdate_WhenNew_InsertsAndReturnsDownload()
        {
            // Arrange
            var download = new PdfDownload
            {
                Id = Guid.NewGuid(),
                BRnum = "BR-1",
                Url = "https://example.com/1",
                BackupUrl = "https://backup.com/1",
                Title = "Title 1",
                IsDownloaded = false,
                RetryCount = 0,
                ErrorMessage = null
            };

            // Act
            var result = await _sut.CreateOrUpdate(download);

            // Assert
            Assert.NotNull(result);

            var fromDb = await _db.PdfDownloads.SingleAsync(x => x.BRnum == "BR-1");
            Assert.Equal(download.Id, fromDb.Id);
            Assert.Equal("Title 1", fromDb.Title);
        }

        [Fact]
        public async Task CreateOrUpdate_WhenExisting_UpdatesExistingAndKeepsId()
        {
            // Arrange
            var existing = new PdfDownload
            {
                Id = Guid.NewGuid(),
                BRnum = "BR-1",
                Url = "https://example.com/1",
                BackupUrl = "https://backup.com/1",
                Title = "Old",
                IsDownloaded = false,
                RetryCount = 0,
                ErrorMessage = null
            };

            _db.PdfDownloads.Add(existing);
            await _db.SaveChangesAsync();

            var update = new PdfDownload
            {
                Id = Guid.NewGuid(), // will be overwritten to existing.Id by repo
                BRnum = "BR-1",
                Url = "https://example.com/1-new",
                BackupUrl = "https://backup.com/1-new",
                Title = "New",
                IsDownloaded = true,
                RetryCount = 2,
                ErrorMessage = null,
                LocalPath = @"c:\tmp\file.pdf"
            };

            // Act
            var returned = await _sut.CreateOrUpdate(update);

            // Assert
            var fromDb = await _db.PdfDownloads.SingleAsync(x => x.BRnum == "BR-1");
            Assert.Equal(existing.Id, fromDb.Id);
            Assert.Equal("New", fromDb.Title);
            Assert.True(fromDb.IsDownloaded);
            Assert.Equal(2, fromDb.RetryCount);

            // Repo returns the tracked existing instance
            Assert.Equal(existing.Id, returned.Id);
        }

        [Fact]
        public async Task GetDownloads_ReturnsAll()
        {
            // Arrange
            _db.PdfDownloads.AddRange(
                new PdfDownload { Id = Guid.NewGuid(), BRnum = "BR-1", Url = "u1", BackupUrl = "b1", Title = "t1" },
                new PdfDownload { Id = Guid.NewGuid(), BRnum = "BR-2", Url = "u2", BackupUrl = "b2", Title = "t2" }
            );
            await _db.SaveChangesAsync();

            // Act
            var all = _sut.GetDownloads();

            // Assert
            Assert.Equal(2, all.Count);
        }

        [Fact]
        public async Task GetDownload_WhenExists_ReturnsOne()
        {
            // Arrange
            var id = Guid.NewGuid();
            _db.PdfDownloads.Add(new PdfDownload { Id = id, BRnum = "BR-1", Url = "u1", BackupUrl = "b1", Title = "t1" });
            await _db.SaveChangesAsync();

            // Act
            var result = _sut.GetDownload(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result!.Id);
        }

        [Fact]
        public void GetDownload_WhenMissing_ReturnsNull()
        {
            // Act
            var result = _sut.GetDownload(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDownloads_ByIds_ReturnsOnlyMatching()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();

            _db.PdfDownloads.AddRange(
                new PdfDownload { Id = id1, BRnum = "BR-1", Url = "u1", BackupUrl = "b1", Title = "t1" },
                new PdfDownload { Id = id2, BRnum = "BR-2", Url = "u2", BackupUrl = "b2", Title = "t2" },
                new PdfDownload { Id = id3, BRnum = "BR-3", Url = "u3", BackupUrl = "b3", Title = "t3" }
            );
            await _db.SaveChangesAsync();

            // Act
            var result = _sut.GetDownloads(new List<Guid> { id1, id3 });

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Id == id1);
            Assert.Contains(result, x => x.Id == id3);
            Assert.DoesNotContain(result, x => x.Id == id2);
        }

        [Fact]
        public async Task GetNotDownloaded_ReturnsOnlyNotDownloadedWithoutError()
        {
            // Arrange
            _db.PdfDownloads.AddRange(
                new PdfDownload { Id = Guid.NewGuid(), BRnum = "BR-1", Url = "u1", BackupUrl = "b1", Title = "t1", IsDownloaded = false, ErrorMessage = null },
                new PdfDownload { Id = Guid.NewGuid(), BRnum = "BR-2", Url = "u2", BackupUrl = "b2", Title = "t2", IsDownloaded = false, ErrorMessage = "boom" },
                new PdfDownload { Id = Guid.NewGuid(), BRnum = "BR-3", Url = "u3", BackupUrl = "b3", Title = "t3", IsDownloaded = true, ErrorMessage = null }
            );
            await _db.SaveChangesAsync();

            // Act
            var result = _sut.GetNotDownloaded();

            // Assert
            Assert.Single(result);
            Assert.Equal("BR-1", result.Single().BRnum);
        }
    }
}