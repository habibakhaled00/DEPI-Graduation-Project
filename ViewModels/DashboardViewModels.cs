using NeighborHelp.Models;

namespace NeighborHelp.ViewModels
{
    public class DashboardViewModel
    {
        public List<HelpRequest> MyRequests { get; set; } = new();
        public List<VolunteerRequest> MyVolunteering { get; set; } = new();
        public int UnreadMessages { get; set; }
        public int UnreadNotifications { get; set; }
        public double MyRating { get; set; }
        public int TotalHelped { get; set; }
        public int TotalRequests { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalRequests { get; set; }
        public int OpenRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int TotalMessages { get; set; }
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public List<CategoryStat> CategoryStats { get; set; } = new();
        public List<MonthlyStat> MonthlyStats { get; set; } = new();
        public List<ApplicationUser> RecentUsers { get; set; } = new();
        public List<HelpRequest> RecentRequests { get; set; } = new();
        public List<AdminLog> RecentLogs { get; set; } = new();
    }

    public class CategoryStat
    {
        public string CategoryName { get; set; } = "";
        public int Count { get; set; }
    }

    public class MonthlyStat
    {
        public string Month { get; set; } = "";
        public int Requests { get; set; }
        public int Completed { get; set; }
    }
}
