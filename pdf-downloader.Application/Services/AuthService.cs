using BCrypt.Net;
using pdf_downloader.Domain.Exceptions;
using PdfDownloader.Application.DTOs;
using PdfDownloader.Application.Interfaces;
using PdfDownloader.Domain.Entities;
using PdfDownloader.Domain.Exceptions;

namespace PdfDownloader.Application.Services;
public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwt;

    public AuthService(IUserRepository userRepository,
                       IJwtTokenGenerator jwt)
    {
        _userRepository = userRepository;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing != null)
            throw new UserAlreadyExistsException("User already exists");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _userRepository.AddAsync(user);

        var token = _jwt.GenerateToken(user);

        return new AuthResponse(token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new InvalidCredentialsException("Invalid credentials");

        var token = _jwt.GenerateToken(user);

        return new AuthResponse(token);
    }
}