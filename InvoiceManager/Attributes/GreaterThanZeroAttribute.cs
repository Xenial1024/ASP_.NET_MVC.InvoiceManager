using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Attributes
{
    public class GreaterThanZeroAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is decimal decimalValue && decimalValue > 0)
                return ValidationResult.Success;

            return new ValidationResult(ErrorMessage);
        }
    }
}