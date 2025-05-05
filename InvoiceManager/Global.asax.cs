using System;
using System.Data.Entity;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using InvoiceManager.ModelBinders;

namespace InvoiceManager
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Database.SetInitializer<Models.ApplicationDbContext>(null);
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(DateTime), new DateModelBinder());
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            CultureInfo newCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            newCulture.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
            newCulture.DateTimeFormat.DateSeparator = "-";
            newCulture.NumberFormat.NumberDecimalDigits = 2;
            newCulture.NumberFormat.NumberDecimalSeparator = ",";
            newCulture.NumberFormat.NumberGroupSeparator = "";
            Thread.CurrentThread.CurrentCulture = newCulture;
        }
    }
}
