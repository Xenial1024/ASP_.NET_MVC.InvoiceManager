using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Models.Validation
{
    public class NotEqualToPropertyAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public NotEqualToPropertyAttribute(string comparisonProperty) =>_comparisonProperty = comparisonProperty;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            object instance = validationContext.ObjectInstance;
            System.Reflection.PropertyInfo propertyInfo = instance.GetType().GetProperty(_comparisonProperty);

            if (propertyInfo == null)
                return new ValidationResult($"Nieznana właściwość: {_comparisonProperty}");

            object propertyValue = propertyInfo.GetValue(instance);

            if (Equals(value, propertyValue))
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}