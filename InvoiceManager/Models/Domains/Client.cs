using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceManager.Models.Domains
{
    public class Client
    {
        public Client() => Invoices = [];

        public int Id { get; set; }

        [Required]
        [StringLength(128, ErrorMessage = "Nazwa jest zbyt długa.")]
        public string Name { get; set; }
        public int? AddressId { get; set; }

        [Required]
        [StringLength(254, ErrorMessage = "Adres email jest zbyt długi.")]
        public string Email { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        public Address Address { get; set; }
        public ICollection<Invoice> Invoices { get; set; }
        public ApplicationUser User { get; set; }
    }
}