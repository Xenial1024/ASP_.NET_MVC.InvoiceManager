using InvoiceManager.Models.Domains;
using System.ComponentModel.DataAnnotations;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace InvoiceManager.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Pole Nazwa jest wymagane.")]
        [StringLength(128, ErrorMessage = "Nazwa jest zbyt długa (maksymalnie 128 znaków).")]
        [Display(Name = "Nazwa")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Pole Email jest wymagane.")]
        [StringLength(254, ErrorMessage = "Adres email jest zbyt długi (maksymalnie 254 znaki).")]
        [RegularExpression(@"^[^\s@]+@[^\s@]+\.[^\s@]+$",
        ErrorMessage = "Podany adres email jest nieprawidłowy.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Pole Hasło jest wymagane.")]
        [StringLength(100, ErrorMessage = "Hasło musi mieć od {2} do 100 znaków.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]).*$",
        ErrorMessage = "Hasło musi zawierać co najmniej 1 małą literę, 1 dużą literę, 1 cyfrę i 1 znak specjalny.")]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Pole Potwierdź hasło jest wymagane.")]
        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Hasło i jego potwierdzenie różnią się.")]
        [StringLength(100)]
        public string ConfirmPassword { get; set; }

        public Address Address { get; set; }
    }
}
