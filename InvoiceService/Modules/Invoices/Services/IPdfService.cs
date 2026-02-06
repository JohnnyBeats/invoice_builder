namespace InvoiceService.Modules.Invoices.Services;

public interface IPdfService
{
    Task<byte[]> GenerateInvoicePdfAsync(Guid invoiceId, CancellationToken ct = default);
}
