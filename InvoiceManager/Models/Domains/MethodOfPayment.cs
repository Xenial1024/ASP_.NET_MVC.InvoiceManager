using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceManager.Models.Domains
{
    [Table("MethodsOfPayment")]
    public class MethodOfPayment
    {
        public MethodOfPayment() => Invoices = [];

        public int Id { get; set; }

        [Required]
        [StringLength(7)]
        public string Name { get; set; }


        public ICollection<Invoice> Invoices { get; set; }
    }
}