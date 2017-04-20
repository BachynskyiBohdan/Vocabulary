using System.ComponentModel.DataAnnotations;

namespace Vocabulary.Web.Models.Account
{
    public class LoginViewModel
    {
        [Required]
        [RegularExpression(@"[a-zA-Z][a-zA-Z0-9@.]{4,29}", ErrorMessage = "Имя должно содержать только латинские литеры и цифры и начинаться с литеры. Минимум 5, максимум 30.")]
        public string UserName { get; set; }

        [Required, DataType(DataType.Password)]
        [RegularExpression(@"^[A-Za-z0-9]{6,16}$", ErrorMessage = "Пароль должен содержать только литеры, цифры. Минимум 6, максимум 16.")]
        public string Password { get; set; }
    }
}