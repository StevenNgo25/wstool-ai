using System.ComponentModel.DataAnnotations;

namespace WhatsAppCampaignManager.DTOs
{
    public class PaginationRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 1000, ErrorMessage = "PageSize must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "desc"; // asc or desc
    }

    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public PaginationMetadata Pagination { get; set; } = new PaginationMetadata();
    }

    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }
}
