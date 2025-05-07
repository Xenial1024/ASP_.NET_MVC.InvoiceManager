using System.Web.Mvc;

namespace InvoiceManager.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }
    }

}