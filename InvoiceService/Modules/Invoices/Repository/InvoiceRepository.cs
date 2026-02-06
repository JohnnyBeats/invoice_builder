using InvoiceService.Data;
using InvoiceService.Modules.Invoices.Domain;
using InvoiceService.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Modules.Invoices.Repository;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _db;

    public InvoiceRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<Invoice>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _db.Invoices
            .AsNoTracking()
            .Include(i => i.Sender)
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .OrderByDescending(i => i.InvoiceDate);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Invoice>
        {
            Items = items,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Invoices
            .Include(i => i.Sender)
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default)
    {
        var lastInvoice = await _db.Invoices
            .AsNoTracking()
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (lastInvoice is null)
            return "INV-0001";

        var lastNumber = lastInvoice.InvoiceNumber.Replace("INV-", "");
        if (int.TryParse(lastNumber, out var number))
            return $"INV-{(number + 1):D4}";

        return $"INV-{DateTime.UtcNow.Ticks}";
    }

    public async Task<Invoice> CreateAsync(Invoice invoice, CancellationToken ct = default)
    {
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);
        return invoice;
    }

    public async Task<Invoice> UpdateAsync(Invoice invoice, CancellationToken ct = default)
    {
        _db.Attach(invoice);
        _db.InvoiceLineItems.AddRange(invoice.LineItems);

        _db.Invoices.Update(invoice);
        await _db.SaveChangesAsync(ct);
        return invoice;
    }

    public async Task DeleteAsync(Invoice invoice, CancellationToken ct = default)
    {
        _db.Invoices.Remove(invoice);
        await _db.SaveChangesAsync(ct);
    }
}
