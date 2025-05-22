using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Attributes
{
    public class NotZeroAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null && value.ToString() == "0")
                return new ValidationResult("Numer domu nie może być równy 0.");

            return ValidationResult.Success;
        }
    }
}