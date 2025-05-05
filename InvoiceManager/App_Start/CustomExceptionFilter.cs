using System;
using System.Web.Mvc;

namespace InvoiceManager.App_Start
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public void OnException(ExceptionContext filterContext)
        {
            Exception exception = filterContext.Exception;
            _logger.Error(exception);
        }
    }
}