using BankingApi.EventReceiver.Application;
using BankingApi.EventReceiver.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApi.EventReceiver;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddBankingServices(this IServiceCollection services)
  {
    services.AddBankingApplicationServices();
    services.AddBankingRepositoryServices();

    return services;
  }

}
