using InvoiceService.Shared.Domain;

namespace InvoiceService.Modules.Invoices.Domain;

public class InvoiceLineItem : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;

    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
}
