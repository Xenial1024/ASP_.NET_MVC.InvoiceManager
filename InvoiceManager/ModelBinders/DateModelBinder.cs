using System;
using System.Globalization;
using System.Web.Mvc;

namespace InvoiceManager.ModelBinders
{
    public class DateModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueResult == null || string.IsNullOrWhiteSpace(valueResult.AttemptedValue))
                return null; 

            string attemptedValue = valueResult.AttemptedValue.Trim();

            if (DateTime.TryParseExact(attemptedValue, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                return parsedDate; 

            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Nieprawidłowy format daty. Użyj formatu dd-MM-yyyy.");
            return null;
        }
    }
}