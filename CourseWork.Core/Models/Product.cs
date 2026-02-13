using CourseWork.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace CourseWork.Core.Models;

public partial class Product
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Введіть назву товару")]
    public string Name { get; set; } = null!;

    public int TypeOfProductId { get; set; }

    public int BrandId { get; set; }

    [Range(0, 5000, ErrorMessage = "Калорійність не може бути від'ємною")]
    public decimal CaloriesPer100g { get; set; }
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int AggregateRating { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual TypeOfProduct TypeOfProduct { get; set; } = null!;
}
