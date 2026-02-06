using InvoiceService.Modules.Customers.Domain;
using InvoiceService.Shared.Models;

namespace InvoiceService.Modules.Customers.Repository;

public interface ICustomerRepository
{
    Task<PagedResult<Customer>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Customer> CreateAsync(Customer customer, CancellationToken ct = default);
    Task<Customer> UpdateAsync(Customer customer, CancellationToken ct = default);
    Task DeleteAsync(Customer customer, CancellationToken ct = default);
}
