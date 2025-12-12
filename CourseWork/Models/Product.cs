using System;
using System.Collections.Generic;

namespace CourseWork.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public int TypeOfProductId { get; set; }

    public int BrandId { get; set; }

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
