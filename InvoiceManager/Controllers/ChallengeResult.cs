using Microsoft.Owin.Security;
using System.Web;
using System.Web.Mvc;


namespace InvoiceManager.Controllers
{
    public partial class AccountController
    {
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
    }
}