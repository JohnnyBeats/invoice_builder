using System.Net;
using System.Net.Http.Json;
using InvoiceService.Modules.Customers.DTOs;
using InvoiceService.Shared.Models;
using InvoiceService.Tests.Infrastructure;

namespace InvoiceService.Tests.Tests;

public class CustomerIntegrationTests : IClassFixture<InvoiceServiceFactory>
{
    private readonly HttpClient _client;

    public CustomerIntegrationTests(InvoiceServiceFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsCreated()
    {
        var request = new CreateCustomerRequest(
            CompanyName: "Acme Corp",
            ContactPerson: "John Doe",
            Address: "123 Main St, Springfield",
            Email: "john@acme.com",
            PostalCode: "12345",
            VatTaxId: "VAT123456");

        var response = await _client.PostAsJsonAsync("/api/customers", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(customer);
        Assert.Equal("Acme Corp", customer.CompanyName);
        Assert.Equal("john@acme.com", customer.Email);
        Assert.NotEqual(Guid.Empty, customer.Id);
    }

    [Fact]
    public async Task GetCustomer_AfterCreate_ReturnsCorrectData()
    {
        var request = new CreateCustomerRequest(
            CompanyName: "Widget Inc",
            ContactPerson: "Jane Smith",
            Address: "456 Oak Ave",
            Email: "jane@widget.com",
            PostalCode: "67890",
            VatTaxId: null);

        var createResponse = await _client.PostAsJsonAsync("/api/customers", request);
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var getResponse = await _client.GetAsync($"/api/customers/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var customer = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(customer);
        Assert.Equal("Widget Inc", customer.CompanyName);
        Assert.Equal("Jane Smith", customer.ContactPerson);
    }

    [Fact]
    public async Task UpdateCustomer_WithValidData_ReturnsUpdated()
    {
        var createRequest = new CreateCustomerRequest(
            CompanyName: "Old Name",
            ContactPerson: "Person",
            Address: "Address",
            Email: "old@test.com",
            PostalCode: "11111",
            VatTaxId: null);

        var createResponse = await _client.PostAsJsonAsync("/api/customers", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var updateRequest = new UpdateCustomerRequest(
            CompanyName: "New Name",
            ContactPerson: "Updated Person",
            Address: "New Address",
            Email: "new@test.com",
            PostalCode: "22222",
            VatTaxId: "VAT999");

        var updateResponse = await _client.PutAsJsonAsync($"/api/customers/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.CompanyName);
        Assert.Equal("new@test.com", updated.Email);
        Assert.Equal("VAT999", updated.VatTaxId);
    }

    [Fact]
    public async Task DeleteCustomer_ExistingCustomer_ReturnsNoContent()
    {
        var request = new CreateCustomerRequest(
            CompanyName: "To Delete",
            ContactPerson: "Person",
            Address: "Address",
            Email: "delete@test.com",
            PostalCode: "33333",
            VatTaxId: null);

        var createResponse = await _client.PostAsJsonAsync("/api/customers", request);
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/customers/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/customers/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetCustomers_WithPagination_ReturnsPagedResult()
    {
        for (int i = 0; i < 3; i++)
        {
            var req = new CreateCustomerRequest(
                CompanyName: $"Paged Co {Guid.NewGuid():N}",
                ContactPerson: "Person",
                Address: "Address",
                Email: $"paged{Guid.NewGuid():N}@test.com",
                PostalCode: "44444",
                VatTaxId: null);
            await _client.PostAsJsonAsync("/api/customers", req);
        }

        var response = await _client.GetAsync("/api/customers?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<CustomerResponse>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.True(result.TotalCount >= 3);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidData_ReturnsBadRequest()
    {
        var request = new CreateCustomerRequest(
            CompanyName: "",
            ContactPerson: "",
            Address: "",
            Email: "not-an-email",
            PostalCode: "",
            VatTaxId: null);

        var response = await _client.PostAsJsonAsync("/api/customers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomer_NonExistentId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
