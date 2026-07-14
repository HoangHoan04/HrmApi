using System;

namespace HrmApi.Application.Common.Models
{
    /// <summary>
    /// Yêu cầu phân trang chung cho hệ thống
    /// </summary>
    public class PagedRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchText { get; set; } = string.Empty;
        public string? SortField { get; set; }
        public string? SortOrder { get; set; } // "asc" hoặc "desc"
    }
}
