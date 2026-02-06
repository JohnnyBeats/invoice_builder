using InvoiceService.Modules.Invoices.Domain;
using InvoiceService.Modules.Invoices.Repository;
using InvoiceService.Shared.Exceptions;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace InvoiceService.Modules.Invoices.Services;

public class PdfService : IPdfService
{
    private readonly IInvoiceRepository _repository;

    public PdfService(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Guid invoiceId, CancellationToken ct = default)
    {
        var invoice = await _repository.GetByIdAsync(invoiceId, ct)
            ?? throw new NotFoundException(nameof(Invoice), invoiceId);

        var html = BuildInvoiceHtml(invoice);

        var executablePath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");

        if (string.IsNullOrEmpty(executablePath))
        {
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
        }

        var launchOptions = new LaunchOptions
        {
            Headless = true,
            Args = ["--no-sandbox", "--disable-setuid-sandbox"]
        };

        if (!string.IsNullOrEmpty(executablePath))
            launchOptions.ExecutablePath = executablePath;

        await using var browser = await Puppeteer.LaunchAsync(launchOptions);
        await using var page = await browser.NewPageAsync();

        await page.SetContentAsync(html);
        var pdfBytes = await page.PdfDataAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "20mm",
                Bottom = "20mm",
                Left = "15mm",
                Right = "15mm"
            }
        });

        return pdfBytes;
    }

    private static string BuildInvoiceHtml(Invoice invoice)
    {
        var lineItemsHtml = string.Join("", invoice.LineItems.Select((li, index) =>
            $@"<tr>
                <td>{index + 1}</td>
                <td>{Escape(li.Description)}</td>
                <td class=""right"">{li.Quantity:N2}</td>
                <td class=""right"">{invoice.Currency} {li.UnitPrice:N2}</td>
                <td class=""right"">{invoice.Currency} {li.Total:N2}</td>
            </tr>"));

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: 'Segoe UI', Arial, sans-serif; font-size: 14px; color: #333; }}
        .container {{ padding: 20px; }}
        .header {{ display: flex; justify-content: space-between; margin-bottom: 40px; }}
        .header h1 {{ font-size: 28px; color: #2c3e50; }}
        .invoice-info {{ text-align: right; }}
        .invoice-info p {{ margin: 4px 0; }}
        .parties {{ display: flex; justify-content: space-between; margin-bottom: 30px; }}
        .party {{ width: 45%; }}
        .party h3 {{ color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 5px; margin-bottom: 10px; }}
        .party p {{ margin: 3px 0; }}
        table {{ width: 100%; border-collapse: collapse; margin-bottom: 30px; }}
        th {{ background-color: #3498db; color: white; padding: 10px 8px; text-align: left; }}
        td {{ padding: 8px; border-bottom: 1px solid #ddd; }}
        tr:nth-child(even) {{ background-color: #f9f9f9; }}
        .right {{ text-align: right; }}
        .totals {{ float: right; width: 300px; }}
        .totals table {{ margin-bottom: 0; }}
        .totals td {{ border-bottom: 1px solid #ddd; padding: 6px 8px; }}
        .totals tr:last-child td {{ font-weight: bold; font-size: 16px; border-top: 2px solid #3498db; }}
        .notes {{ clear: both; margin-top: 40px; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #3498db; }}
        .notes h3 {{ margin-bottom: 5px; color: #2c3e50; }}
        .status {{ display: inline-block; padding: 4px 12px; border-radius: 4px; font-weight: bold; font-size: 12px; color: white; background-color: {GetStatusColor(invoice.Status)}; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div>
                <h1>INVOICE</h1>
                <span class=""status"">{invoice.Status}</span>
            </div>
            <div class=""invoice-info"">
                <p><strong>Invoice #:</strong> {Escape(invoice.InvoiceNumber)}</p>
                <p><strong>Date:</strong> {invoice.InvoiceDate:yyyy-MM-dd}</p>
                <p><strong>Due Date:</strong> {invoice.DueDate:yyyy-MM-dd}</p>
                <p><strong>Currency:</strong> {Escape(invoice.Currency)}</p>
            </div>
        </div>

        <div class=""parties"">
            <div class=""party"">
                <h3>From</h3>
                <p><strong>{Escape(invoice.Sender.CompanyName)}</strong></p>
                <p>{Escape(invoice.Sender.ContactPerson)}</p>
                <p>{Escape(invoice.Sender.Address)}</p>
                <p>{Escape(invoice.Sender.Email)}</p>
                <p>{Escape(invoice.Sender.Phone)}</p>
                {(string.IsNullOrEmpty(invoice.Sender.VatTaxId) ? "" : $"<p>VAT/Tax ID: {Escape(invoice.Sender.VatTaxId)}</p>")}
                {(string.IsNullOrEmpty(invoice.Sender.Iban) ? "" : $"<p>IBAN: {Escape(invoice.Sender.Iban)}</p>")}
            </div>
            <div class=""party"">
                <h3>Bill To</h3>
                <p><strong>{Escape(invoice.Customer.CompanyName)}</strong></p>
                <p>{Escape(invoice.Customer.ContactPerson)}</p>
                <p>{Escape(invoice.Customer.Address)}</p>
                <p>{Escape(invoice.Customer.Email)}</p>
                <p>{Escape(invoice.Customer.PostalCode)}</p>
                {(string.IsNullOrEmpty(invoice.Customer.VatTaxId) ? "" : $"<p>VAT/Tax ID: {Escape(invoice.Customer.VatTaxId)}</p>")}
            </div>
        </div>

        <table>
            <thead>
                <tr>
                    <th style=""width:5%"">#</th>
                    <th style=""width:45%"">Description</th>
                    <th class=""right"" style=""width:15%"">Quantity</th>
                    <th class=""right"" style=""width:17.5%"">Unit Price</th>
                    <th class=""right"" style=""width:17.5%"">Total</th>
                </tr>
            </thead>
            <tbody>
                {lineItemsHtml}
            </tbody>
        </table>

        <div class=""totals"">
            <table>
                <tr>
                    <td>Subtotal</td>
                    <td class=""right"">{invoice.Currency} {invoice.SubTotal:N2}</td>
                </tr>
                <tr>
                    <td>Tax ({invoice.TaxRate:N2}%)</td>
                    <td class=""right"">{invoice.Currency} {invoice.TaxAmount:N2}</td>
                </tr>
                <tr>
                    <td>Grand Total</td>
                    <td class=""right"">{invoice.Currency} {invoice.GrandTotal:N2}</td>
                </tr>
            </table>
        </div>

        {(string.IsNullOrEmpty(invoice.Notes) ? "" : $@"<div class=""notes""><h3>Notes</h3><p>{Escape(invoice.Notes)}</p></div>")}
    </div>
</body>
</html>";
    }

    private static string Escape(string? value) =>
        System.Net.WebUtility.HtmlEncode(value ?? string.Empty);

    private static string GetStatusColor(InvoiceStatus status) => status switch
    {
        InvoiceStatus.Draft => "#95a5a6",
        InvoiceStatus.Sent => "#3498db",
        InvoiceStatus.Paid => "#27ae60",
        InvoiceStatus.Overdue => "#e74c3c",
        InvoiceStatus.Cancelled => "#e67e22",
        _ => "#95a5a6"
    };
}
