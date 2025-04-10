using Microsoft.AspNetCore.Mvc;

namespace FloByte.Application.Common.Models;

public class SearchOptions
{
    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = 10;

    [FromQuery(Name = "sortBy")]
    public string? SortBy { get; set; }

    [FromQuery(Name = "sortDirection")]
    public string? SortDirection { get; set; }

    [FromQuery(Name = "filter")]
    public string? Filter { get; set; }

    public int Skip => (Page - 1) * PageSize;

    public bool IsDescending => string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}
