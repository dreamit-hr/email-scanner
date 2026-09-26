namespace EmailScanner.Application.Abstractions;

public sealed record PaginationRequest(int Page = 1, int PageSize = 50)
{
    public int SafePage => Math.Max(1, Page);
    public int SafePageSize => Math.Clamp(PageSize, 1, 200);
}
