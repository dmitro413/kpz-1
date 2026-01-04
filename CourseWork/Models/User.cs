using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseWork.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;
    [Required(ErrorMessage = "Введіть Email")]
    [EmailAddress(ErrorMessage = "Некоректний Email")]
    public string Email { get; set; } = null!;
    [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Формат: +380xxxxxxxxx")]
    public string? Phone { get; set; }

    public string Role { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
