using InvoiceService.Modules.Invoices.Domain;
using InvoiceService.Shared.Domain;

namespace InvoiceService.Modules.Senders.Domain;

public class Sender : BaseEntity
{
    public string CompanyName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? VatTaxId { get; set; }
    public string? Iban { get; set; }

    public ICollection<Invoice> Invoices { get; set; } = [];
}
