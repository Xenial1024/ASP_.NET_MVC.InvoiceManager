using InvoiceManager.Models.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System;

namespace InvoiceManager.Models.Repositories
{
    public class InvoiceRepository
    {
        static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public List<Invoice> GetInvoices(string userId)
        {
            using ApplicationDbContext context = new();
            return [.. context.Invoices
                .Include(x => x.Client)
                .Where(x => x.UserId == userId)];
        }

        public Invoice GetInvoice(int id, string userId)
        {
            using ApplicationDbContext context = new();
            return context.Invoices
                .Include(x => x.InvoicePositions)
                .Include(x => x.InvoicePositions.Select(y => y.Product))
                .Include(x => x.MethodOfPayment)
                .Include(x => x.User)
                .Include(x => x.User.Address)
                .Include(x => x.Client)
                .Include(x => x.Client.Address)
                .FirstOrDefault(x => x.Id == id && x.UserId == userId);
        }

        public List<MethodOfPayment> GetMethodsOfPayment()
        {
            using ApplicationDbContext context = new();
            return [.. context.MethodsOfPayment];
        }

        public InvoicePosition GetInvoicePosition(
            int invoicePositionId, string userId)
        {
            using ApplicationDbContext context = new();
            return context.InvoicePositions
        .Include("Invoice")
        .Include("Product")
        .FirstOrDefault(x => x.Id == invoicePositionId && x.Invoice.UserId == userId);
        }

        public int Add(Invoice invoice)
        {
            using ApplicationDbContext context = new();
            invoice.CreatedDate = DateTime.Now;
            context.Invoices.Add(invoice);
            context.SaveChanges();

            return invoice.Id;

        }

        public void Update(Invoice invoice)
        {
            using ApplicationDbContext context = new();
            var invoiceToUpdate = context.Invoices
                .Single(x => x.Id == invoice.Id && x.UserId == invoice.UserId);

            invoiceToUpdate.ClientId = invoice.ClientId;
            invoiceToUpdate.Comments = invoice.Comments;
            invoiceToUpdate.MethodOfPaymentId = invoice.MethodOfPaymentId;
            invoiceToUpdate.PaymentDate = invoice.PaymentDate;
            invoiceToUpdate.Title = invoice.Title;

            context.SaveChanges();
        }

        public void AddPosition(InvoicePosition invoicePosition, string userId, ApplicationDbContext context)
        {
            try
            {
                var invoice = context.Invoices
                    .FirstOrDefault(x =>
                        x.Id == invoicePosition.InvoiceId &&
                        x.UserId == userId);

                if (invoice == null)
                {
                    _logger.Error($"Nie znaleziono faktury: ID={invoicePosition.InvoiceId}, UserID={userId}");
                    throw new Exception($"Nie znaleziono faktury o ID {invoicePosition.InvoiceId} dla bieżącego użytkownika");
                }

                if (invoicePosition.Lp == 0)
                {
                    int maxLp = 0;
                    if (invoice.InvoicePositions != null && invoice.InvoicePositions.Any())
                        maxLp = invoice.InvoicePositions.Max(p => p.Lp);
                    invoicePosition.Lp = maxLp + 1;
                }

                if (invoicePosition.ProductId <= 0)
                    throw new Exception("Nie wybrano produktu");

                if (invoicePosition.Quantity <= 0)
                    throw new Exception("Ilość musi być większa od zera");

                context.InvoicePositions.Add(invoicePosition);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error($"Wyjątek w AddPosition: {ex.Message}");
                _logger.Error($"Stos: {ex.StackTrace}");
                if (ex.InnerException != null)
                    _logger.Error($"Inner exception: {ex.InnerException.Message}");
                throw;
            }
        }

        public void UpdatePosition(InvoicePosition position, string userId)
        {
            using ApplicationDbContext context = new();

            var positionToUpdate = context.InvoicePositions
                .Include("Invoice")
                .FirstOrDefault(x => x.Id == position.Id && x.Invoice.UserId == userId) ?? throw new Exception("Nie znaleziono pozycji faktury.");

            positionToUpdate.ProductId = position.ProductId;
            positionToUpdate.Quantity = position.Quantity;
            positionToUpdate.Value = position.Value;

            var product = context.Products.FirstOrDefault(p => p.Id == position.ProductId);
            if (product != null)
                positionToUpdate.Product = product;

            context.SaveChanges();
        }

        public decimal UpdateInvoiceValue(int invoiceId, string userId, ApplicationDbContext context = null)
        {
            bool externalContext = context != null;
            context ??= new ApplicationDbContext();
            try
            {
                Invoice invoice = context.Invoices.FirstOrDefault(x => x.Id == invoiceId && x.UserId == userId)
                    ?? throw new Exception("Faktura nie została znaleziona.");

                var positions = context.InvoicePositions.Where(x => x.InvoiceId == invoiceId).ToList();
                decimal totalValue = positions != null && positions.Any() ? positions.Sum(x => x.Value) : 0;

                if (totalValue > 9999999999999999.99m)
                    throw new Exception(null, new ArgumentException("Value is out of range."));

                invoice.Value = totalValue;

                if (!externalContext)
                    context.SaveChanges();

                return invoice.Value;
            }
            finally
            {
                if (!externalContext)
                    context.Dispose();
            }
        }

        public void Delete(int id, string userId)
        {
            using ApplicationDbContext context = new();
            var invoiceToDelete = context.Invoices
                .Single(x => x.Id == id && x.UserId == userId);
            context.Invoices.Remove(invoiceToDelete);
            context.SaveChanges();
        }

        public void DeletePosition(int id, string userId)
        {
            using ApplicationDbContext context = new();
            var positionToDelete = context.InvoicePositions
                .Include(x => x.Invoice)
                .Single(x => x.Id == id && x.Invoice.UserId == userId);
            context.InvoicePositions.Remove(positionToDelete);
            context.SaveChanges();
        }
    }
}