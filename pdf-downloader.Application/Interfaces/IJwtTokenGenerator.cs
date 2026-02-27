using PdfDownloader.Domain.Entities;

namespace PdfDownloader.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}