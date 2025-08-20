using BankingApi.EventReceiver.Domain.Enums;
using BankingApi.EventReceiver.Domain.ValueObjects;

namespace BankingApi.EventReceiver.Infrastructure.Data.Entities;

public class TransactionEntity
{
  public Guid Id { get; set; }
  public Guid AccountId { get; set; }
  public decimal Amount { get; set; }
  public TransactionType Type { get; set; }
  public TransactionDirection Direction { get; set; }
  public DateTimeOffset Timestamp { get; set; }

  public Transaction ToModel()
  {
    return new Transaction(
      id: Id,
      accountId: AccountId,
      amount: Amount,
      type: Type,
      direction: Direction,
      timestamp: Timestamp
    );
  }

  public static TransactionEntity FromModel(Transaction transaction)
  {
    ArgumentNullException.ThrowIfNull(transaction);

    return new TransactionEntity
    {
      Id = transaction.Id,
      AccountId = transaction.AccountId,
      Amount = transaction.Amount,
      Type = transaction.Type,
      Direction = transaction.Direction,
      Timestamp = transaction.Timestamp
    };
  }
}
