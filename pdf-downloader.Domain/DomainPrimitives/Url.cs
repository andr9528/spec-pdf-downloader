using System.Data;
using PdfDownloader.Domain.Exceptions;

// Extending `Uri` might have been a good choice, if this should be kept.
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