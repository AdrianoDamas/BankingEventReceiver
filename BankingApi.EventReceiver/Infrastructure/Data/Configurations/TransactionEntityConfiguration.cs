using BankingApi.EventReceiver.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingApi.EventReceiver.Infrastructure.Data.Configurations;

public class TransactionEntityConfiguration : IEntityTypeConfiguration<TransactionEntity>
{
  public void Configure(EntityTypeBuilder<TransactionEntity> builder)
  {
    builder.ToTable("Transactions");

    builder.Property(t => t.Id)
      .HasColumnType("uniqueidentifier")
      .IsRequired();
    builder.HasKey(t => t.Id);

    builder.Property(t => t.AccountId)
      .HasColumnType("uniqueidentifier")
      .IsRequired();

    builder.Property(t => t.Amount)
      .HasColumnType("decimal(18,2)")
      .IsRequired();

    builder.Property(t => t.Type)
      .HasConversion<byte>()
      .IsRequired();

    builder.Property(t => t.Direction)
      .HasConversion<byte>()
      .IsRequired();

    builder.Property(t => t.Timestamp)
      .HasColumnType("datetimeoffset(7)")
      .IsRequired();
  }
}
