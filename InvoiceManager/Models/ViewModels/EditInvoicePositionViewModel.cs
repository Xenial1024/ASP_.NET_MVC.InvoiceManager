using InvoiceManager.Models.Domains;
using System.Collections.Generic;

namespace InvoiceManager.Models
{
    public class EditInvoicePositionViewModel
    {
        public InvoicePosition InvoicePosition { get; set; }
        public string Heading { get; set; }
        public List<Product> Products { get; set; }
    }
}