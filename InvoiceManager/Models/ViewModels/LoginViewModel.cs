using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Pole Email jest wymagane.")]
        [Display(Name = "Email")]
        [StringLength(254)]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Pole Hasło jest wymagane.")]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        [StringLength(100)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
