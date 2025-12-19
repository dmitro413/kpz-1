using Microsoft.AspNetCore.Mvc.Rendering;

namespace CourseWork.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<Brand> Brands { get; set; } = new List<Brand>();
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public IEnumerable<ProductBatch> Batches { get; set; } = new List<ProductBatch>();
        public IEnumerable<TypeOfProduct> Types { get; set; } = new List<TypeOfProduct>();
        public IEnumerable<Weight> Weights { get; set; } = new List<Weight>();
        public IEnumerable<User> Users { get; set; } = new List<User>();
        public IEnumerable<Review> Reviews { get; set; } = new List<Review>();
        public IEnumerable<Order> Orders { get; set; } = new List<Order>();
        public IEnumerable<OrderStatus> OrderStatuses { get; set; } = new List<OrderStatus>();
        public IEnumerable<ProductBatch> ExpiringBatches { get; set; } = new List<ProductBatch>();


        public List<BrandStatsDto> TopBrands { get; set; } = new();
        public List<LowStockProductDto> LowStockProducts { get; set; } = new();


        public int TotalBrands { get; set; }
        public int TotalProducts { get; set; }
        public int TotalVariants { get; set; }
        public int TotalBatches { get; set; }
        public int TotalTypes { get; set; }
        public int TotalWeights { get; set; }
        public int TotalUsers { get; set; }
        public int TotalReviews { get; set; }


        public bool ShowDeleted { get; set; }


        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalProducts / PageSize);

        public string? CurrentSearch { get; set; }
        public int? CurrentBrandId { get; set; }
        public int? CurrentTypeId { get; set; }

        public int ReportDaysUntilExpiry { get; set; } = 30;

        public int ReportTopCount { get; set; } = 5;
        public int ReportLowStockThreshold { get; set; } = 10;
        public SelectList? BrandList { get; set; }
        public SelectList? TypeList { get; set; }
    }
}