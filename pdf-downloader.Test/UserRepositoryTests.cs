using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PdfDownloader.Domain.Entities;
using PdfDownloader.Domain.Enums;
using PdfDownloader.Infrastructure.Persistence;

namespace pdf_downloader.Test
{
    public class UserRepositoryTests : IAsyncLifetime
    {
        private SqliteConnection _connection = default!;
        private AppDbContext _db = default!;
        private UserRepository _sut = default!;

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

            _sut = new UserRepository(_db);
        }

        public async Task DisposeAsync()
        {
            await _db.DisposeAsync();
            await _connection.DisposeAsync();
        }

        [Fact]
        public async Task GetByEmailAsync_WhenUserExists_ReturnsUser_AsNoTracking()
        {
            // Arrange
            var user = new User
            {
                Email = "a@b.com",
                PasswordHash = "hash",
                Role = Role.USER.ToString()
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Act
            var result = await _sut.GetByEmailAsync("a@b.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("a@b.com", result!.Email);

            Assert.Equal(EntityState.Detached, _db.Entry(result).State);
        }

        [Fact]
        public async Task GetByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _sut.GetByEmailAsync("missing@b.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_FirstUser_BecomesAdmin()
        {
            // Arrange
            var user = new User
            {
                Email = "first@site.com",
                PasswordHash = "hash",
                Role = "IGNORED"
            };

            // Act
            await _sut.AddAsync(user);

            // Assert
            var fromDb = await _db.Users.SingleAsync();
            Assert.Equal(Role.ADMIN.ToString(), fromDb.Role);
        }

        [Fact]
        public async Task AddAsync_SecondUser_BecomesUser()
        {
            // Arrange
            await _sut.AddAsync(new User
            {
                Email = "first@site.com",
                PasswordHash = "hash",
                Role = "IGNORED"
            });

            var second = new User
            {
                Email = "second@site.com",
                PasswordHash = "hash",
                Role = "IGNORED"
            };

            // Act
            await _sut.AddAsync(second);

            // Assert
            var fromDb = await _db.Users.SingleAsync(u => u.Email == "second@site.com");
            Assert.Equal(Role.USER.ToString(), fromDb.Role);
        }

        [Fact]
        public async Task AddAsync_DuplicateEmail_ThrowsDbUpdateException()
        {
            // Arrange
            await _sut.AddAsync(new User
            {
                Email = "dup@site.com",
                PasswordHash = "hash",
                Role = "IGNORED"
            });

            // Act + Assert
            await Assert.ThrowsAsync<DbUpdateException>(() =>
                _sut.AddAsync(new User
                {
                    Email = "dup@site.com",
                    PasswordHash = "hash",
                    Role = "IGNORED"
                }));
        }
    }
}