using InvoiceService.Data;
using InvoiceService.Modules.Customers.Domain;
using InvoiceService.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Modules.Customers.Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<Customer>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _db.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(pagination.Search))
        {
            var search = pagination.Search.ToLower();
            query = query.Where(c =>
                c.CompanyName.ToLower().Contains(search) ||
                c.ContactPerson.ToLower().Contains(search) ||
                c.Email.ToLower().Contains(search) ||
                c.Address.ToLower().Contains(search));
        }

        query = query.OrderBy(c => c.CompanyName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Customer>
        {
            Items = items,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Customer> CreateAsync(Customer customer, CancellationToken ct = default)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);
        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        _db.Customers.Update(customer);
        await _db.SaveChangesAsync(ct);
        return customer;
    }

    public async Task DeleteAsync(Customer customer, CancellationToken ct = default)
    {
        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync(ct);
    }
}
