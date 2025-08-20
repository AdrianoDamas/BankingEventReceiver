using BankingApi.EventReceiver.Common.Messaging.Entities;
using Microsoft.Extensions.Logging;

namespace BankingApi.EventReceiver.Common.Messaging;

public class MessageProcessor<TMessage> : IMessageProcessor<TMessage>
{
  // TODO: these configs can be extracted to options
  private static readonly TimeSpan NoMessageDelay = TimeSpan.FromSeconds(10);
  private static readonly TimeSpan PeekTransientDelay = TimeSpan.FromSeconds(5);

  private static readonly TimeSpan[] Backoff = [
    TimeSpan.FromSeconds(5),
    TimeSpan.FromSeconds(25),
    TimeSpan.FromSeconds(125)
  ];

  private readonly IMessageParser<TMessage> _messageParser;
  private readonly IServiceBusReceiver _serviceBusReceiver;
  private readonly ILogger<MessageProcessor<TMessage>> _logger;


  public MessageProcessor(
    IMessageParser<TMessage> messageParser,
    IServiceBusReceiver serviceBusReceiver,
    ILogger<MessageProcessor<TMessage>> logger)
  {
    _messageParser = messageParser;
    _serviceBusReceiver = serviceBusReceiver;
    _logger = logger;
  }

  public async Task ProcessNextAsync(
    Func<TMessage, CancellationToken, Task<MessageProcessingResult>> handler,
    Func<MessageProcessingResult> onParsingFailure,
    CancellationToken cancellationToken)
  {
    if (handler is null)
    {
      throw new InvalidOperationException("handler cannot be null.");
    }
    if (onParsingFailure is null)
    {
      throw new InvalidOperationException("onParsingFailure cannot be null.");
    }

    var message = await GetNextMessageAsync(cancellationToken);

    var result = await ProcessMessageAsync(message, handler, onParsingFailure, cancellationToken);
    await HandleResultAsync(result, message, cancellationToken);
  }

  private async Task<EventMessage> GetNextMessageAsync(CancellationToken cancellationToken)
  {
    while (true)
    {
      cancellationToken.ThrowIfCancellationRequested();

      try
      {
        var message = await _serviceBusReceiver.Peek();
        if (message is not null)
        {
          return message;
        }
        else
        {
          _logger.LogInformation("No messages to process. Waiting for {DelaySeconds} seconds...", NoMessageDelay.TotalSeconds);
          await Task.Delay(NoMessageDelay, cancellationToken);
        }
      }
      catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
      {
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Failed to receive message. Retrying in {DelaySeconds} seconds...", PeekTransientDelay.TotalSeconds);
        await Task.Delay(PeekTransientDelay, cancellationToken);
      }
    }
  }

  private async Task<MessageProcessingResult> ProcessMessageAsync(
    EventMessage message,
    Func<TMessage, CancellationToken, Task<MessageProcessingResult>> handler,
    Func<MessageProcessingResult> onParsingFailure,
    CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(message.MessageBody))
    {
      var result = onParsingFailure.Invoke();
      _logger.LogWarning("Failed to parse the message payload: it is null or empty. Message {MessageId} will be marked as {Result}", message.Id, result);
      return result;
    }

    _logger.LogInformation("Parsing message: {MessageId}", message.Id);

    TMessage parsedMessage;
    try
    {
      parsedMessage = _messageParser.Parse(message.MessageBody);
    }
    catch (Exception ex)
    {
      var result = onParsingFailure.Invoke();
      _logger.LogWarning(ex, "Failed to parse the message payload. Message {MessageId} will be marked as {Result}", message.Id, result);
      return result;
    }

    try
    {
      return await handler.Invoke(parsedMessage, cancellationToken);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Unhandled exception occurred when trying to process message: {MessageId}. Message will be retried.", message.Id);
      return MessageProcessingResult.TransientFailure;
    }
  }

  private async Task HandleResultAsync(MessageProcessingResult result, EventMessage message, CancellationToken cancellationToken)
  {
    switch (result)
    {
      case MessageProcessingResult.Success:
        await _serviceBusReceiver.Complete(message);
        _logger.LogInformation("Successfully processed message: {MessageId}, result: {Result}", message.Id, result);
        break;

      case MessageProcessingResult.PermanentFailure:
        await _serviceBusReceiver.MoveToDeadLetter(message);
        _logger.LogWarning("Permanent failure occurred while processing message: {MessageId}. Message moved to dead letter.", message.Id);
        break;

      case MessageProcessingResult.TransientFailure:
        await ReScheduleWithBackoffAsync(message);
        break;

      default:
        throw new InvalidOperationException("Unknown message processing result.");
    }
  }

  private async Task ReScheduleWithBackoffAsync(EventMessage message)
  {
    var attempt = Math.Max(0, message.ProcessingCount);

    if (attempt >= Backoff.Length)
    {
      _logger.LogWarning("Maximum retry attempts reached for message {MessageId}. Moving to dead letter.", message.Id);
      await _serviceBusReceiver.MoveToDeadLetter(message);
      return;
    }

    var next = DateTime.UtcNow.Add(Backoff[attempt]);
    await _serviceBusReceiver.ReSchedule(message, next);

    _logger.LogInformation("Failed to process message {MessageId}. It will be retried at {RetryTime}. Attempt: {Attempt}", message.Id, next, attempt + 1);
  }

}
