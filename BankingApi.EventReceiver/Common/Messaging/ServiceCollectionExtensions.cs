using Microsoft.Extensions.DependencyInjection;

namespace BankingApi.EventReceiver.Common.Messaging;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddMessagingServices(this IServiceCollection services)
  {
    services.AddScoped(typeof(IMessageProcessor<>), typeof(MessageProcessor<>));

    return services;
  }

}
