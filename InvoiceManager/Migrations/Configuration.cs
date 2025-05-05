namespace InvoiceManager.Migrations
{
    using InvoiceManager.Models.Domains;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<InvoiceManager.Models.ApplicationDbContext>
    {
        public Configuration() => AutomaticMigrationsEnabled = true;

        protected override void Seed(InvoiceManager.Models.ApplicationDbContext context)
        {
            if (!context.MethodsOfPayment.Any())
            {
                context.MethodsOfPayment.AddOrUpdate(
                    m => m.Name,
                    new MethodOfPayment { Name = "gotówka" },
                    new MethodOfPayment { Name = "przelew" },
                    new MethodOfPayment { Name = "blik" },
                    new MethodOfPayment { Name = "karta" }
                );
            }
        }
    }
}
