using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Models
{
    public class SetPasswordViewModel
    {
        [Required(ErrorMessage = "Pole Nowe hasło jest wymagane.")]
        [StringLength(100, ErrorMessage = "Hasło musi mieć od {2} do 100 znaków.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]).*$",
    ErrorMessage = "Hasło musi zawierać co najmniej 1 małą literę, 1 dużą literę, 1 cyfrę i 1 znak specjalny.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nowe hasło:")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Pole Potwierdź hasło jest wymagane.")]
        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź nowe hasło:")]
        [Compare("NewPassword", ErrorMessage = "Hasło i jego potwierdzenie różnią się.")]
        [StringLength(100)]
        public string ConfirmPassword { get; set; }
    }
}