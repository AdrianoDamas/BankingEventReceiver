using System;
using BankingApi.EventReceiver.Application.Processors;
using BankingApi.EventReceiver.Application.Repositories;
using BankingApi.EventReceiver.Domain.Enums;
using BankingApi.EventReceiver.Domain.Models;
using BankingApi.EventReceiver.Domain.ValueObjects;
using NSubstitute;

namespace BankingApi.EventReceiver.UnitTests.Application.Processors;

public class TransactionProcessorTests
{
  [Fact]
  public async Task ProcessAsync_ShouldSucceed_WhenNoErrorOccursDuringProcessing()
  {
    // Arrange
    var account = new Account(Guid.NewGuid(), 200.00m, [1, 0, 0, 0]);
    var transaction = new Transaction(Guid.NewGuid(), account.Id, 100, TransactionType.Regular, TransactionDirection.Credit, DateTimeOffset.UtcNow);

    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    IAccountRepository accountRepositoryMock = Substitute.For<IAccountRepository>();
    accountRepositoryMock.GetByIdAsync(transaction.AccountId, cancellationTokenSource.Token)
      .Returns(Task.FromResult(account));

    accountRepositoryMock.UpdateAsync(account, cancellationTokenSource.Token)
      .Returns(Task.FromResult(account));

    var processor = new TransactionProcessor(accountRepositoryMock);

    // Act
    await processor.ProcessAsync(transaction, cancellationTokenSource.Token);

    // Assert
    Assert.Equal(300.00m, account.Balance);
    Assert.Single(account.PendingTransactions);
    Assert.Equal(transaction, account.PendingTransactions.First());
    await accountRepositoryMock.Received(1).GetByIdAsync(transaction.AccountId, cancellationTokenSource.Token);
    await accountRepositoryMock.Received(1).UpdateAsync(account, cancellationTokenSource.Token);
    Assert.Equal(2, accountRepositoryMock.ReceivedCalls().Count());
  }

  [Fact]
  public async Task ProcessAsync_ShouldReThrow_WhenRepositoryThrows_WhenUpdatingAccount()
  {
    // Arrange
    var account = new Account(Guid.NewGuid(), 200.00m, [1, 0, 0, 0]);
    var transaction = new Transaction(Guid.NewGuid(), account.Id, 100, TransactionType.Regular, TransactionDirection.Credit, DateTimeOffset.UtcNow);

    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    IAccountRepository accountRepositoryMock = Substitute.For<IAccountRepository>();
    accountRepositoryMock.GetByIdAsync(transaction.AccountId, cancellationTokenSource.Token)
      .Returns(Task.FromResult(account));

    var expectedException = new Exception("Repository error");
    accountRepositoryMock.UpdateAsync(account, cancellationTokenSource.Token)
      .Returns(Task.FromException<Account>(expectedException));

    var processor = new TransactionProcessor(accountRepositoryMock);

    // Act
    var exception = await Assert.ThrowsAsync<Exception>(() => processor.ProcessAsync(transaction, cancellationTokenSource.Token));

    // Assert
    Assert.Equal("Repository error", exception.Message);
    Assert.Same(expectedException, exception);
    await accountRepositoryMock.Received(1).GetByIdAsync(transaction.AccountId, cancellationTokenSource.Token);
    await accountRepositoryMock.Received(1).UpdateAsync(account, cancellationTokenSource.Token);
    Assert.Equal(2, accountRepositoryMock.ReceivedCalls().Count());
  }

  [Fact]
  public async Task ProcessAsync_ShouldReThrow_WhenRepositoryThrows_WhenGettingAccount()
  {
    // Arrange
    var account = new Account(Guid.NewGuid(), 200.00m, [1, 0, 0, 0]);
    var transaction = new Transaction(Guid.NewGuid(), account.Id, 100, TransactionType.Regular, TransactionDirection.Credit, DateTimeOffset.UtcNow);

    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    var expectedException = new Exception("Repository error");

    IAccountRepository accountRepositoryMock = Substitute.For<IAccountRepository>();
    accountRepositoryMock.GetByIdAsync(transaction.AccountId, cancellationTokenSource.Token)
      .Returns(Task.FromException<Account>(expectedException));

    var processor = new TransactionProcessor(accountRepositoryMock);

    // Act
    var exception = await Assert.ThrowsAsync<Exception>(() => processor.ProcessAsync(transaction, cancellationTokenSource.Token));

    // Assert
    Assert.Equal("Repository error", exception.Message);
    await accountRepositoryMock.Received(1).GetByIdAsync(transaction.AccountId, cancellationTokenSource.Token);
    Assert.Single(accountRepositoryMock.ReceivedCalls());
  }

}
