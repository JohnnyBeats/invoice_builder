using InvoiceService.Modules.Senders.DTOs;
using InvoiceService.Shared.Models;

namespace InvoiceService.Modules.Senders.Services;

public interface ISenderService
{
    Task<PagedResult<SenderResponse>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<SenderResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SenderResponse> CreateAsync(CreateSenderRequest request, CancellationToken ct = default);
    Task<SenderResponse> UpdateAsync(Guid id, UpdateSenderRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
