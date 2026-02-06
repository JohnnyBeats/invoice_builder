using InvoiceService.Modules.Senders.Domain;
using InvoiceService.Shared.Models;

namespace InvoiceService.Modules.Senders.Repository;

public interface ISenderRepository
{
    Task<PagedResult<Sender>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<Sender?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Sender> CreateAsync(Sender sender, CancellationToken ct = default);
    Task<Sender> UpdateAsync(Sender sender, CancellationToken ct = default);
    Task DeleteAsync(Sender sender, CancellationToken ct = default);
}
