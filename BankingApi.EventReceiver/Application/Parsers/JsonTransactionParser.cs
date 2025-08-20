using System.Text.Json;
using System.Text.Json.Serialization;
using BankingApi.EventReceiver.Application.DTOs;
using BankingApi.EventReceiver.Domain.Enums;
using BankingApi.EventReceiver.Domain.Exceptions;
using BankingApi.EventReceiver.Domain.ValueObjects;
using BankingApi.EventReceiver.Common.Messaging;

namespace BankingApi.EventReceiver.Application.Parsers;

public class JsonTransactionParser : IMessageParser<Transaction>
{
  // TODO: this might be configured globally
  private static readonly JsonSerializerOptions Options = new()
  {
    PropertyNameCaseInsensitive = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString
  };

  public Transaction Parse(string message)
  {
    if (string.IsNullOrWhiteSpace(message))
    {
      throw new ValidationException("Message cannot be null or empty.");
    }

    TransactionDto? transactionDto;
    try
    {
      transactionDto = JsonSerializer.Deserialize<TransactionDto>(message, Options);
    }
    catch (JsonException ex)
    {
      throw new ValidationException("Failed to parse the transaction message.", ex);
    }

    if (transactionDto is null)
    {
      throw new ValidationException("Transaction message is not in the expected format.");
    }

    var direction = transactionDto.MessageType?.Trim().ToLowerInvariant() switch
    {
      "credit" => TransactionDirection.Credit,
      "debit" => TransactionDirection.Debit,
      _ => throw new ValidationException($"Unknown transaction type: {transactionDto.MessageType}")
    };

    return new Transaction(
      id: transactionDto.Id,
      accountId: transactionDto.BankAccountId,
      amount: transactionDto.Amount,
      type: TransactionType.Regular,
      direction: direction,
      timestamp: DateTimeOffset.UtcNow // Use current time for the transaction timestamp, since the current DTO object does not have it
    );
  }
}
