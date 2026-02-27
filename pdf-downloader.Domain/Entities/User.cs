using PdfDownloader.Domain.Enums;

namespace PdfDownloader.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;

    public string Role { get; set; } =default!;
}