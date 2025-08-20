using BankingApi.EventReceiver.Common.Messaging.Entities;

namespace BankingApi.EventReceiver.Common.Messaging
{
  public interface IServiceBusReceiver
  {
    Task<EventMessage?> Peek();

    Task Abandon(EventMessage message);

    Task Complete(EventMessage message);
    Task ReSchedule(EventMessage message, DateTime nextAvailableTime);
    Task MoveToDeadLetter(EventMessage message);
  }
}
