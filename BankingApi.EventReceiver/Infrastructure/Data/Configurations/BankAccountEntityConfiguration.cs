using System;
using BankingApi.EventReceiver.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingApi.EventReceiver.Infrastructure.Data.Configurations;

public class BankAccountEntityConfiguration : IEntityTypeConfiguration<BankAccountEntity>
{
  public void Configure(EntityTypeBuilder<BankAccountEntity> builder)
  {
    builder.ToTable("BankAccounts");

    builder.Property(ba => ba.Id)
      .HasColumnType("uniqueidentifier")
      .IsRequired();
    builder.HasKey(ba => ba.Id);

    builder.Property(ba => ba.Balance)
      .IsRequired()
      .HasColumnType("decimal(18,2)");

    builder.Property(ba => ba.RowVersion)
      .IsRequired()
      .IsRowVersion();
  }
}
