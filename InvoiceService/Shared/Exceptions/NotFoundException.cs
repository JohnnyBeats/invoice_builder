namespace InvoiceService.Shared.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, Guid id)
        : base($"{entityName} with ID '{id}' was not found.")
    {
    }
}
