using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Data.Configurations;

public class InboxConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages", "public");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasComment("Уникальный идентификатор записи о полученном сообщении");

        builder.Property(i => i.EventId)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Уникальный идентификатор события (используется для дедупликации)");

        builder.Property(i => i.Topic)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Название топика Kafka, из которого получено сообщение");

        builder.Property(i => i.Payload)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasComment("Тело сообщения в формате JSON");

        builder.Property(i => i.ProcessedAt)
            .IsRequired()
            .HasComment("Дата и время обработки сообщения");

        builder.HasIndex(i => new { i.EventId, i.Topic })
            .IsUnique()
            .HasDatabaseName("IX_InboxMessages_EventId_Topic");
    }
}