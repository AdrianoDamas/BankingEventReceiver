using BankingApi.EventReceiver.Domain.Enums;
using BankingApi.EventReceiver.Domain.Exceptions;
using BankingApi.EventReceiver.Domain.Models;
using BankingApi.EventReceiver.Domain.ValueObjects;

namespace BankingApi.EventReceiver.UnitTests.Domain.Models;

public class AccountTests
{
  [Fact]
  public void Constructor_ShouldInitializeWithCorrectValues()
  {
    // Arrange
    var id = Guid.NewGuid();
    decimal balance = 1000.00m;
    var version = new byte[] { 1, 0, 0, 0 };

    // Act
    var account = new Account(id, balance, version);

    // Assert
    Assert.Equal(id, account.Id);
    Assert.Equal(balance, account.Balance);
    Assert.Equal(version, account.Version);
    Assert.Empty(account.PendingTransactions);
  }

  [Fact]
  public void Credit_ShouldIncreaseBalance_WhenAmountIsPositive()
  {
    // Arrange
    var account = new Account(Guid.NewGuid(), 1000.00m, [1, 0, 0, 0]);
    decimal creditAmount = 200.00m;

    // Act
    account.Credit(creditAmount);

    // Assert
    Assert.Equal(1200.00m, account.Balance);
  }

  [Fact]
  public void Debit_ShouldDecreaseBalance_WhenAmountIsPositive()
  {
    // Arrange
    var account = new Account(Guid.NewGuid(), 1000.00m, [1, 0, 0, 0]);
    decimal debitAmount = 200.00m;

    // Act
    account.Debit(debitAmount);

    // Assert
    Assert.Equal(800.00m, account.Balance);
  }

  [Fact]
  public void Credit_ShouldThrowValidationException_WhenAmountIsNegative()
  {
    // Arrange
    var account = new Account(Guid.NewGuid(), 1000.00m, [1, 0, 0, 0]);
    decimal creditAmount = -200.00m;

    // Act & Assert
    var exception = Assert.Throws<ValidationException>(() => account.Credit(creditAmount));
    Assert.Equal("Amount must be greater than zero.", exception.Message);
  }

  [Fact]
  public void Debit_ShouldThrowValidationException_WhenAmountIsNegative()
  {
    // Arrange
    var account = new Account(Guid.NewGuid(), 1000.00m, [1, 0, 0, 0]);
    decimal debitAmount = -200.00m;

    // Act & Assert
    var exception = Assert.Throws<ValidationException>(() => account.Debit(debitAmount));
    Assert.Equal("Amount must be greater than zero.", exception.Message);
  }

  [Theory]
  [InlineData(200.00, TransactionDirection.Credit, TransactionType.Regular, 400)]
  [InlineData(100.00, TransactionDirection.Debit, TransactionType.Regular, 100)]
  [InlineData(50.00, TransactionDirection.Credit, TransactionType.Reconciliation, 250)]
  [InlineData(25.00, TransactionDirection.Debit, TransactionType.Reconciliation, 175)]
  public void ApplyTransaction_ShouldAddTransactionToPending_WhenTransactionIsValid(decimal amount, TransactionDirection direction, TransactionType type, decimal expectedBalance)
  {
    // Arrange
    var account = new Account(Guid.NewGuid(), 200.00m, [1, 0, 0, 0]);
    var transaction = new Transaction(Guid.NewGuid(), account.Id, amount, type, direction, DateTimeOffset.UtcNow);

    // Act
    account.ApplyTransaction(transaction);

    // Assert
    Assert.Single(account.PendingTransactions);
    Assert.Equal(transaction, account.PendingTransactions.First());
    Assert.Equal(expectedBalance, account.Balance);
  }

  [Fact]
  public void ApplyTransaction_ShouldThrowValidationException_WhenTransactionAlreadyApplied()
  {
    // Arrange
    var account = new Account(Guid.NewGuid(), 200.00m, [1, 0, 0, 0]);
    var transaction = new Transaction(Guid.NewGuid(), account.Id, 100.00m, TransactionType.Regular, TransactionDirection.Credit, DateTimeOffset.UtcNow);
    account.ApplyTransaction(transaction);

    // Act & Assert
    var exception = Assert.Throws<ValidationException>(() => account.ApplyTransaction(transaction));
    Assert.Equal("Transaction with the same ID already exists in pending transactions.", exception.Message);

    Assert.Single(account.PendingTransactions);
    Assert.Equal(transaction, account.PendingTransactions.First());
    Assert.Equal(300.00m, account.Balance);
  }

  [Fact]
  public void ApplyTransaction_ShouldThrowValidationException_WhenTransactionDoesNotBelongToAccount()
  {
    // Arrange
    var account = new Account(Guid.NewGuid(), 200.00m, [1, 0, 0, 0]);
    var transaction = new Transaction(Guid.NewGuid(), Guid.NewGuid(), 100.00m, TransactionType.Regular, TransactionDirection.Credit, DateTimeOffset.UtcNow);

    // Act & Assert
    var exception = Assert.Throws<ValidationException>(() => account.ApplyTransaction(transaction));
    Assert.Equal("Transaction does not belong to this account.", exception.Message);
  }
}
