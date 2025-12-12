using System;
using System.Collections.Generic;

namespace CourseWork.Models;

public partial class Weight
{
    public int WeightId { get; set; }

    public decimal WeightValue { get; set; }

    public string? Unit { get; set; }

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}
