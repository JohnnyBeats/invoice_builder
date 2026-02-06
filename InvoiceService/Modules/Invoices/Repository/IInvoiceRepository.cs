using InvoiceService.Modules.Invoices.Domain;
using InvoiceService.Shared.Models;

namespace InvoiceService.Modules.Invoices.Repository;

public interface IInvoiceRepository
{
    Task<PagedResult<Invoice>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default);
    Task<Invoice> CreateAsync(Invoice invoice, CancellationToken ct = default);
    Task<Invoice> UpdateAsync(Invoice invoice, CancellationToken ct = default);
    Task DeleteAsync(Invoice invoice, CancellationToken ct = default);
}
