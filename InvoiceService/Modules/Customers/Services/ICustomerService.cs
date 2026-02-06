using InvoiceService.Modules.Customers.DTOs;
using InvoiceService.Shared.Models;

namespace InvoiceService.Modules.Customers.Services;

public interface ICustomerService
{
    Task<PagedResult<CustomerResponse>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
    Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
