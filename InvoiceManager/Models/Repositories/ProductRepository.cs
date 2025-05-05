using InvoiceManager.Models.Domains;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceManager.Models.Repositories
{
    public class ProductRepository
    {
        public List<Product> GetProducts()
        {
            using ApplicationDbContext context = new();
            return [.. context.Products];
        }

        public Product GetProduct(int productId)
        {
            using ApplicationDbContext context = new();
            return context.Products.FirstOrDefault(x => x.Id == productId);
        }
    }
}