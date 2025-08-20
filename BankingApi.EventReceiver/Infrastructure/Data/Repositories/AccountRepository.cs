using BankingApi.EventReceiver.Application.Repositories;
using BankingApi.EventReceiver.Domain.Exceptions;
using BankingApi.EventReceiver.Domain.Models;
using BankingApi.EventReceiver.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankingApi.EventReceiver.Infrastructure.Data.Repositories;

public class AccountRepository : IAccountRepository
{
  private readonly BankingApiDbContext _dbContext;

  public AccountRepository(BankingApiDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<Account> GetByIdAsync(Guid accountId, CancellationToken cancellationToken)
  {
    var entity = await _dbContext.BankAccounts
      .AsNoTracking()
      .SingleOrDefaultAsync(a => a.Id == accountId, cancellationToken) ?? throw new ResourceNotFoundException($"Account with ID {accountId} not found.");

    return entity.ToModel();
  }

  public async Task<Account> UpdateAsync(Account account, CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(account);

    await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

    // Load tracked entity of the account
    var accountEntity = await _dbContext.BankAccounts
      .SingleOrDefaultAsync(a => a.Id == account.Id, cancellationToken)
      ?? throw new ResourceNotFoundException($"Account with ID {account.Id} not found.");

    // Ensure row version matches the current model version. Otherwise fail immediately
    if (accountEntity.RowVersion is null || !accountEntity.RowVersion.SequenceEqual(account.Version))
    {
      throw new ConflictException("The account has been modified by another process. Please reload and try again.");
    }

    // Persist all pending transactions (on by one, to catch any duplicates). We assume that there will be only one pending transaction (at least according to the current requirements)
    foreach (var transaction in account.PendingTransactions)
    {
      var transactionEntity = TransactionEntity.FromModel(transaction);
      _dbContext.Transactions.Add(transactionEntity);

      try
      {
        await _dbContext.SaveChangesAsync(cancellationToken);
      }
      catch (DbUpdateException ex) when (IsUniqueViolation(ex))
      {
        throw new DuplicateTransactionException("Transaction with the same ID already exists.", transaction.Id, ex);
      }
    }

    // Update the account with optimistic concurrency
    accountEntity.UpdateFromModel(account);
    try
    {
      await _dbContext.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException ex)
    {
      throw new ConflictException("The account has been modified by another process. Please reload and try again.", ex);
    }

    // Commit transaction
    await dbTransaction.CommitAsync(cancellationToken);

    // Return the updated model
    return accountEntity.ToModel();
  }

  static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx
           && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
}
