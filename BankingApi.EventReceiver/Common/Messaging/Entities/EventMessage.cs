namespace BankingApi.EventReceiver.Common.Messaging.Entities;

public class EventMessage
{
  public Guid Id { get; set; }
  public string? MessageBody { get; set; }
  public int ProcessingCount { get; set; }
}
