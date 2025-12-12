namespace CourseWork.Models
{
    public class BrandStatsDto
    {
        public string BrandName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal EstimatedProfit { get; set; }

        public int SalesCount { get; set; }
    }
}