using InvoiceManager.Models;
using InvoiceManager.Models.Domains;
using InvoiceManager.Models.Repositories;
using Microsoft.AspNet.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Caching;
using System.Web.Mvc;

namespace InvoiceManager.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        readonly InvoiceRepository _invoiceRepository = new();
        readonly ClientRepository _clientRepository = new();
        readonly ProductRepository _productRepository = new();
        static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        readonly ApplicationDbContext _db = new();


        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            var invoices = _invoiceRepository.GetInvoices(userId);
            return View(invoices);
        }

        public ActionResult Invoice(int id = 0)
        {
            string userId = User.Identity.GetUserId();
            var invoice = id == 0 ?
                GetNewInvoice(userId) :
                _invoiceRepository.GetInvoice(id, userId);
            var vm = PrepareInvoiceVm(invoice, userId);
            return View(vm);
        }

        private EditInvoiceViewModel PrepareInvoiceVm(Invoice invoice, string userId)
        {
            return new EditInvoiceViewModel
            {
                Invoice = invoice,
                Heading = invoice.Id == 0 ? "Dodawanie nowej faktury" : "Edycja faktury",
                Clients = _clientRepository.GetClients(userId),
                MethodsOfPayment = _invoiceRepository.GetMethodsOfPayment()
            };
        }

        private Invoice GetNewInvoice(string userId)
        {
            return new Invoice
            {
                UserId = userId,
                CreatedDate = DateTime.Now,
                PaymentDate = DateTime.Now.AddDays(7)
            };
        }
        [HttpGet]
        public ActionResult InvoicePosition(int invoiceId, int invoicePositionId = 0)
        {
            if (invoiceId <= 0)
                return RedirectToAction("Error", new { message = "Nieprawidłowy identyfikator faktury" });

            var rawQuantity = Request.Form["InvoicePosition.Quantity"];
            if (!string.IsNullOrEmpty(rawQuantity) && !int.TryParse(rawQuantity, out _))
            {
                ModelState.Remove("InvoicePosition.Quantity");
                ModelState.AddModelError("InvoicePosition.Quantity", "Ilość musi być liczbą całkowitą większą od 0.");
            }

            string userId = User.Identity.GetUserId();
            var invoicePosition = invoicePositionId == 0 ?
                GetNewPosition(invoiceId, invoicePositionId) :
                _invoiceRepository.GetInvoicePosition(invoicePositionId, userId);

            var vm = PrepareInvoicePositionVm(invoicePosition);
            return View(vm);
        }

        private EditInvoicePositionViewModel PrepareInvoicePositionVm(InvoicePosition invoicePosition)
        {
            return new EditInvoicePositionViewModel
            {
                InvoicePosition = invoicePosition,
                Heading = invoicePosition.Id == 0 ?
                "Dodawanie nowej pozycji" : "Edytowanie Pozycji",
                Products = _productRepository.GetProducts()
            };
        }

        private InvoicePosition GetNewPosition(int invoiceId, int invoicePositionId)
        {
            int maxLp = 0;
            var invoice = _invoiceRepository.GetInvoice(invoiceId, User.Identity.GetUserId());
            if (invoice.InvoicePositions != null && invoice.InvoicePositions.Any())
                maxLp = invoice.InvoicePositions.Max(p => p.Lp);

            return new InvoicePosition
            {
                InvoiceId = invoiceId,
                Id = invoicePositionId,
                Lp = maxLp + 1
            };
        }

        public ActionResult GetProductPrice(int productId)
        {
            Product product = _productRepository.GetProduct(productId);
            if (product == null)
                return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
            return Json(new { Success = true, Price = product.Value }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Invoice(Invoice invoice)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                invoice.UserId = userId;

                if (invoice.Id == 0)
                    invoice.CreatedDate = DateTime.Now;
                else
                {
                    var existingInvoice = _invoiceRepository.GetInvoice(invoice.Id, userId);
                    invoice.CreatedDate = existingInvoice.CreatedDate;
                }

                if (!ModelState.IsValid)
                {
                    var vm = PrepareInvoiceVm(invoice, userId);
                    return View("Invoice", vm);
                }

                if (invoice.Id == 0)
                    _invoiceRepository.Add(invoice);
                else
                {
                    var existingInvoice = _invoiceRepository.GetInvoice(invoice.Id, userId);
                    invoice.InvoicePositions = existingInvoice.InvoicePositions; 
                    _invoiceRepository.Update(invoice);
                }
                return RedirectToAction("Invoice", new { id = invoice.Id });
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationError in ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors))
                    ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);

                string userId = User.Identity.GetUserId();
                var vm = PrepareInvoiceVm(invoice, userId);
                return View("Invoice", vm);
            }
            catch (Exception ex)
            {
                _logger.Error($"Błąd podczas zapisywania faktury: {ex.Message}");
                ModelState.AddModelError("", $"Wystąpił błąd: {ex.Message}");

                string userId = User.Identity.GetUserId();
                var vm = PrepareInvoiceVm(invoice, userId);
                return View("Invoice", vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveInvoicePosition(InvoicePosition invoicePosition)
        {
            using ApplicationDbContext context = new();
            try
            {
                string userId = User.Identity.GetUserId();

                if (invoicePosition.InvoiceId <= 0)
                {
                    ModelState.AddModelError("", "Nieprawidłowy identyfikator faktury");
                    return ReturnToInvoicePosition(invoicePosition);
                }

                if (ModelState["InvoicePosition.Quantity"]?.Errors.Count > 0)
                {
                    ModelState.Remove("InvoicePosition.Quantity");
                    ModelState.AddModelError("InvoicePosition.Quantity", "Ilość musi być liczbą całkowitą większą od 0.");
                }

                Product product = _productRepository.GetProduct(invoicePosition.ProductId);

                if (product == null)
                {
                    ModelState.AddModelError("InvoicePosition.ProductId", "Wybrany produkt nie istnieje");
                    return ReturnToInvoicePosition(invoicePosition);
                }

                invoicePosition.Value = invoicePosition.Quantity * product.Value;

                ModelState.Remove("InvoicePosition.Value");
                ModelState.Remove("Value");

                if (!ModelState.IsValid)
                {
                    _logger.Error($"ModelState errors: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                    return ReturnToInvoicePosition(invoicePosition);
                }

                Invoice invoice;
                try
                {
                    invoice = _invoiceRepository.GetInvoice(invoicePosition.InvoiceId, userId);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Nie można pobrać faktury: {ex.Message}");
                    ModelState.AddModelError("", "Nie znaleziono faktury.");
                    return ReturnToInvoicePosition(invoicePosition);
                }

                decimal currentTotalValue = invoice.InvoicePositions?.Sum(p => p.Id != invoicePosition.Id ? p.Value : 0) ?? 0;
                decimal newTotalValue = currentTotalValue + invoicePosition.Value;

                if (newTotalValue > 9999999999999999.99m)
                {
                    ModelState.AddModelError("", "Suma cen wszystkich pozycji faktury jest zbyt duża. Maksymalna dozwolona wartość to 9999999999999999.99.");
                    return ReturnToInvoicePosition(invoicePosition);
                }

                if (invoicePosition.Id == 0) // Dodawanie nowej pozycji
                {
                    var existingPosition = invoice.InvoicePositions?.FirstOrDefault(p => p.ProductId == invoicePosition.ProductId);

                    if (existingPosition != null)
                    {
                        int updatedQuantity = existingPosition.Quantity + invoicePosition.Quantity;
                        decimal updatedValue = updatedQuantity * product.Value;
                        decimal updatedTotalValue = currentTotalValue - existingPosition.Value + updatedValue;
                        if (updatedTotalValue > 9999999999999999.99m)
                        {
                            ModelState.AddModelError("", "Suma cen wszystkich pozycji faktury jest zbyt duża. Maksymalna dozwolona wartość to 9999999999999999.99.");
                            return ReturnToInvoicePosition(invoicePosition);
                        }
                        existingPosition.Quantity = updatedQuantity;
                        existingPosition.Value = updatedValue;

                        _invoiceRepository.UpdatePosition(existingPosition, userId);
                    }
                    else
                    {
                        int maxLp = invoice.InvoicePositions?.Any() == true
                            ? invoice.InvoicePositions.Max(p => p.Lp)
                            : 0;
                        invoicePosition.Lp = maxLp + 1;

                        _invoiceRepository.AddPosition(invoicePosition, userId, context);
                    }
                }
                else // Edycja istniejącej pozycji
                {
                    var originalPosition = _invoiceRepository.GetInvoicePosition(invoicePosition.Id, userId);

                    if (originalPosition == null)
                    {
                        ModelState.AddModelError("", "Nie znaleziono pozycji faktury.");
                        return ReturnToInvoicePosition(invoicePosition);
                    }

                    decimal updatedTotalValue = currentTotalValue - originalPosition.Value + invoicePosition.Value;
                    if (updatedTotalValue > 9999999999999999.99m)
                    {
                        ModelState.AddModelError("", "Suma cen wszystkich pozycji faktury jest zbyt duża. Maksymalna dozwolona wartość to 9999999999999999.99.");
                        return ReturnToInvoicePosition(invoicePosition);
                    }

                    originalPosition.ProductId = invoicePosition.ProductId;
                    originalPosition.Quantity = invoicePosition.Quantity;
                    originalPosition.Value = invoicePosition.Value;

                    _invoiceRepository.UpdatePosition(originalPosition, userId);
                }

                _invoiceRepository.UpdateInvoiceValue(invoicePosition.InvoiceId, userId, context);
                context.SaveChanges();

                return RedirectToAction("Invoice", new { id = invoicePosition.InvoiceId });
            }
            catch (Exception exception)
            {
                _logger.Error($"Błąd podczas dodawania pozycji faktury: {exception.Message}\n   {exception.StackTrace}");

                for (Exception ex = exception; ex != null; ex = ex.InnerException)
                    _logger.Error($"→ {ex.GetType().Name}: {ex.Message}");

                Exception rootCause = exception;
                while (rootCause.InnerException != null)
                    rootCause = rootCause.InnerException;

                ModelState.AddModelError("",
                    rootCause is ArgumentException && rootCause.Message.Contains("out of range")
                        ? "Suma cen wszystkich pozycji faktury jest zbyt duża. Maksymalna dozwolona wartość to 9999999999999999.99."
                        : "Wystąpił błąd podczas dodawania pozycji faktury.");

                return ReturnToInvoicePosition(invoicePosition);
            }
        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                _invoiceRepository.Delete(id, userId);
            }
            catch (Exception exception)
            {
                var innerException = exception.InnerException?.Message ?? "No inner exception";
                return Json(new { Success = false, exception.Message, InnerDetails = innerException });
            }

            return Json(new { Success = true });
        }

        [HttpPost]
        public JsonResult DeletePosition(int id, int invoiceId)
        {
            using ApplicationDbContext context = new();
            try
            {
                string userId = User.Identity.GetUserId();
                _invoiceRepository.DeletePosition(id, userId);
                decimal updatedValue = _invoiceRepository.UpdateInvoiceValue(invoiceId, userId, context);
                return Json(new
                {
                    Success = true,
                    InvoiceValue = updatedValue
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Błąd podczas usuwania pozycji faktury: {ex.Message}");
                return Json(new { Success = false, ex.Message });
            }

        }

        [HttpPost]
        public ActionResult AddClient(Client client)
        {
            try
            {
                string currentUserId = User.Identity.GetUserId();
                client.UserId = User.Identity.GetUserId();

                if (client.Address == null)
                {
                    client.Address = new Address();
                    _logger.Error("Addres klienta był nullem, utworzono nowy obiekt");
                }

                if (string.IsNullOrWhiteSpace(client.Name))
                    return Json(new { Success = false, Message = "Nazwa klienta jest wymagana." });

                if (client.Name.Length > 128)
                    return Json(new { Success = false, Message = "Nazwa jest zbyt długa (maksymalnie 128 znaków)." });

                if (string.IsNullOrWhiteSpace(client.Email))
                    return Json(new { Success = false, Message = "Email jest wymagany." });

                if (client.Email.Length > 254)
                    return Json(new { Success = false, Message = "Adres email jest zbyt długi (maksymalnie 254 znaki)." });

                Regex emailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
                if (!emailRegex.IsMatch(client.Email))
                    return Json(new { Success = false, Message = "Podany adres email jest nieprawidłowy." });

                if (string.IsNullOrWhiteSpace(client.Address.HouseOrApartmentNumber))
                    return Json(new { Success = false, Message = "Numer domu / mieszkania jest wymagany." });

                if (client.Address.HouseOrApartmentNumber == "0")
                    return Json(new { Success = false, Message = "Numer domu / mieszkania nie może być równy 0." });

                if (client.Address.HouseOrApartmentNumber.Length > 15)
                    return Json(new { Success = false, Message = "Numer domu / mieszkania jest zbyt długi." });

                if (string.IsNullOrWhiteSpace(client.Address.City))
                    return Json(new { Success = false, Message = "Nazwa miejscowości jest wymagana." });

                if (client.Address.City.Length > 60)
                    return Json(new { Success = false, Message = "Nazwa miejscowości jest za długa." });

                if (string.IsNullOrWhiteSpace(client.Address.Street))
                    return Json(new { Success = false, Message = "Nazwa ulicy jest wymagana." });

                if (client.Address.Street.Length > 90)
                    return Json(new { Success = false, Message = "Nazwa ulicy jest za długa." });

                if (string.IsNullOrWhiteSpace(client.Address.PostalCode))
                    return Json(new { Success = false, Message = "Kod pocztowy jest wymagany." });

                Regex postalCodeRegex = new(@"^[0-9]{2}-[0-9]{3}$");
                if (!postalCodeRegex.IsMatch(client.Address.PostalCode))
                    return Json(new { Success = false, Message = "Kod pocztowy jest nieprawidłowy." });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { Success = false, Message = "Błędy walidacji: " + string.Join(", ", errors) });
                }
                Client clientWithExistingEmail = _db.Clients.FirstOrDefault(c => c.Email == client.Email && c.UserId == currentUserId);
                if (clientWithExistingEmail != null)
                    return Json(new { Success = false, Message = "Klient o tym adresie email już istnieje na Twojej liście." });

                Client clientWithExistingName = _db.Clients.FirstOrDefault(c => c.Name == client.Name && c.UserId == currentUserId);
                if (clientWithExistingName != null)
                    return Json(new { Success = false, Message = "Klient o tej nazwie już istnieje na Twojej liście." });

                _db.Clients.Add(client);

                try
                {
                    _db.SaveChanges();
                    return Json(new { Success = true, ClientId = client.Id });
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    _logger.Error($"Błąd walidacji: {dbEx.Message}"); 
                    IEnumerable validationErrors = dbEx.EntityValidationErrors
                        .SelectMany(eve => eve.ValidationErrors)
                        .Select(ve => ve.PropertyName + ": " + ve.ErrorMessage);

                    return Json(new
                    {
                        Success = false,
                        Message = "Błędy walidacji: " + string.Join(", ", validationErrors)
                    });
                }
            }
            catch (Exception ex)
            {
                List<string> exceptionDetails = [];
                Exception currentException = ex;

                while (currentException != null)
                {
                    exceptionDetails.Add(currentException.Message);
                    currentException = currentException.InnerException;
                }

                return Json(new { Success = false, Message = string.Join(" -> ", exceptionDetails) });
            }
        }

        [HttpPost]
        public ActionResult AddProduct(Product product)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(product.Name))
                    return Json(new 
                    { 
                        Success = false, 
                        Message = "Nazwa produktu jest wymagana." 
                    }); 
                
                if (product.Name.Length > 128)
                    return Json(new 
                    { 
                        Success = false, 
                        Message = "Nazwa jest zbyt długa (maksymalnie 128 znaków)." 
                    });

                if (product.Value <= 0)
                    return Json(new 
                    { 
                        Success = false, 
                        Message = "Cena musi być dodatnią liczbą nie większą niż 9999999999999999.99."
                    });

                if (product.Value > 9999999999999999.99m) // Maksymalna wartość dla decimal(18,2)
                    return Json(new 
                    { 
                        Success = false, 
                        Message = "Cena wykracza poza dopuszczalny zakres." 
                    });

                if (!Regex.IsMatch(product.Value.ToString(), @"^[0-9]+(?:[.,][0-9]{0,2})?$"))
                    return Json(new
                    {
                        Success = false,
                        Message = "Nieprawidłowy format ceny. Użyj cyfr i opcjonalnie przecinka lub kropki jako separatora."
                    });

                _db.Products.Add(product);
                _db.SaveChanges();
                return Json(new { Success = true, ProductId = product.Id });

            }
            catch (Exception ex)
            {
                _logger.Error($"Błąd podczas dodawania produktu: {ex.Message}"); 
                
                List<string> exceptionDetails = [];
                Exception currentException = ex;

                while (currentException != null)
                {
                    exceptionDetails.Add(currentException.Message);
                    currentException = currentException.InnerException;
                }

                return Json(new { Success = false, Message = string.Join(" -> ", exceptionDetails) });
            }
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            AboutViewModel vm = new();
            return View(vm);
        }

        [AllowAnonymous]
        public ActionResult Contact()
        {
            ContactViewModel vm = new();
            return View(vm);
        }

        [AllowAnonymous]
        public ActionResult PrivacyPolicy()
        {
            PrivacyPolicyViewModel vm = new();
            return View(vm);
        }

        private ActionResult ReturnToInvoicePosition(InvoicePosition invoicePosition)
        {
            var vm = PrepareInvoicePositionVm(invoicePosition);
            return View("InvoicePosition", vm);
        }

        [AllowAnonymous]
        public ActionResult ThrowException() => throw new Exception("Kod błędu 123");
    }
}