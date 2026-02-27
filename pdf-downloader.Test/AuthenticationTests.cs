using System.Threading.Tasks;
using Moq;
using Xunit;
using PdfDownloader.Application.Services;
using PdfDownloader.Application.Interfaces;
using PdfDownloader.Application.DTOs;
using PdfDownloader.Domain.Entities;
using pdf_downloader.Domain.Exceptions;
using PdfDownloader.Domain.Enums;
using PdfDownloader.Domain.Exceptions;

namespace pdf_downloader.Test;

public class AuthenticationTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtTokenGenerator> _jwtMock;
    private readonly AuthService _authService;

    public AuthenticationTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtTokenGenerator>();
        _authService = new AuthService(_userRepoMock.Object, _jwtMock.Object);
    }

    private RegisterRequest CreateRegisterRequest(
        string? email = null,
        string? password = null)
        => new RegisterRequest(
            email ?? "test@test.com",
            password ?? "Password123"
        );
    
    private LoginRequest CreateLoginRequest(string? email = null, string? password = null)
    {
        LoginRequest login = new LoginRequest();
        login.Email = email ?? "test@test.com";
        login.Password = password ?? "Password123";
        return login;
    }

    private User CreateUser(string email = "test@example.com", string password = "password123")
    {
        return new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };
    }


    [Fact]
    public async Task RegisterAsync_Should_Create_User_And_Return_Token()
    {
        var request = CreateRegisterRequest();

        _userRepoMock
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _jwtMock
            .Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns("fake-jwt-token");

        var result = await _authService.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.Equal("fake-jwt-token", result.Token);

        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_Should_Throw_When_User_Already_Exists()
    {
        var request = CreateRegisterRequest();

        _userRepoMock
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(new User());

        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ValidRequest_ReturnsAuthResponse()
    {
        string password = "TheRightPassword";
        // Arrange
        var user = CreateUser(password: password);
        _userRepoMock.Setup(s => s.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _jwtMock.Setup(j => j.GenerateToken(user)).Returns("token123");

        var request = CreateLoginRequest(user.Email, password);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("token123", result.Token);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsInvalidCredentialsException()
    {
        // Arrange
        _userRepoMock.Setup(s => s.GetByEmailAsync("unknown@example.com"))
                     .ReturnsAsync((User)null);

        var request = new LoginRequest
        {
            Email = "unknown@example.com",
            Password = "anyPassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_PasswordVerificationFails_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var user = CreateUser();
        _userRepoMock.Setup(s => s.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var request = new LoginRequest
        {
            Email = user.Email,
            Password = "wrongpassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _authService.LoginAsync(request));
    }
}