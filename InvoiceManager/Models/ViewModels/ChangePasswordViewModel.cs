using InvoiceManager.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Models
{
    public class ChangePasswordViewModel : SetPasswordViewModel
    {
        [Required(ErrorMessage = "Pole Hasło jest wymagane.")]
        [DataType(DataType.Password)]
        [Display(Name = "Stare hasło:")]
        public string OldPassword { get; set; }

        [NotEqualToProperty("OldPassword", ErrorMessage = "Nowe hasło musi różnić się od starego hasła.")]
        public new string NewPassword { get; set; }

        public bool RememberMe { get; set; }
    }
}