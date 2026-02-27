using Microsoft.EntityFrameworkCore;
using PdfDownloader.Application.Interfaces;
using PdfDownloader.Domain.Entities;

namespace PdfDownloader.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        user.Role = Domain.Enums.Role.USER.ToString();

        //TODO Create the first admin in a better way
        if (_context.Users.Count() == 0)
        {
            user.Role = Domain.Enums.Role.ADMIN.ToString();
        }

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}