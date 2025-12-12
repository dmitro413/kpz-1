using System;
using System.Collections.Generic;

namespace CourseWork.Models;

public partial class DemoOrderDetail
{
    public int DetailId { get; set; }

    public int? OrderId { get; set; }

    public int? ProductId { get; set; }

    public int? Quantity { get; set; }

    public decimal? Price { get; set; }
}
