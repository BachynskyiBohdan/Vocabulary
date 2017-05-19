using System.ComponentModel.DataAnnotations;
using Vocabulary.Web.App_LocalResources;

namespace Vocabulary.Web.Areas.Main.Models
{
    public class LoginViewModel
    {
        [Required]
        [RegularExpression(@"[a-zA-Z][a-zA-Z0-9@.]{4,29}", ErrorMessageResourceType = typeof(GlobalRes), 
            ErrorMessageResourceName = "UsernameParameterError")]
        public string UserName { get; set; }

        [Required, DataType(DataType.Password)]
        [RegularExpression(@"^[A-Za-z0-9]{6,16}$", ErrorMessageResourceType = typeof(GlobalRes), 
            ErrorMessageResourceName = "PasswordParameterError")]
        public string Password { get; set; }
    }
}