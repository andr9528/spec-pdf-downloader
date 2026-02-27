using System.Data;
using ExcelDataReader;
using PdfDownloader.Infrastructure.Services.Interfaces;

namespace PdfDownloader.Infrastructure.Services;
public class ExcelReader : IReader{

    public async Task<List<PdfDownload>?> Read(Stream fileStream)
    {
        if (fileStream == null)
            return null;

        if (fileStream.CanSeek)
            fileStream.Position = 0;

        System.Text.Encoding.RegisterProvider(
            System.Text.CodePagesEncodingProvider.Instance);

        var result = new List<PdfDownload>();

        using var reader = ExcelReaderFactory.CreateReader(fileStream);

        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = (_) => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true
            }
        });

        var table = dataSet.Tables[0];

        foreach (DataRow row in table.Rows)
        {
            result.Add(new PdfDownload
            {
                Id = Guid.NewGuid(),
                BRnum = row["BRnum"]?.ToString() ?? "",
                Url = row["Pdf_URL"]?.ToString() ?? "",
                BackupUrl = row["Report Html Address"]?.ToString() ?? "",
                Title = row["Title"]?.ToString() ?? "",
                IsDownloaded = false,
                RetryCount = 0
            });
        }

        return result;
    }
}