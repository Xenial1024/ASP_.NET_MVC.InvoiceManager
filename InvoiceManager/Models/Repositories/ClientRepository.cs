using InvoiceManager.Models.Domains;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceManager.Models.Repositories
{
    public class ClientRepository
    {
        public List<Client> GetClients(string userId)
        {
            using ApplicationDbContext context = new();
            return [.. context.Clients.Where(x => x.UserId == userId)];
        }
    }
}