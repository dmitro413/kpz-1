namespace CourseWork.Models
{
    public class DashboardFilter
    {
        public string? SearchString { get; set; }
        public int? BrandId { get; set; }
        public int? TypeId { get; set; }
        public bool ShowDeleted { get; set; }
        public int Page { get; set; } = 1;

        public string? OrderSearch { get; set; } = "";
        public int? OrderStatusId { get; set; }
        public int OrderPage { get; set; } = 1;

        public string? VariantSearch { get; set; } = "";
        public int VariantPage { get; set; } = 1;

        public int? ReportTopCount { get; set; }
        public int? ReportLowStock { get; set; }
        public int? ReportDaysExpiry { get; set; }

        public string? UserSearch { get; set; } = "";
        public int UserPage { get; set; } = 1;
    }
}