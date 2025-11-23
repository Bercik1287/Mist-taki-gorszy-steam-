using mist.Models;

namespace mist.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalGames { get; set; }
        public int TotalPurchases { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActivePromotions { get; set; }
        public List<Purchase> RecentPurchases { get; set; }
    }

    public class StatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalGames { get; set; }
        public int TotalPurchases { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<GameStatistic> TopGames { get; set; }
        public int Last30DaysSales { get; set; }
        public decimal Last30DaysRevenue { get; set; }
    }

    public class GameStatistic
    {
        public Game Game { get; set; }
        public int SalesCount { get; set; }
        public decimal Revenue { get; set; }
    }
}