using BankingApi.EventReceiver.Application.Repositories;
using BankingApi.EventReceiver.Infrastructure.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApi.EventReceiver.Infrastructure.Data;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddBankingRepositoryServices(this IServiceCollection services)
  {
    services.AddScoped<IAccountRepository, AccountRepository>();
    services.AddDbContext<BankingApiDbContext>();

    return services;
  }

}
