using System.Data;
using PdfDownloader.Domain.Exceptions;

public class Url
{
    public string Value { get; set; } = default!;

    public Url(string url)
    {
        
    }

    private void Validate(string value)
    {
        if (String.IsNullOrEmpty(value))
        {
            throw new NoNullAllowedException("Url was empty");
        }

        if (value.Count() > 1000000)
        {
            throw new TooLongException("Message was too long");
        }
    }
}