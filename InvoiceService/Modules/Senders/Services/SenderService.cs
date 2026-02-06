using InvoiceService.Modules.Senders.Domain;
using InvoiceService.Modules.Senders.DTOs;
using InvoiceService.Modules.Senders.Repository;
using InvoiceService.Shared.Exceptions;
using InvoiceService.Shared.Models;

namespace InvoiceService.Modules.Senders.Services;

public class SenderService : ISenderService
{
    private readonly ISenderRepository _repository;

    public SenderService(ISenderRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<SenderResponse>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var result = await _repository.GetAllAsync(pagination, ct);

        return new PagedResult<SenderResponse>
        {
            Items = result.Items.Select(MapToResponse).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<SenderResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var sender = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Sender), id);

        return MapToResponse(sender);
    }

    public async Task<SenderResponse> CreateAsync(CreateSenderRequest request, CancellationToken ct = default)
    {
        var sender = new Sender
        {
            Id = Guid.NewGuid(),
            CompanyName = request.CompanyName,
            ContactPerson = request.ContactPerson,
            Address = request.Address,
            Email = request.Email,
            Phone = request.Phone,
            VatTaxId = request.VatTaxId,
            Iban = request.Iban
        };

        await _repository.CreateAsync(sender, ct);
        return MapToResponse(sender);
    }

    public async Task<SenderResponse> UpdateAsync(Guid id, UpdateSenderRequest request, CancellationToken ct = default)
    {
        var existing = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Sender), id);

        existing.CompanyName = request.CompanyName;
        existing.ContactPerson = request.ContactPerson;
        existing.Address = request.Address;
        existing.Email = request.Email;
        existing.Phone = request.Phone;
        existing.VatTaxId = request.VatTaxId;
        existing.Iban = request.Iban;

        await _repository.UpdateAsync(existing, ct);
        return MapToResponse(existing);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var sender = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Sender), id);

        await _repository.DeleteAsync(sender, ct);
    }

    private static SenderResponse MapToResponse(Sender sender) => new(
        sender.Id,
        sender.CompanyName,
        sender.ContactPerson,
        sender.Address,
        sender.Email,
        sender.Phone,
        sender.VatTaxId,
        sender.Iban,
        sender.CreatedAt,
        sender.UpdatedAt);
}
