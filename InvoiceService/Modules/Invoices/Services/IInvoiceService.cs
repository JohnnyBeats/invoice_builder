using InvoiceService.Modules.Invoices.DTOs;
using InvoiceService.Shared.Models;

namespace InvoiceService.Modules.Invoices.Services;

public interface IInvoiceService
{
    Task<PagedResult<InvoiceListResponse>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<InvoiceResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, CancellationToken ct = default);
    Task<InvoiceResponse> UpdateAsync(Guid id, UpdateInvoiceRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
