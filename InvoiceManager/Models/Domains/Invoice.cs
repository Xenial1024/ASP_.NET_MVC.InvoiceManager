using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InvoiceManager.Attributes;

namespace InvoiceManager.Models.Domains
{
    public class Invoice
    {
        public Invoice() => InvoicePositions = [];

        public int Id { get; set; }

        [Required(ErrorMessage = "Pole Tytuł jest wymagane.")]
        [Display(Name = "Tytuł")]
        public string Title { get; set; }

        [Display(Name = "Klient")]
        [Required(ErrorMessage = "Pole Klient jest wymagane.")]
        public int ClientId { get; set; }

        [Display(Name = "Kwota")]
        public decimal Value { get; set; }

        [Display(Name = "Sposób płatności")]
        [Required(ErrorMessage = "Pole Sposób płatności jest wymagane.")]
        public int MethodOfPaymentId { get; set; }

        [Display(Name = "Termin płatności")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [PaymentDate]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Data utworzenia")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Uwagi")]
        public string Comments { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        public MethodOfPayment MethodOfPayment { get; set; }
        public Client Client { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<InvoicePosition> InvoicePositions { get; set; }
    }
}