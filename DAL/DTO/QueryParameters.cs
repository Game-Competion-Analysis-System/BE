namespace DAL.DTO
{
    public class QueryParameters
    {
        private const int MaxPageSize = 50;
        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = (value < 1) ? 1 : value;
        }
        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = true;
        public string? Filter { get; set; } 
    }

    public class AIQueryParameters : QueryParameters
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? GameName { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}