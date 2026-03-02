using Microsoft.EntityFrameworkCore;
using PdfDownloader.Application.Interfaces;
using PdfDownloader.Domain.Entities;

namespace PdfDownloader.Infrastructure.Persistence;

public class DownloadRepository : IDownloadRepository
{
    private readonly AppDbContext _context;

    public DownloadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PdfDownload> CreateOrUpdate(PdfDownload download)
    {
        var existing = _context.PdfDownloads
            .SingleOrDefault(x => x.BRnum == download.BRnum);

        if (existing == null)
        {
            await _context.PdfDownloads.AddAsync(download);
            _context.SaveChanges();
            return download;
        }

        download.Id = existing.Id;

        _context.Entry(existing).CurrentValues.SetValues(download);

        _context.SaveChanges();
        return existing;
    }

    public List<PdfDownload> GetDownloads()
    {
        return _context.PdfDownloads.ToList();
    }

    /// <inheritdoc />
    public byte[] GetFile(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            throw new FileNotFoundException("Could not find the requested PDF");
        }

        return System.IO.File.ReadAllBytes(path);
    }

    public PdfDownload? GetDownload(Guid id)
    {
        return _context.PdfDownloads
            .Where(download => download.Id == id)
            .FirstOrDefault();
    }

    public List<PdfDownload> GetDownloads(List<Guid> ids)
    {
        return _context.PdfDownloads
                .Where(download => ids.Contains(download.Id))
                .ToList();
    }

    public List<PdfDownload> GetNotDownloaded()
    {
        return _context.PdfDownloads
            .Where(download => download.IsDownloaded == false && download.ErrorMessage == null)
            .ToList();
    }
}