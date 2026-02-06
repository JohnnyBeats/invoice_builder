namespace InvoiceService.Shared.Models;

public record PaginationParams(int Page = 1, int PageSize = 10, string? Search = null)
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize < 1 ? 10 : PageSize > 100 ? 100 : PageSize;
    public string? Search { get; init; } = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();
}
