using BankingApi.EventReceiver.Domain.Enums;
using BankingApi.EventReceiver.Domain.Exceptions;

namespace BankingApi.EventReceiver.Domain.ValueObjects;

public sealed record class Transaction
{
  public Guid Id { get; }
  public Guid AccountId { get; }
  public decimal Amount { get; }
  public TransactionType Type { get; }
  public TransactionDirection Direction { get; }
  public DateTimeOffset Timestamp { get; }

  public Transaction(Guid id, Guid accountId, decimal amount, TransactionType type, TransactionDirection direction, DateTimeOffset timestamp)
  {
    Validate(id, accountId, amount);

    Id = id;
    AccountId = accountId;
    Amount = amount;
    Type = type;
    Direction = direction;
    Timestamp = timestamp;
  }

  private static void Validate(Guid id, Guid accountId, decimal amount)
  {
    if (id == Guid.Empty)
    {
      throw new ValidationException("Transaction ID cannot be empty.");
    }

    if (accountId == Guid.Empty)
    {
      throw new ValidationException("Account ID cannot be empty.");
    }

    if (amount <= 0)
    {
      throw new ValidationException("Transaction amount must be a positive value greater than zero.");
    }
  }
}
