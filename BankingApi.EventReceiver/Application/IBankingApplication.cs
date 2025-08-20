
namespace BankingApi.EventReceiver.Application;

public interface IBankingApplication
{
  Task ProcessAsync(CancellationToken cancellationToken);

}
