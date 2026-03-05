using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using pdf_downloader.Infrastructure.Services;
using PdfDownloader.Application.DTOs;
using PdfDownloader.Application.Services;
using PdfDownloader.Infrastructure;
using PdfDownloader.Infrastructure.Persistence;
using PdfDownloader.Infrastructure.Services;

namespace pdf_downloader.Test
{
    public class IntegrationTests
    {
        private const string USER_EMAIL = "SomeUser";
        private const string USER_PASSWORD = "SomePassword";

        [Fact]
        public async Task Program_Should_Successfully_Run()
        {
            var loggerFactory = LoggerFactory.Create(ConfigureLogging);

            var (connection, db) = await CreateDatabaseAsync();
            AuthController authController = CreateAuthController(db);

            await RegisterUserAsync(authController);
            await LoginUserAsync(authController);

            FileController fileController = CreateFileController(db, loggerFactory);

            await UploadExcelFile(fileController);
            await DownloadAll(fileController);

            await db.DisposeAsync();
            await connection.DisposeAsync();
        }

        private FileController CreateFileController(AppDbContext db, ILoggerFactory loggerFactory)
        {
            var reader = new ExcelReader();
            var downloader = new PdfsDownloader();
            var downloadRepository = new DownloadRepository(db);
            var fileServiceLogger = new Logger<FileService>(loggerFactory);
            var fileService = new FileService(reader, downloader, downloadRepository, fileServiceLogger);
            var fileControllerLogger = new Logger<FileController>(loggerFactory);
            var fileController = new FileController(fileService, downloadRepository, fileControllerLogger);
            return fileController;
        }

        private async Task DownloadAll(FileController fileController)
        {
            var result = await fileController.DownloadAll();

            IsOk_With_Five_PdfDownload(result);
        }

        private async Task UploadExcelFile(FileController fileController)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "TestData", "GRI_2017_2020_test5.xlsx");
            var stream = File.OpenRead(filePath);
            var formFile = new FormFile(baseStream: stream, baseStreamOffset: 0, length: stream.Length, name: "file",
                fileName: Path.GetFileName(filePath));

            var result = await fileController.Upload(formFile);

            IsOk_With_Five_PdfDownload(result);
        }

        private void IsOk_With_Five_PdfDownload(IActionResult result)
        {
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var pdfs = Assert.IsType<List<PdfDownload>>(ok.Value);
            Assert.True(pdfs.Count == 5);
        }

        private void ConfigureLogging(ILoggingBuilder configure)
        {
        }

        private AuthController CreateAuthController(AppDbContext db)
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsAValidTestJwtKey1234567890123456",
            }).Build();
            var userRepository = new UserRepository(db);
            var tokenGenerator = new JwtTokenGenerator(configuration);
            var authService = new AuthService(userRepository, tokenGenerator);
            var authController = new AuthController(authService);
            return authController;
        }

        private async Task<(SqliteConnection connection, AppDbContext db)> CreateDatabaseAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection)
                .EnableSensitiveDataLogging().Options;
            var db = new AppDbContext(options);
            await db.Database.EnsureCreatedAsync();
            return (connection, db);
        }

        private async Task LoginUserAsync(AuthController authController)
        {
            var loginRequest = new LoginRequest() {Email = USER_EMAIL, Password = USER_PASSWORD};

            var result = await authController.Login(loginRequest);

            var ok = Assert.IsType<OkObjectResult>(result);
            var token = ok.Value;
            Assert.NotNull(token);
        }

        private async Task RegisterUserAsync(AuthController authController)
        {
            var registerRequest = new RegisterRequest(USER_EMAIL, USER_PASSWORD);

            var result = await authController.Register(registerRequest);

            var ok = Assert.IsType<OkObjectResult>(result);
            var token = ok.Value;
            Assert.NotNull(token);
        }
    }
}