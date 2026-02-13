using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseWork.Core.Models;

public partial class ProductBatch
{
    public int BatchId { get; set; }

    public int VariantId { get; set; }

    public DateOnly ManufactureDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    [Required(ErrorMessage = "Введіть кількість")]
    [Range(0, 1000000, ErrorMessage = "Кількість не може бути від'ємною")]
    public int Stock { get; set; }

    [Range(0.01, 1000000, ErrorMessage = "Ціна закупівлі не може бути від'ємною або нульовою")]
    public decimal? PurchasePrice { get; set; }
    public string? SupplierName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ProductVariant Variant { get; set; } = null!;
}
