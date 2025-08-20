using BankingApi.EventReceiver.Domain.Models;

namespace BankingApi.EventReceiver.Application.Repositories;

public interface IAccountRepository
{
  Task<Account> GetByIdAsync(Guid accountId, CancellationToken cancellationToken);
  Task<Account> UpdateAsync(Account account, CancellationToken cancellationToken);
}
