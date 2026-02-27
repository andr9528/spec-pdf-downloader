using Microsoft.Extensions.DependencyInjection;
using pdf_downloader.Infrastructure.Services;
using pdf_downloader.Infrastructure.Services.Interfaces;
using PdfDownloader.Application.Interfaces;
using PdfDownloader.Infrastructure;
using PdfDownloader.Infrastructure.Persistence;
using PdfDownloader.Infrastructure.Services;
using PdfDownloader.Infrastructure.Services.Interfaces;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDownloadRepository, DownloadRepository>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IReader, ExcelReader>();
        services.AddScoped<IDownloader, PdfsDownloader>();
        services.AddScoped<IFileService, FileService>();

        return services;
    }
}