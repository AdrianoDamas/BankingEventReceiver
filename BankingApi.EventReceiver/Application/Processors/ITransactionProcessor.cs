using BankingApi.EventReceiver.Domain.ValueObjects;

namespace BankingApi.EventReceiver.Application.Processors;

public interface ITransactionProcessor
{
  Task ProcessAsync(Transaction transaction, CancellationToken cancellationToken);
}
