using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Attributes
{
    public class PaymentDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var invoice = (Models.Domains.Invoice)validationContext.ObjectInstance;

            if (invoice.PaymentDate.Date < invoice.CreatedDate.Date)
                return new ValidationResult("Termin płatności nie może być wcześniejszy niż data sprzedaży.", ["Invoice.PaymentDate"]);

            return ValidationResult.Success;
        }
    }
}