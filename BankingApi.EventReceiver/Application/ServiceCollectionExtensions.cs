using BankingApi.EventReceiver.Application.Parsers;
using BankingApi.EventReceiver.Application.Processors;
using BankingApi.EventReceiver.Common.Messaging;
using BankingApi.EventReceiver.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApi.EventReceiver.Application;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddBankingApplicationServices(this IServiceCollection services)
  {
    services.AddScoped<IBankingApplication, BankingApplication>();
    services.AddScoped<ITransactionProcessor, TransactionProcessor>();
    services.AddScoped<IMessageParser<Transaction>, JsonTransactionParser>();
    services.AddMessagingServices();

    return services;
  }

}
