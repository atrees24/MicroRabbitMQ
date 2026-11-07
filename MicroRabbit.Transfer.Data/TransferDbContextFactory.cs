using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MicroRabbit.Transfer.Data.Context
{
    public class TransferDbContextFactory : IDesignTimeDbContextFactory<TransferDbContext>
    {
        public TransferDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TransferDbContext>();
            optionsBuilder.UseSqlServer(
                "Server=.;Database=TransferDB;Trusted_Connection=True;TrustServerCertificate=True;");

            return new TransferDbContext(optionsBuilder.Options);
        }
    }
}
