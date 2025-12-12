using System;
using System.Collections.Generic;

namespace CourseWork.Models;

public partial class DemoOrder
{
    public int OrderId { get; set; }

    public string? CustomerName { get; set; }

    public DateTime? OrderDate { get; set; }
}
