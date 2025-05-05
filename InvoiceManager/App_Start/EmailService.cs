using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace InvoiceManager
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message) => Task.FromResult(0); // Plug in your email service here to send an email.
    }
}
