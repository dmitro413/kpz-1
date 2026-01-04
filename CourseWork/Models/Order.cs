using System.ComponentModel.DataAnnotations;

namespace CourseWork.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public int StatusId { get; set; }
    [Required(ErrorMessage = "Введіть ім'я")]

    public string? CustomerName { get; set; }
    [Required(ErrorMessage = "Введіть Email")]
    [EmailAddress(ErrorMessage = "Некоректний Email")]
    public string? CustomerEmail { get; set; }
    [Required(ErrorMessage = "Введіть телефон")]
    [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Формат: +380xxxxxxxxx")]
    public string? CustomerPhone { get; set; }
    [Required(ErrorMessage = "Введіть адресу доставки")]

    public string? DeliveryAddress { get; set; }

    public DateTime DeliveryDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual OrderStatus Status { get; set; } = null!;

    public virtual User? User { get; set; }
}
