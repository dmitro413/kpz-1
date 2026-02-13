using Microsoft.EntityFrameworkCore;

namespace CourseWork.Core.Models
{
    public class LowStockProductDto
    {
        public int VariantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public int TotalStock { get; set; }
    }
}