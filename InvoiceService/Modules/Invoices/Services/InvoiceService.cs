using InvoiceService.Modules.Invoices.Domain;
using InvoiceService.Modules.Invoices.DTOs;
using InvoiceService.Modules.Invoices.Repository;
using InvoiceService.Shared.Exceptions;
using InvoiceService.Shared.Models;

namespace InvoiceService.Modules.Invoices.Services;

public class InvoiceAppService : IInvoiceService
{
    private readonly IInvoiceRepository _repository;

    public InvoiceAppService(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<InvoiceListResponse>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var result = await _repository.GetAllAsync(pagination, ct);

        return new PagedResult<InvoiceListResponse>
        {
            Items = result.Items.Select(MapToListResponse).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<InvoiceResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Invoice), id);

        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, CancellationToken ct = default)
    {
        var invoiceNumber = await _repository.GetNextInvoiceNumberAsync(ct);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            InvoiceDate = DateTime.SpecifyKind(request.InvoiceDate, DateTimeKind.Utc),
            DueDate = DateTime.SpecifyKind(request.DueDate, DateTimeKind.Utc),
            Currency = request.Currency,
            TaxRate = request.TaxRate,
            Notes = request.Notes,
            Status = InvoiceStatus.Draft,
            SenderId = request.SenderId,
            CustomerId = request.CustomerId,
            LineItems = request.LineItems.Select(li => new InvoiceLineItem
            {
                Id = Guid.NewGuid(),
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice
            }).ToList()
        };

        await _repository.CreateAsync(invoice, ct);

        var created = await _repository.GetByIdAsync(invoice.Id, ct);
        return MapToResponse(created!);
    }

    public async Task<InvoiceResponse> UpdateAsync(Guid id, UpdateInvoiceRequest request, CancellationToken ct = default)
    {
        var existing = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Invoice), id);

        existing.InvoiceDate = DateTime.SpecifyKind(request.InvoiceDate, DateTimeKind.Utc);
        existing.DueDate = DateTime.SpecifyKind(request.DueDate, DateTimeKind.Utc);
        existing.Currency = request.Currency;
        existing.TaxRate = request.TaxRate;
        existing.Notes = request.Notes;
        existing.Status = Enum.Parse<InvoiceStatus>(request.Status, true);
        existing.SenderId = request.SenderId;
        existing.CustomerId = request.CustomerId;

        existing.LineItems = request.LineItems.Select(li => new InvoiceLineItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = id,
            Description = li.Description,
            Quantity = li.Quantity,
            UnitPrice = li.UnitPrice
        }).ToList();

        await _repository.UpdateAsync(existing, ct);

        var updated = await _repository.GetByIdAsync(id, ct);
        return MapToResponse(updated!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Invoice), id);

        await _repository.DeleteAsync(invoice, ct);
    }

    private static InvoiceResponse MapToResponse(Invoice invoice) => new(
        invoice.Id,
        invoice.InvoiceNumber,
        invoice.InvoiceDate,
        invoice.DueDate,
        invoice.Currency,
        invoice.TaxRate,
        invoice.Notes,
        invoice.Status.ToString(),
        invoice.SenderId,
        invoice.Sender.CompanyName,
        invoice.CustomerId,
        invoice.Customer.CompanyName,
        invoice.LineItems.Select(li => new InvoiceLineItemResponse(
            li.Id,
            li.Description,
            li.Quantity,
            li.UnitPrice,
            li.Total)).ToList(),
        invoice.SubTotal,
        invoice.TaxAmount,
        invoice.GrandTotal,
        invoice.CreatedAt,
        invoice.UpdatedAt);

    private static InvoiceListResponse MapToListResponse(Invoice invoice) => new(
        invoice.Id,
        invoice.InvoiceNumber,
        invoice.InvoiceDate,
        invoice.DueDate,
        invoice.Currency,
        invoice.Status.ToString(),
        invoice.Sender.CompanyName,
        invoice.Customer.CompanyName,
        invoice.GrandTotal,
        invoice.CreatedAt);
}
