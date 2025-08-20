namespace BankingApi.EventReceiver.Application.DTOs;

internal sealed record class TransactionDto(Guid Id, string MessageType, Guid BankAccountId, decimal Amount);
