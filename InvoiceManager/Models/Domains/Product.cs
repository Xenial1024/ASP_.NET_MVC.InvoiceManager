using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Models.Domains
{
    public class Product //Zakładamy, że produkty mają być wspólne dla wszystkich użytkowników (np. aplikacja jest używana w obrębie danej firmy).
    {
        public Product() => InvoicePositions = [];

        public int Id { get; set; }

        [Required]
        [StringLength(128, ErrorMessage = "Nazwa jest zbyt długa.")]
        public string Name { get; set; }

        public decimal Value { get; set; }

        public ICollection<InvoicePosition> InvoicePositions { get; set; }
    }
}