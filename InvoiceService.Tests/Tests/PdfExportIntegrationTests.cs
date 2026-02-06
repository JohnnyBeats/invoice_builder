using System.Net;
using System.Net.Http.Json;
using InvoiceService.Modules.Customers.DTOs;
using InvoiceService.Modules.Invoices.DTOs;
using InvoiceService.Modules.Senders.DTOs;
using InvoiceService.Tests.Infrastructure;

namespace InvoiceService.Tests.Tests;

public class PdfExportIntegrationTests : IClassFixture<InvoiceServiceFactory>
{
    private readonly HttpClient _client;

    public PdfExportIntegrationTests(InvoiceServiceFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<InvoiceResponse> CreateFullInvoiceAsync()
    {
        var customerRequest = new CreateCustomerRequest(
            CompanyName: $"PDF Customer {Guid.NewGuid():N}",
            ContactPerson: "PDF Contact",
            Address: "789 PDF Lane",
            Email: $"pdf{Guid.NewGuid():N}@customer.com",
            PostalCode: "55555",
            VatTaxId: "PDF-VAT-001");

        var customerResponse = await _client.PostAsJsonAsync("/api/customers", customerRequest);
        var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var senderRequest = new CreateSenderRequest(
            CompanyName: $"PDF Sender {Guid.NewGuid():N}",
            ContactPerson: "PDF Sender Contact",
            Address: "321 Sender Blvd",
            Email: $"pdf{Guid.NewGuid():N}@sender.com",
            Phone: "+9876543210",
            VatTaxId: "SEND-PDF-001",
            Iban: "GB29NWBK60161331926819");

        var senderResponse = await _client.PostAsJsonAsync("/api/senders", senderRequest);
        var sender = await senderResponse.Content.ReadFromJsonAsync<SenderResponse>();

        var invoiceRequest = new CreateInvoiceRequest(
            InvoiceDate: DateTime.UtcNow,
            DueDate: DateTime.UtcNow.AddDays(30),
            Currency: "GBP",
            TaxRate: 20.00m,
            Notes: "Thank you for your business",
            SenderId: sender!.Id,
            CustomerId: customer!.Id,
            LineItems:
            [
                new CreateInvoiceLineItemRequest("Web Development", 40, 125.00m),
                new CreateInvoiceLineItemRequest("UI/UX Design", 15, 95.00m)
            ]);

        var invoiceResponse = await _client.PostAsJsonAsync("/api/invoices", invoiceRequest);
        return (await invoiceResponse.Content.ReadFromJsonAsync<InvoiceResponse>())!;
    }

    [Fact]
    public async Task ExportPdf_ExistingInvoice_ReturnsPdfFile()
    {
        var invoice = await CreateFullInvoiceAsync();

        var response = await _client.GetAsync($"/api/invoices/{invoice.Id}/pdf");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(pdfBytes.Length > 0, "PDF should not be empty");

        // PDF files start with %PDF
        Assert.Equal((byte)'%', pdfBytes[0]);
        Assert.Equal((byte)'P', pdfBytes[1]);
        Assert.Equal((byte)'D', pdfBytes[2]);
        Assert.Equal((byte)'F', pdfBytes[3]);
    }

    [Fact]
    public async Task ExportPdf_NonExistentInvoice_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/invoices/{Guid.NewGuid()}/pdf");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
