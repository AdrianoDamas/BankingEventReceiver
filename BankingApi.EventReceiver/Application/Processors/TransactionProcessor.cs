using BankingApi.EventReceiver.Application.Repositories;
using BankingApi.EventReceiver.Domain.ValueObjects;

namespace BankingApi.EventReceiver.Application.Processors;

public class TransactionProcessor : ITransactionProcessor
{
  private readonly IAccountRepository _accountRepository;

  public TransactionProcessor(IAccountRepository accountRepository)
  {
    _accountRepository = accountRepository;
  }

  public async Task ProcessAsync(Transaction transaction, CancellationToken cancellationToken)
  {
    // Fetch the account
    var account = await _accountRepository.GetByIdAsync(transaction.AccountId, cancellationToken);

    // Apply the transaction to the account
    account.ApplyTransaction(transaction);

    // Update the account
    await _accountRepository.UpdateAsync(account, cancellationToken);
  }
}
