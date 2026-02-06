namespace InvoiceService.Shared.Models;

public record PaginationParams(int Page = 1, int PageSize = 10)
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize < 1 ? 10 : PageSize > 100 ? 100 : PageSize;
}
