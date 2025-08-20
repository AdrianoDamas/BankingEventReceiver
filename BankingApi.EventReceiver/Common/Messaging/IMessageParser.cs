namespace BankingApi.EventReceiver.Common.Messaging;

public interface IMessageParser<out TMessage>
{
  TMessage Parse(string message);
}
