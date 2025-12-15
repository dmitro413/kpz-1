using System.ComponentModel.DataAnnotations;

namespace CourseWork.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Введіть старий пароль")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть новий пароль")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Мінімум 6 символів")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Повторіть новий пароль")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}