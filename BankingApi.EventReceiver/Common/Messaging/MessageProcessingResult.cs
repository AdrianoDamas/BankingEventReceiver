namespace BankingApi.EventReceiver.Common.Messaging;

public enum MessageProcessingResult
{
  /// <summary>
  /// The message was processed successfully.
  /// </summary>
  Success,

  /// <summary>
  /// The message processing failed due to a transient error and should be retried using the exponential backoff strategy.
  /// </summary>
  TransientFailure,

  /// <summary>
  /// The message processing failed due to a permanent error and should not be retried. Such messages are moved to DLQ
  /// </summary>
  PermanentFailure
}
