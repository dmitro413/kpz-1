using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseWork.Models;

public partial class Weight
{
    public int WeightId { get; set; }

    [Required(ErrorMessage = "Введіть значення ваги")]
    [Range(0.01, 100000, ErrorMessage = "Вага має бути більшою за 0")]
    public decimal WeightValue { get; set; }

    [Required(ErrorMessage = "Введіть одиницю виміру (г, кг, мл)")]
    public string? Unit { get; set; }

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}
