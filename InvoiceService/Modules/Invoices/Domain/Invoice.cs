using InvoiceService.Modules.Customers.Domain;
using InvoiceService.Modules.Senders.Domain;
using InvoiceService.Shared.Domain;

namespace InvoiceService.Modules.Invoices.Domain;

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal TaxRate { get; set; }
    public string? Notes { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public Guid SenderId { get; set; }
    public Sender Sender { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public ICollection<InvoiceLineItem> LineItems { get; set; } = [];

    public decimal SubTotal => LineItems.Sum(li => li.Total);
    public decimal TaxAmount => SubTotal * (TaxRate / 100m);
    public decimal GrandTotal => SubTotal + TaxAmount;
}
