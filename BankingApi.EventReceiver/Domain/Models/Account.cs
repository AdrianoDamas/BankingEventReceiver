using BankingApi.EventReceiver.Domain.Enums;
using BankingApi.EventReceiver.Domain.Exceptions;
using BankingApi.EventReceiver.Domain.ValueObjects;

namespace BankingApi.EventReceiver.Domain.Models;

public sealed class Account
{
  private readonly List<Transaction> _pendingTransactions;

  public Guid Id { get; }
  public decimal Balance { get; private set; }

  public IReadOnlyList<byte> Version { get; }

  public IReadOnlyCollection<Transaction> PendingTransactions => _pendingTransactions.AsReadOnly();

  public Account(Guid id, decimal currentBalance, IReadOnlyList<byte> version)
  {
    Id = id;
    Balance = currentBalance;
    _pendingTransactions = [];
    Version = version ?? throw new ValidationException("Row version cannot be null.");
  }

  public void Credit(decimal amount)
  {
    EnsurePositive(amount);
    Balance += amount;
  }

  public void Debit(decimal amount)
  {
    EnsurePositive(amount);
    Balance -= amount;
  }

  public void ApplyTransaction(Transaction transaction)
  {
    if (transaction.AccountId != Id)
    {
      throw new ValidationException("Transaction does not belong to this account.");
    }

    if(PendingTransactions.Any(t => t.Id == transaction.Id))
    {
      throw new ValidationException("Transaction with the same ID already exists in pending transactions.");
    }

    _pendingTransactions.Add(transaction);

    switch (transaction.Direction)
    {
      case TransactionDirection.Credit:
        Credit(transaction.Amount);
        break;
      case TransactionDirection.Debit:
        Debit(transaction.Amount);
        break;
      default:
        throw new ValidationException("Unknown transaction type.");
    }
  }

  private static void EnsurePositive(decimal amount)
  {
    if (amount <= 0)
    {
      throw new ValidationException("Amount must be greater than zero.");
    }
  }
}
