using InvoiceService.Data;
using InvoiceService.Modules.Senders.Domain;
using InvoiceService.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Modules.Senders.Repository;

public class SenderRepository : ISenderRepository
{
    private readonly AppDbContext _db;

    public SenderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<Sender>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _db.Senders.AsNoTracking().OrderBy(s => s.CompanyName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Sender>
        {
            Items = items,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Sender?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Senders.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<Sender> CreateAsync(Sender sender, CancellationToken ct = default)
    {
        _db.Senders.Add(sender);
        await _db.SaveChangesAsync(ct);
        return sender;
    }

    public async Task<Sender> UpdateAsync(Sender sender, CancellationToken ct = default)
    {
        _db.Senders.Update(sender);
        await _db.SaveChangesAsync(ct);
        return sender;
    }

    public async Task DeleteAsync(Sender sender, CancellationToken ct = default)
    {
        _db.Senders.Remove(sender);
        await _db.SaveChangesAsync(ct);
    }
}
