using System.Net;
using System.Net.Http.Json;
using InvoiceService.Modules.Customers.DTOs;
using InvoiceService.Modules.Invoices.DTOs;
using InvoiceService.Modules.Senders.DTOs;
using InvoiceService.Tests.Infrastructure;

namespace InvoiceService.Tests.Tests;

public class InvoiceIntegrationTests : IClassFixture<InvoiceServiceFactory>
{
    private readonly HttpClient _client;

    public InvoiceIntegrationTests(InvoiceServiceFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<CustomerResponse> CreateTestCustomerAsync()
    {
        var request = new CreateCustomerRequest(
            CompanyName: $"Test Customer {Guid.NewGuid():N}",
            ContactPerson: "Test Person",
            Address: "123 Test St",
            Email: $"test{Guid.NewGuid():N}@customer.com",
            PostalCode: "10001",
            VatTaxId: "CUST-VAT-001");

        var response = await _client.PostAsJsonAsync("/api/customers", request);
        return (await response.Content.ReadFromJsonAsync<CustomerResponse>())!;
    }

    private async Task<SenderResponse> CreateTestSenderAsync()
    {
        var request = new CreateSenderRequest(
            CompanyName: $"Test Sender {Guid.NewGuid():N}",
            ContactPerson: "Sender Person",
            Address: "456 Sender Ave",
            Email: $"test{Guid.NewGuid():N}@sender.com",
            Phone: "+1234567890",
            VatTaxId: "SEND-VAT-001",
            Iban: "DE89370400440532013000");

        var response = await _client.PostAsJsonAsync("/api/senders", request);
        return (await response.Content.ReadFromJsonAsync<SenderResponse>())!;
    }

    [Fact]
    public async Task CreateInvoice_WithLineItems_ReturnsCreatedWithCalculations()
    {
        var customer = await CreateTestCustomerAsync();
        var sender = await CreateTestSenderAsync();

        var request = new CreateInvoiceRequest(
            InvoiceDate: DateTime.UtcNow,
            DueDate: DateTime.UtcNow.AddDays(30),
            Currency: "USD",
            TaxRate: 15.00m,
            Notes: "Payment due within 30 days",
            SenderId: sender.Id,
            CustomerId: customer.Id,
            LineItems:
            [
                new CreateInvoiceLineItemRequest("Consulting Services", 10, 150.00m),
                new CreateInvoiceLineItemRequest("Development Work", 20, 200.00m),
                new CreateInvoiceLineItemRequest("Project Management", 5, 100.00m)
            ]);

        var response = await _client.PostAsJsonAsync("/api/invoices", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var invoice = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        Assert.NotNull(invoice);
        Assert.StartsWith("INV-", invoice.InvoiceNumber);
        Assert.Equal("Draft", invoice.Status);
        Assert.Equal(3, invoice.LineItems.Count);
        Assert.Equal(customer.Id, invoice.CustomerId);
        Assert.Equal(sender.Id, invoice.SenderId);

        // Verify calculations: (10*150) + (20*200) + (5*100) = 1500 + 4000 + 500 = 6000
        Assert.Equal(6000.00m, invoice.SubTotal);
        // Tax: 6000 * 15% = 900
        Assert.Equal(900.00m, invoice.TaxAmount);
        // Grand total: 6000 + 900 = 6900
        Assert.Equal(6900.00m, invoice.GrandTotal);
    }

    [Fact]
    public async Task UpdateInvoice_ChangeStatusAndLineItems_ReturnsUpdated()
    {
        var customer = await CreateTestCustomerAsync();
        var sender = await CreateTestSenderAsync();

        var createRequest = new CreateInvoiceRequest(
            InvoiceDate: DateTime.UtcNow,
            DueDate: DateTime.UtcNow.AddDays(30),
            Currency: "EUR",
            TaxRate: 20.00m,
            Notes: "First",
            SenderId: sender.Id,
            CustomerId: customer.Id,
            LineItems:
            [
                new CreateInvoiceLineItemRequest("Initial Item", 1, 100.00m)
            ]);

        var createResponse = await _client.PostAsJsonAsync("/api/invoices", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<InvoiceResponse>();

        var updateRequest = new UpdateInvoiceRequest(
            InvoiceDate: created!.InvoiceDate,
            DueDate: created.DueDate,
            Currency: "EUR",
            TaxRate: 20.00m,
            Notes: "Updated notes",
            Status: "Sent",
            SenderId: sender.Id,
            CustomerId: customer.Id,
            LineItems:
            [
                new CreateInvoiceLineItemRequest("Updated Item", 5, 250.00m),
                new CreateInvoiceLineItemRequest("New Item", 2, 300.00m)
            ]);

        var updateResponse = await _client.PutAsJsonAsync($"/api/invoices/{created.Id}", updateRequest);

        var body = await updateResponse.Content.ReadAsStringAsync();
        Assert.True(updateResponse.IsSuccessStatusCode, $"Update failed ({updateResponse.StatusCode}): {body}");

        var updated = await updateResponse.Content.ReadFromJsonAsync<InvoiceResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Sent", updated.Status);
        Assert.Equal("Updated notes", updated.Notes);
        Assert.Equal(2, updated.LineItems.Count);
        // (5*250) + (2*300) = 1250 + 600 = 1850
        Assert.Equal(1850.00m, updated.SubTotal);
    }

    [Fact]
    public async Task CreateInvoice_WithInvalidData_ReturnsBadRequest()
    {
        var request = new CreateInvoiceRequest(
            InvoiceDate: DateTime.UtcNow,
            DueDate: DateTime.UtcNow.AddDays(-1), // Due date before invoice date
            Currency: "INVALID",
            TaxRate: -5,
            Notes: null,
            SenderId: Guid.Empty,
            CustomerId: Guid.Empty,
            LineItems: []);

        var response = await _client.PostAsJsonAsync("/api/invoices", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetInvoice_NonExistentId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/invoices/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteInvoice_ExistingInvoice_ReturnsNoContent()
    {
        var customer = await CreateTestCustomerAsync();
        var sender = await CreateTestSenderAsync();

        var createRequest = new CreateInvoiceRequest(
            InvoiceDate: DateTime.UtcNow,
            DueDate: DateTime.UtcNow.AddDays(30),
            Currency: "USD",
            TaxRate: 10.00m,
            Notes: null,
            SenderId: sender.Id,
            CustomerId: customer.Id,
            LineItems:
            [
                new CreateInvoiceLineItemRequest("Item to delete", 1, 50.00m)
            ]);

        var createResponse = await _client.PostAsJsonAsync("/api/invoices", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<InvoiceResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/invoices/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/invoices/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
