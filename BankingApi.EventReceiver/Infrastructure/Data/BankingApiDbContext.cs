using BankingApi.EventReceiver.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankingApi.EventReceiver.Infrastructure.Data
{
  public class BankingApiDbContext : DbContext
  {
    public DbSet<BankAccountEntity> BankAccounts { get; set; }

    public DbSet<TransactionEntity> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=BankingApiTest;Integrated Security=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfigurationsFromAssembly(typeof(BankingApiDbContext).Assembly);
    }

  }
}
