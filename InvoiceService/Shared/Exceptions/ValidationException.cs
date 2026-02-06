namespace InvoiceService.Shared.Exceptions;

public class ApiValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ApiValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
