using System.Globalization;
using System.Web.Mvc;

namespace InvoiceManager.ModelBinders
{
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueResult == null || string.IsNullOrWhiteSpace(valueResult.AttemptedValue))
                return null;

            string attemptedValue = valueResult.AttemptedValue.Replace(".", ",");

            if (decimal.TryParse(attemptedValue, NumberStyles.Number, CultureInfo.GetCultureInfo("pl-PL"), out decimal result))
                return result;

            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Nieprawidłowy format ceny. Użyj cyfr i przecinka.");
            return null;
        }
    }
}