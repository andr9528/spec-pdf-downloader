using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using pdf_downloader.Domain.Exceptions;
using PdfDownloader.Application.DTOs;
using PdfDownloader.Application.Services;
using PdfDownloader.Domain.Exceptions;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }
        catch (UserAlreadyExistsException e)
        {
            return Conflict(new { e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { e.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (InvalidCredentialsException exception)
        {
            return Unauthorized(exception.Message);
        }
    }
}