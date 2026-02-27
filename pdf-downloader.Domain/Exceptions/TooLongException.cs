namespace PdfDownloader.Domain.Exceptions;

public class TooLongException : Exception
{
    public TooLongException() { }

    public TooLongException(string message)
    : base(message) { }

    public TooLongException(string message, Exception inner)
    : base(message, inner) { }
}