using System.ComponentModel.DataAnnotations;

namespace CourseWork.Core.Models
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Введіть назву товару")]
        public string Name { get; set; }

        public int TypeOfProductId { get; set; }

        public int BrandId { get; set; }

        [Range(0, 5000, ErrorMessage = "Калорійність не може бути від'ємною")]
        public decimal CaloriesPer100g { get; set; }

        public string? Description { get; set; }
    }
}