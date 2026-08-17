using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Data.Configurations;

public class OutboxConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", "public");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasComment("Уникальный идентификатор сообщения");

        builder.Property(o => o.Topic)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Название топика Kafka, куда должно быть отправлено сообщение");

        builder.Property(o => o.Key)
            .HasMaxLength(100)
            .HasComment("Ключ сообщения для Kafka (используется для партиционирования)");

        builder.Property(o => o.Payload)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasComment("Тело сообщения в формате JSON");

        builder.Property(o => o.CreatedAt)
            .IsRequired()
            .HasComment("Дата и время создания сообщения");

        builder.Property(o => o.ProcessedAt)
            .HasComment("Дата и время успешной отправки сообщения в Kafka");

        builder.Property(o => o.Attempts)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Количество попыток отправки сообщения");

        builder.HasIndex(o => o.ProcessedAt)
            .HasDatabaseName("IX_OutboxMessages_ProcessedAt")
            .HasFilter("\"ProcessedAt\" IS NULL");
    }
}