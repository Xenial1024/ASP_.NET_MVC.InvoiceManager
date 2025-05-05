using InvoiceManager.Models.Domains;
using System.Collections.Generic;

namespace InvoiceManager.Models
{
    public class EditInvoiceViewModel
    {
        public Invoice Invoice { get; set; }
        public List<Client> Clients { get; set; }
        public List<MethodOfPayment> MethodsOfPayment { get; set; }
        public string Heading { get; set; }
    }
}