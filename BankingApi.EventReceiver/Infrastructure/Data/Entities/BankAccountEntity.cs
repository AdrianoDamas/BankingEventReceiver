using BankingApi.EventReceiver.Domain.Models;

namespace BankingApi.EventReceiver.Infrastructure.Data.Entities
{
  public class BankAccountEntity
  {
    public Guid Id { get; set; }
    public decimal Balance { get; set; }
    public required byte[] RowVersion { get; set; }

    public Account ToModel()
    {
      return new Account(Id, Balance, RowVersion);
    }

    public void UpdateFromModel(Account account)
    {
      ArgumentNullException.ThrowIfNull(account);

      Balance = account.Balance;
    }

    public static BankAccountEntity FromModel(Account account)
    {
      ArgumentNullException.ThrowIfNull(account);

      return new BankAccountEntity
      {
        Id = account.Id,
        Balance = account.Balance,
        RowVersion = [.. account.Version]
      };
    }
  }
}
