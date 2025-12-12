using System;
using System.Collections.Generic;

namespace CourseWork.Models;

public partial class ProductBatch
{
    public int BatchId { get; set; }

    public int VariantId { get; set; }

    public DateOnly ManufactureDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public int Stock { get; set; }

    public decimal? PurchasePrice { get; set; }

    public string? SupplierName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ProductVariant Variant { get; set; } = null!;
}
