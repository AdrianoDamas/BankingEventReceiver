using BankingApi.EventReceiver.Application.Processors;
using BankingApi.EventReceiver.Domain.Exceptions;
using BankingApi.EventReceiver.Domain.ValueObjects;
using BankingApi.EventReceiver.Common.Messaging;
using Microsoft.Extensions.Logging;

namespace BankingApi.EventReceiver.Application;

public class BankingApplication : IBankingApplication
{
  private static readonly TimeSpan UnknownFailureDelay = TimeSpan.FromSeconds(10);
  private readonly ITransactionProcessor _transactionProcessor;
  private readonly IMessageProcessor<Transaction> _messageProcessor;

  private readonly ILogger<BankingApplication> _logger;
  public BankingApplication(
    ILogger<BankingApplication> logger,
    ITransactionProcessor transactionProcessor,
    IMessageProcessor<Transaction> messageProcessor)
  {
    _transactionProcessor = transactionProcessor;
    _messageProcessor = messageProcessor;

    _logger = logger;
  }

  public async Task ProcessAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Starting message processing...");

    while (!cancellationToken.IsCancellationRequested)
    {
      try
      {
        await _messageProcessor.ProcessNextAsync(
          handler: ProcessMessageAsync,
          onParsingFailure: () => MessageProcessingResult.PermanentFailure,
          cancellationToken);
      }
      catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
      {
        _logger.LogInformation("Message processing was cancelled.");
        break;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An unexpected error occurred during message processing. Retrying in {Delay} seconds...", UnknownFailureDelay.TotalSeconds);
        await Task.Delay(UnknownFailureDelay, cancellationToken);
      }
    }
  }

  private async Task<MessageProcessingResult> ProcessMessageAsync(Transaction transaction, CancellationToken cancellationToken)
  {
    try
    {
      _logger.LogInformation("Processing transaction: {TransactionId}", transaction.Id);

      await _transactionProcessor.ProcessAsync(transaction, cancellationToken);

      _logger.LogInformation("Successfully processed transaction: {TransactionId}", transaction.Id);
      return MessageProcessingResult.Success;
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      throw;
    }
    catch (Exception ex)
    {
      return HandleException(ex, transaction);
    }
  }

  private MessageProcessingResult HandleException(Exception ex, Transaction transaction)
  {
    switch (ex)
    {
      case DuplicateTransactionException dup:
        _logger.LogInformation(dup, "Duplicate transaction detected. It will be completed without further processing. Transaction ID: {TransactionId}", transaction.Id);
        return MessageProcessingResult.Success;

      case ValidationException ve:
        _logger.LogWarning(ve, "Validation error while processing transaction. It will be moved to dead letter. Transaction ID: {TransactionId}", transaction.Id);
        return MessageProcessingResult.PermanentFailure;

      case ResourceNotFoundException nf:
        _logger.LogWarning(nf, "Resource not found while processing transaction. It will be moved to dead letter. Transaction ID: {TransactionId}", transaction.Id);
        return MessageProcessingResult.PermanentFailure;

      case ConflictException cx:
        _logger.LogWarning(cx, "Conflict detected while processing transaction. It will be processed again later. Transaction ID: {TransactionId}", transaction.Id);
        return MessageProcessingResult.TransientFailure;

      default:
        _logger.LogError(ex, "Unexpected error occurred. Transaction will be processed again later. TransactionId: {TransactionId}", transaction.Id);
        return MessageProcessingResult.TransientFailure;
    }
  }
}
