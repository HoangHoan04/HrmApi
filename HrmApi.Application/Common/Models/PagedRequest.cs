using System;

namespace HrmApi.Application.Common.Models
{
    public class PagedRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; } = string.Empty;
        public string? SortField { get; set; }
        public string? SortOrder { get; set; }
    }
}
