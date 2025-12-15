using System.ComponentModel.DataAnnotations;

namespace CourseWork.Models
{
    public class RegisterViewModel
    {
        [Display(Name = "ПІБ")]
        [Required(ErrorMessage = "Введіть ваше ім'я")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Введіть Email")]
        [EmailAddress(ErrorMessage = "Некоректний Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Телефон")]
        [Phone(ErrorMessage = "Некоректний номер телефону")]
        public string? Phone { get; set; }

        [Display(Name = "Пароль")]
        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Пароль має бути мінімум 6 символів")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Підтвердження пароля")]
        [Required(ErrorMessage = "Повторіть пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}