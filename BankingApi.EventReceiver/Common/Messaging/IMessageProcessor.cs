namespace BankingApi.EventReceiver.Common.Messaging;

public interface IMessageProcessor<out TMessage>
{
  Task ProcessNextAsync(Func<TMessage, CancellationToken, Task<MessageProcessingResult>> handler, Func<MessageProcessingResult> onParsingFailure, CancellationToken cancellationToken);
}
