using InvoiceManager.Models.Domains;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;

namespace InvoiceManager.Models
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            try
            {
                Database.Connection.Open();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoicePosition> InvoicePositions { get; set; }
        public DbSet<MethodOfPayment> MethodsOfPayment { get; set; }
        public DbSet<Product> Products { get; set; }

        public static ApplicationDbContext Create() => new();

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.Invoices)
                .WithRequired(x => x.User)
                .HasForeignKey(x => x.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.Clients)
                .WithRequired(x => x.User)
                .HasForeignKey(x => x.UserId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}