using BankingApi.EventReceiver.Application;
using Microsoft.Extensions.Hosting;

namespace BankingApi.EventReceiver;

public class BankingBackgroundService: BackgroundService
{
  private readonly IBankingApplication _bankingApplication;

  public BankingBackgroundService(IBankingApplication bankingApplication)
  {
    _bankingApplication = bankingApplication;
  }

  protected override Task ExecuteAsync(CancellationToken stoppingToken)
  {
    return _bankingApplication.ProcessAsync(stoppingToken);
  }
}
