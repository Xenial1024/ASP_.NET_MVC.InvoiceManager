using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace InvoiceManager
{
    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message) => Task.FromResult(0); // Plug in your SMS service here to send a text message.
    }
}
