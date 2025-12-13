using Microsoft.EntityFrameworkCore;

namespace CourseWork.Models
{
    [Keyless]
    public class ExpiringBatchDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public decimal WeightValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int BatchID { get; set; }
        public DateOnly ManufactureDate { get; set; } 
        public DateOnly ExpiryDate { get; set; }    
        public int RemainingStock { get; set; }
    }
}