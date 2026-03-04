using Microsoft.EntityFrameworkCore;
using PdfDownloader.Domain.Entities;

namespace PdfDownloader.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<PdfDownload> PdfDownloads => Set<PdfDownload>();

    // Is configuring multiple entities. Not a big issue with this amount of entities.
    // If more entities gets added, then consider extracting to different classes.
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        builder.Entity<PdfDownload>()
            .HasIndex(p => p.Url);
        builder.Entity<PdfDownload>()
            .HasIndex(p => p.BRnum)
            .IsUnique();
    }
}