namespace BankingApi.EventReceiver.Domain.Exceptions;

public class DuplicateTransactionException : Exception
{
  public Guid TransactionId { get; }

  public DuplicateTransactionException(string message, Guid transactionId) : base(message)
  {
    TransactionId = transactionId;
  }

  public DuplicateTransactionException(string message, Guid transactionId, Exception innerException)
    : base(message, innerException)
  {
    TransactionId = transactionId;
  }
}
