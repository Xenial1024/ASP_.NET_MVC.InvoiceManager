using InvoiceManager.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Data.Entity.Validation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace InvoiceManager.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value; 
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.Title = "Zaloguj się"; 
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                model.Email = model.Email.Trim().ToLower();

                // Sprawdź czy użytkownik istnieje
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // Bezpośrednie sprawdzenie hasła
                    bool isPasswordValid = await UserManager.CheckPasswordAsync(user, model.Password);
                    if (isPasswordValid)
                    {
                        await SignInManager.SignInAsync(user, model.RememberMe, false);
                        return RedirectToLocal(returnUrl);
                    }
                }

                ModelState.AddModelError("", "Błędny login lub hasło.");
                return View(model);
            }
            catch (System.Data.SqlClient.SqlException exception)
            {
                ModelState.AddModelError("", "W konfiguracji jest błędny login lub hasło do bazy danych.");
                _logger.Error(exception);
                return View(model);
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", "Wystąpił nieoczekiwany błąd");
                _logger.Error(exception);
                return View(model);
            }
        }



        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register() => View();

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Regex emailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$"),
                        postalCodeRegex = new(@"^[0-9]{2}-[0-9]{3}$");

                    bool isEmailValid = emailRegex.IsMatch(model.Email),
                        isPostalCodeValid = postalCodeRegex.IsMatch(model.Address.PostalCode);

                    if (!isEmailValid)
                        return View(model);

                    if (!isPostalCodeValid)
                        ModelState.AddModelError("Address.PostalCode", "Podany kod pocztowy jest nieprawidłowy.");

                    if (model.Address.HouseOrApartmentNumber == "0")
                        ModelState.AddModelError("Address.HouseOrApartmentNumber", "Numer domu / mieszkania nie może być równy 0." );

                    if (model.Address.City.Length > 60)
                        ModelState.AddModelError("Address.City", "Nazwa miejscowości jest za długa.");

                    if (model.Address.Street.Length > 90)
                        ModelState.AddModelError("Address.Street", "Nazwa ulicy jest za długa." );

                    if (model.Address.HouseOrApartmentNumber.Length > 15)
                        ModelState.AddModelError("Address.HouseOrApartmentNumber", "Numer domu / mieszkania jest zbyt długi.");

                    ApplicationUser user = new()
                    {
                        UserName = model.Name,
                        Name = model.Name,
                        Email = model.Email,
                        Address = model.Address
                    };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                        return RedirectToAction("Index", "Home");
                    }
                    foreach (var error in result.Errors)
                    {
                        if (error.Contains("Email") && error.Contains("is already taken"))
                            ModelState.AddModelError("Email", $"Adres email \"{model.Email}\" jest już używany.");
                        else if (error.Contains("Name") && error.Contains("is already taken"))
                            ModelState.AddModelError("Name", $"Nazwa \"{model.Name}\" jest już używana.");
                        else
                            AddErrors(result);
                    }
                }
                return View(model);
            }
            catch (DbEntityValidationException exception)
            {
                foreach (var entityErrors in exception.EntityValidationErrors)
                {
                    foreach (var validationError in entityErrors.ValidationErrors)
                        _logger.Error($"Właściwość: {validationError.PropertyName} Błąd: {validationError.ErrorMessage}");
                }
                return View("FailedDbConnection");
            }
            catch (Exception exception)
            {
                _logger.Error("Błąd połączenia z bazą danych: " + exception);
                return View("FailedDbConnection");
            }
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return View("Error");

            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }


        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure() => View();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult(string provider, string redirectUri, string userId) : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public string LoginProvider { get; set; } = provider;
            public string RedirectUri { get; set; } = redirectUri;
            public string UserId { get; set; } = userId;

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                    properties.Dictionary[XsrfKey] = UserId;

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}