using System;
using System.Collections.Generic;

namespace CourseWork.Core.Models;

public partial class TypeOfProduct
{
    public int TypeOfProductId { get; set; }

    public string TypeOfProductName { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
