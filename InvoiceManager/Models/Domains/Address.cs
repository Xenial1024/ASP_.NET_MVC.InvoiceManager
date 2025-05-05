using InvoiceManager.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceManager.Models.Domains
{
    public class Address
    {
        public Address()
        {
            Clients = [];
            Users = [];
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Pole Ulica jest wymagane.")]
        [StringLength(90, ErrorMessage = "Nazwa ulicy jest za długa.")]
        [Display(Name = "Ulica")]
        public string Street { get; set; }

        [StringLength(8)]
        public string StreetType {  get; set; }

        [Required(ErrorMessage = "Pole Numer jest wymagane.")]
        [StringLength(15, ErrorMessage = "Numer domu / mieszkania jest zbyt długi.")]
        [NotZero]
        [Display(Name = "Numer")]
        public string HouseOrApartmentNumber { get; set; }

        [Required(ErrorMessage = "Pole Miejscowość jest wymagane.")]
        [StringLength(60, ErrorMessage = "Nazwa miejscowości jest za długa.")]
        [Display(Name = "Miejscowość")]
        public string City { get; set; }

        [Required(ErrorMessage = "Pole Kod pocztowy jest wymagane.")]
        [RegularExpression(@"^[0-9]{2}-[0-9]{3}$", ErrorMessage = "Podany kod pocztowy jest nieprawidłowy.")]
        [Display(Name = "Kod pocztowy")]
        [Column(TypeName = "varchar")]
        [StringLength(6)]
        public string PostalCode { get; set; }

        public ICollection<Client> Clients { get; set; }
        public ICollection<ApplicationUser> Users { get; set; }
    }
}