using InvoiceManager.Models.Domains;
using InvoiceManager.Models.Repositories;
using Microsoft.AspNet.Identity;
using Rotativa;
using Rotativa.Options;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace InvoiceManager.Controllers
{
    [Authorize]
    public class PrintController : Controller
    {
        private readonly InvoiceRepository _invoiceRepository = new();
        static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public ActionResult InvoiceToPdf(int id)
        {
            const byte maxRetries = 3;
            byte retryCount = 1;
            var handle = Guid.NewGuid().ToString();
            string userId = User.Identity.GetUserId();
            Invoice invoice = _invoiceRepository.GetInvoice(id, userId);
            string fileName = GetSafeFileName(invoice.Title);

            while (retryCount <= maxRetries)
            {
                try
                {
                    TempData[handle] = GetPdfContent(invoice);
                    return Json(new
                    {
                        FileGuid = handle,
                        FileName = $"{fileName}.pdf"
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error($"{retryCount}. próba generowania PDF nie powiodła się: {ex.Message}");
                    retryCount++;
                }
            }
            return Json(new { Success = false, Message = "Nie udało się wygenerować faktury PDF." });
        }

        private string GetSafeFileName(string fileName) //czyści tytuł faktury ze znaków niedozwolonych w nazwie pliku
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = string.Format(@"[{0}]", invalidChars);

            string safeName = Regex.Replace(fileName, invalidReStr, "_");

            if (safeName.Length > 100)
                safeName = safeName.Substring(0, 100);

            return safeName;
        }



        private byte[] GetPdfContent(Invoice invoice)
        {
            try
            {
                if (invoice == null)
                    throw new ArgumentNullException(nameof(invoice), "Faktura nie może być nullem");

                if (invoice.InvoicePositions == null)
                    throw new ArgumentNullException(nameof(invoice.InvoicePositions), "Pozycje faktury nie mogą być nullem");

                ViewAsPdf pdfResult = new(@"InvoiceTemplate", invoice)
                {
                    PageSize = Size.A4,
                    PageOrientation = Orientation.Portrait,
                    CustomSwitches = "--disable-smart-shrinking --minimum-font-size 8 --zoom 0.85 --no-print-media-type --disable-external-links"
                };
                return pdfResult.BuildFile(ControllerContext);
            }
            catch (Exception ex)
            {
                _logger.Error($"Szczegóły błędu: {ex.Message}");
                _logger.Error($"Wewnętrzny błąd: {ex.InnerException?.Message}");
                _logger.Error($"Ślad stosu: {ex.StackTrace}");
                throw;
            }
        }

        public ActionResult DownloadInvoicePdf(string fileGuid, string fileName)
        {
            if (TempData[fileGuid] == null)
                throw new Exception("Błąd przy próbie eksportu faktury do PDF: TempData[fileGuid] == null");

            byte[] data = TempData[fileGuid] as byte[];
            return File(data, "application/pdf", fileName);
        }

        public ActionResult PrintInvoice(int id) => View("InvoiceTemplate", _invoiceRepository.GetInvoice(id, User.Identity.GetUserId()));
    }
}