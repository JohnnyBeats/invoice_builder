using InvoiceService.Modules.Customers.Domain;
using InvoiceService.Modules.Customers.DTOs;
using InvoiceService.Modules.Customers.Repository;
using InvoiceService.Shared.Exceptions;
using InvoiceService.Shared.Models;

namespace InvoiceService.Modules.Customers.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<CustomerResponse>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var result = await _repository.GetAllAsync(pagination, ct);

        return new PagedResult<CustomerResponse>
        {
            Items = result.Items.Select(MapToResponse).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Customer), id);

        return MapToResponse(customer);
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            CompanyName = request.CompanyName,
            ContactPerson = request.ContactPerson,
            Address = request.Address,
            Email = request.Email,
            PostalCode = request.PostalCode,
            VatTaxId = request.VatTaxId
        };

        await _repository.CreateAsync(customer, ct);
        return MapToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        var existing = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Customer), id);

        existing.CompanyName = request.CompanyName;
        existing.ContactPerson = request.ContactPerson;
        existing.Address = request.Address;
        existing.Email = request.Email;
        existing.PostalCode = request.PostalCode;
        existing.VatTaxId = request.VatTaxId;

        await _repository.UpdateAsync(existing, ct);
        return MapToResponse(existing);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Customer), id);

        await _repository.DeleteAsync(customer, ct);
    }

    private static CustomerResponse MapToResponse(Customer customer) => new(
        customer.Id,
        customer.CompanyName,
        customer.ContactPerson,
        customer.Address,
        customer.Email,
        customer.PostalCode,
        customer.VatTaxId,
        customer.CreatedAt,
        customer.UpdatedAt);
}
