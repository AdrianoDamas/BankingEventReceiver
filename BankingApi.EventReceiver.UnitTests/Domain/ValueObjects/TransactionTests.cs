using BankingApi.EventReceiver.Domain.Enums;
using BankingApi.EventReceiver.Domain.Exceptions;
using BankingApi.EventReceiver.Domain.ValueObjects;

namespace BankingApi.EventReceiver.UnitTests.Domain.ValueObjects;

public class TransactionTests
{
  [Fact]
  public void Constructor_ShouldInitializeWithCorrectValues()
  {
    // Arrange
    var id = Guid.NewGuid();
    var accountId = Guid.NewGuid();
    decimal amount = 100.00m;
    var type = TransactionType.Regular;
    var direction = TransactionDirection.Credit;
    var timestamp = DateTimeOffset.UtcNow;

    // Act
    var transaction = new Transaction(id, accountId, amount, type, direction, timestamp);

    // Assert
    Assert.Equal(id, transaction.Id);
    Assert.Equal(accountId, transaction.AccountId);
    Assert.Equal(amount, transaction.Amount);
    Assert.Equal(type, transaction.Type);
    Assert.Equal(direction, transaction.Direction);
    Assert.Equal(timestamp, transaction.Timestamp);
  }

  [Fact]
  public void Constructor_ShouldThrowValidationException_WhenIdIsEmpty()
  {
    // Arrange
    var accountId = Guid.NewGuid();
    decimal amount = 100.00m;
    var type = TransactionType.Regular;
    var direction = TransactionDirection.Credit;
    var timestamp = DateTimeOffset.UtcNow;

    // Act & Assert
    var exception = Assert.Throws<ValidationException>(() => new Transaction(Guid.Empty, accountId, amount, type, direction, timestamp));
    Assert.Equal("Transaction ID cannot be empty.", exception.Message);
  }

  [Fact]
  public void Constructor_ShouldThrowValidationException_WhenAccountIdIsEmpty()
  {
    // Arrange
    var id = Guid.NewGuid();
    decimal amount = 100.00m;
    var type = TransactionType.Regular;
    var direction = TransactionDirection.Credit;
    var timestamp = DateTimeOffset.UtcNow;

    // Act & Assert
    var exception = Assert.Throws<ValidationException>(() => new Transaction(id, Guid.Empty, amount, type, direction, timestamp));
    Assert.Equal("Account ID cannot be empty.", exception.Message);
  }

  [Fact]
  public void Constructor_ShouldThrowValidationException_WhenAmountIsNegative()
  {
    // Arrange
    var id = Guid.NewGuid();
    var accountId = Guid.NewGuid();
    decimal amount = -100.00m;
    var type = TransactionType.Regular;
    var direction = TransactionDirection.Credit;
    var timestamp = DateTimeOffset.UtcNow;

    // Act & Assert
    var exception = Assert.Throws<ValidationException>(() => new Transaction(id, accountId, amount, type, direction, timestamp));
    Assert.Equal("Transaction amount must be a positive value greater than zero.", exception.Message);
  }

  [Fact]
  public void Constructor_ShouldThrowValidationException_WhenAmountIsZero()
  {
    // Arrange
    var id = Guid.NewGuid();
    var accountId = Guid.NewGuid();
    decimal amount = 0.00m;
    var type = TransactionType.Regular;
    var direction = TransactionDirection.Credit;
    var timestamp = DateTimeOffset.UtcNow;

    // Act & Assert
    var exception = Assert.Throws<ValidationException>(() => new Transaction(id, accountId, amount, type, direction, timestamp));
    Assert.Equal("Transaction amount must be a positive value greater than zero.", exception.Message);
  }

}
