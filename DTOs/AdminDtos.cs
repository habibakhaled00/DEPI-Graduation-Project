namespace NeighborHelp.DTOs
{
    public class DashboardData
    {
        public int TotalUsers { get; set; }
        public int TotalRequests { get; set; }
        public int OpenRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int TotalMessages { get; set; }
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public List<CategoryStatDto> CategoryStats { get; set; } = new();
        public List<MonthlyStatDto> MonthlyStats { get; set; } = new();
    }

    public class CategoryStatDto
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }

    public class MonthlyStatDto
    {
        public string Month { get; set; } = "";
        public int Requests { get; set; }
        public int Completed { get; set; }
    }

    public class AdminUserDto
    {
        public string UserId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime JoinedDate { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
