using Microsoft.EntityFrameworkCore;

namespace CourseWork.Models
{
    [Keyless]
    public class LowStockProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public int TotalStock { get; set; }
    }
}