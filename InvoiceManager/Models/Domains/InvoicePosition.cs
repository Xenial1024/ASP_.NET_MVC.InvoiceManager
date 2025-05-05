using System.ComponentModel.DataAnnotations;
using InvoiceManager.Attributes;

namespace InvoiceManager.Models.Domains
{
    public class InvoicePosition
    {
        public int Id { get; set; }
        public int Lp { get; set; }
        public int InvoiceId { get; set; }

        [Display(Name = "Cena produktu")]
        [Required(ErrorMessage = "Pole Cena produktu jest wymagane.")]
        [GreaterThanZero(ErrorMessage = "Cena produktu musi być większa od zera.")]
        [RegularExpression(@"^[0-9]+(?:[.,][0-9]{0,2})?$", ErrorMessage = "Nieprawidłowy format kwoty. Użyj cyfr i opcjonalnie przecinka lub kropki jako separatora.")]
        public decimal Value { get; set; }

        [Display(Name = "Produkt")]
        [Required(ErrorMessage = "Pole Produkt jest wymagane.")]
        public int ProductId { get; set; }

        [Display(Name = "Ilość")]
        [Required(ErrorMessage = "Pole Ilość jest wymagane.")]
        [RegularExpression(@"^[1-9][0-9]*$", ErrorMessage = "Ilość musi być liczbą całkowitą większą od 0.")]
        public int Quantity { get; set; } = 1;

        public Invoice Invoice { get; set; }
        public Product Product { get; set; }
    }
}