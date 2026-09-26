namespace EmailScanner.Application.Abstractions;

public sealed record PaginationResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount);
