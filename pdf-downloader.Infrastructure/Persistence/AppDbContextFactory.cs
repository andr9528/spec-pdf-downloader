using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PdfDownloader.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    // Need to get Connection a better way.
    // At the moment the port might be different per PC, and the password is highly likely to be different.
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=pdf_downloader;Username=postgres;Password=Wisdom8-Playset7-Delirium0-Half0-Lure5");
        
        return new AppDbContext(optionsBuilder.Options);
    }
}
